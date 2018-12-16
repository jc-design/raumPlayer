using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace raumPlayer.Helpers
{
    static class HtmlExtension
    {
        public static string StringWithStartSlash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (url.Substring(0, 1) == "/") { return url; }
            else { return "/" + url; }
        }

        public static string StringWithEndSlash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            if (url.Substring(url.Length - 1, 1) != "/")
                return "/";

            return "";
        }

        public static string StringWithoutEndSlash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (url.Substring(url.Length - 1, 1) != "/") { return url; }
            else { return url.Substring(0, url.Length - 1); }
        }

        public static string CompleteHttpString(string ip, int port, string extention)
        {
            string uri;
            if (ip.Substring(0, 7) != "http://") { ip = "http://" + ip; }
            uri = StringWithoutEndSlash(ip) + ":" + port + StringWithStartSlash(extention);
            return uri;
        }

        public static async Task<string> RequestStringAsync(Uri url, Encoding encoding)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                HttpResponseMessage response = await client.SendRequestAsync(request);
                if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                {
                    string xmlResponse = await response.Content.ReadAsStringAsync();
                    return xmlResponse;
                }
                else { return null; }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string BuildAvTransportUri(bool isContainer, string deviceUDN, string containerID = null, int firstItemIndex = 0, string firstItemID = null)
        {
            StringBuilder b = new StringBuilder();
            if (isContainer) { b.Append("dlna-playcontainer://"); }
            else { b.Append("dlna-playsingle://"); }
            b.Append(WebUtility.UrlEncode(deviceUDN));

            b.Append(WebUtility.HtmlEncode("?sid="));
            b.Append(WebUtility.UrlEncode("urn:upnp-org:serviceId:ContentDirectory"));

            if (!string.IsNullOrEmpty(containerID))
            {
                b.Append(WebUtility.HtmlEncode("&cid="));
                b.Append(containerID.Replace("%", "%25").Replace("=", "%3d").Replace(@"/", "%2F"));

                b.Append(WebUtility.HtmlEncode("&md=0"));

                if (!string.IsNullOrEmpty(firstItemID))
                {
                    b.Append(WebUtility.HtmlEncode("&fid="));
                    b.Append(firstItemID.Replace("%", "%25").Replace("=", "%3d").Replace(@"/", "%2F"));
                }
            }

            // When Radio set this
            // Otherwise not
            else if (!string.IsNullOrEmpty(firstItemID))
            {
                b.Append(WebUtility.HtmlEncode("&iid="));
                b.Append(firstItemID.Replace("%", "%25").Replace("=", "%3d").Replace(@"/", "%2F"));
            }

            if (firstItemIndex >= 0)
            {
                b.Append(WebUtility.HtmlEncode("&fii="));
                b.Append(firstItemIndex);
            }

            //if (this.searchQuery != null && !this.searchQuery.isEmpty())
            //{
            //            b.append("&sq=");
            //            b.append(encode(searchQuery.toString()));
            //        }

            //if (this.sortCriteria != null && !this.sortCriteria.isEmpty())
            //{
            //            b.append("&sc=");
            //            b.append(encode(sortCriteria.toString()));
            //        }

            return b.ToString();

        }

        /// <summary>
        /// Escape reserved characters
        /// </summary>
        /// <param name="InString">Input</param>
        /// <returns>Result</returns>
        public static string EscapeString(string InString)
        {
            InString = InString.Replace("&", "&amp;");
            InString = InString.Replace("<", "&lt;");
            InString = InString.Replace(">", "&gt;");
            InString = InString.Replace("\"", "&quot;");
            InString = InString.Replace("'", "&apos;");
            return (InString);
        }
        public static string PartialEscapeString(string InString)
        {
            InString = InString.Replace("\"", "&quot;");
            InString = InString.Replace("'", "&apos;");
            return (InString);
        }
        /// <summary>
        /// Unescapes encoded reserved characters
        /// </summary>
        /// <param name="InString">Input</param>
        /// <returns>Result</returns>
        public static string UnEscapeString(string InString)
        {
            InString = InString.Replace("&lt;", "<");
            InString = InString.Replace("&gt;", ">");
            InString = InString.Replace("&quot;", "\"");
            InString = InString.Replace("&apos;", "'");
            InString = InString.Replace("&amp;", "&");
            return (InString);
        }
    }
}
