using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace raumPlayer.Helpers
{
    public static class XmlExtension
    {
        /// <summary>
        /// Subclass to get UFT8
        /// </summary>
        public class UTF8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        /// <summary>
        /// Serialises class into string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Class instance</param>
        /// <returns>Serialized string</returns>
        public static string UTF8Serialize<T>(this T value)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (StringWriter textWriter = new UTF8StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter))
                    {
                        serializer.Serialize(xmlWriter, value);
                    }
                    return textWriter.ToString();
                }
            }
            catch (Exception exception)
            {
                throw new InvalidDataException("SerializeError".GetLocalized() + "(" + exception.Message + ")");
            }
        }

        /// <summary>
        /// Serialises class into string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Class instance</param>
        /// <returns>Serialized string</returns>
        public static string UTF16Serialize<T>(this T value)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (StringWriter textWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter))
                    {
                        serializer.Serialize(xmlWriter, value);
                    }
                    return textWriter.ToString();
                }
            }
            catch (Exception exception)
            {
                throw new InvalidDataException("SerializeError".GetLocalized() + "(" + exception.Message + ")");
            }
        }

        /// <summary>
        /// Deserialises string back into class instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">XML formatted string</param>
        /// <returns>Deserialized class</returns>
        public static T Deserialize<T>(this string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlReaderSettings settings = new XmlReaderSettings();

                using (StringReader textReader = new StringReader(xml))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                    {
                        return (T)serializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static async Task<T> DeserializeUriAsync<T>(Uri url)
        {
            try
            {
                string response = await HtmlExtension.RequestStringAsync(url, Encoding.UTF8);
                return Deserialize<T>(response);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
