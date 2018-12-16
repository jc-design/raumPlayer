using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using Windows.Web.Http;

namespace Upnp
{
    public class ServiceAction
    {
        public ServiceAction() { }

        [XmlIgnore]
        public Services Parent { get; set; } = null;

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("argumentList")]
        [XmlArrayItem("argument")]
        public Argument[] ArgumentList { get; set; }

        #region Public Methods

        public void ClearArgumentsValue()
        {
            foreach (Argument arg in ArgumentList)
            {
                arg.Value = null;
            }
        }
        public string GetArgumentValue(string key)
        {
            string result = ArgumentList.Where(arg => arg.Name.ToUpper() == key.ToUpper()).Select(arg => arg.Value).FirstOrDefault();

            if (result != null) { return result; }
            else { throw new Exception("Invalid argument!"); }
        }
        public void SetArgumentValue(string key, string value)
        {
            Argument argument = ArgumentList.Where(arg => arg.Name.ToUpper() == key.ToUpper()).FirstOrDefault();
            StateVariable stateVariable = Parent.ServiceStateTable.Where(s => s.Name.ToUpper() == argument.RelatedStateVariable.ToUpper()).FirstOrDefault();

            if (argument == null || stateVariable == null) { throw new NullReferenceException("SetArgumentValue Argument Error"); }

            uint numberUInt;
            int numberInt;

            //string s = Regex.Replace(stateVariable.DataType.ToLower(), "[0-9]", "");
            switch (Regex.Replace(stateVariable.DataType.ToLower(), "[0-9]", ""))
            {
                case "string" when (stateVariable.AllowedValueList?.Count() ?? 0) == 0:
                    argument.Value = value;
                    break;
                case "string" when (stateVariable.AllowedValueList?.Count() ?? 0) > 0:
                    if (stateVariable.AllowedValueList.Contains(value)) { argument.Value = value; }
                    else { throw new Exception("Invalid argument!"); }
                    break;
                case "ui":
                    if (UInt32.TryParse(value, out numberUInt))
                    {
                        uint min = UInt32.Parse(stateVariable?.AlloweValueRange?.Minimum ?? "0");
                        uint max = UInt32.Parse(stateVariable?.AlloweValueRange?.Maximum ?? "2147483648");

                        if (min <= numberUInt && numberUInt <= max) { argument.Value = value; }
                        else { throw new Exception("Value not in Range"); }
                    }
                    else { throw new Exception("Value is not an UInt"); }
                    break;
                case "i":
                    if (Int32.TryParse(value, out numberInt))
                    {
                        int min = Int32.Parse(stateVariable?.AlloweValueRange?.Minimum ?? "-2147483648");
                        int max = Int32.Parse(stateVariable?.AlloweValueRange?.Maximum ?? "2147483647");

                        if (min <= numberInt && numberInt <= max) { argument.Value = value; }
                        else { throw new Exception("Value not in Range"); }
                    }
                    else { throw new Exception("Value is not an Int"); }
                    break;
                case "boolean":
                    if (value == "0" || value == "1" ) { argument.Value = value; }
                    else { throw new Exception("Value not in Range"); }
                    break;
                default:
                    throw new Exception("Invalid argument!");
                    //break;
            }
        }

        /// <summary>
        /// Invoke Action
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="serviceUrl"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> InvokeAsync(string serviceType, string serviceUrl)
        {
            try
            {
                StringBuilder xml = new StringBuilder();
                xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                xml.Append("<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                xml.Append("<s:Body>");
                xml.Append("<u:" + Name + " xmlns:u=\"" + serviceType + "\">");
                foreach (Argument arg in ArgumentList)
                {
                    if (arg.Direction.ToUpper() == "IN")
                        xml.Append("<" + arg.Name + ">" + arg.Value + "</" + arg.Name + ">");
                }
                xml.Append("</u:" + Name + ">");
                xml.Append("</s:Body>");
                xml.Append("</s:Envelope>");

                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(serviceUrl)))
                    {
                        request.Headers.Add("SOAPACTION", "\"" + serviceType + "#" + Name + "\"");
                        request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                        request.Headers.Add("Accept-Language", "en");
                        request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

                        //request.Method = HttpMethod.Post;
                        using (HttpStringContent requestContent = new HttpStringContent(xml.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8, "text/xml"))
                        {
                            request.Content = requestContent;

                            using (HttpResponseMessage response = await client.SendRequestAsync(request))
                            {
                                if (response.StatusCode == HttpStatusCode.Ok)
                                {
                                    string xmlResponse = await response.Content.ReadAsStringAsync();
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.LoadXml(xmlResponse);

                                    XmlNodeList xmlNodes = xmlDocument.SelectNodes("//*");

                                    var results = ArgumentList.Join(xmlNodes,
                                                                    arg => arg.Name.ToUpper(),
                                                                    xmlNode => xmlNode.NodeName.ToUpper(),
                                                                    (arg, xmlNode) => new { Argument = arg, XmlNode = xmlNode })
                                                              .Where(selection => selection.Argument.Direction.ToUpper() == "OUT");

                                    if (results.Count() != 0)
                                    {
                                        foreach (var item in results)
                                        {
                                            item.Argument.Value = item.XmlNode.InnerText;
                                        }
                                    }

                                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Okay, ActionMessage = string.Empty };
                                }
                                else
                                {
                                    ServiceActionReturnMessage returnMessage = new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error };

                                    string xmlResponse = await response.Content.ReadAsStringAsync();
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.LoadXml(xmlResponse);

                                    XmlNodeList xmlNodes = xmlDocument.SelectNodes("//*");
                                    IXmlNode errorNode = xmlNodes.Select(n => n).Where(n => n.NodeName.ToUpper() == "UPNPERROR").FirstOrDefault();

                                    if (errorNode != null)
                                    {
                                        foreach (var item in errorNode.ChildNodes)
                                        {
                                            switch (item.NodeName.ToUpper())
                                            {
                                                case "ERRORCODE":
                                                    returnMessage.ActionErrorCode = int.Parse(item.InnerText);
                                                    break;
                                                case "ERRORDESCRIPTION":
                                                    returnMessage.ActionMessage = item.InnerText;
                                                    break;
                                            }
                                        }
                                    }

                                    return returnMessage;
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionErrorCode = exception.HResult, ActionMessage = exception.Message };
            }
        }

        #endregion
    }


}
