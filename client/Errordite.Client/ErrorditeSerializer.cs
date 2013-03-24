
using System.Text;

#if NET2
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
#else
using System.Web.Script.Serialization;
#endif

namespace Errordite.Client
{
    public static class ErrorditeSerializer
    {
        public static byte[] Serialize(ClientError error)
        {
#if NET2
            return Encoding.UTF8.GetBytes(XmlSerialize(error));
#else
            return Encoding.UTF8.GetBytes(JsonSerialize(error));
#endif
        }

#if NET2
        public static string XmlSerialize(ClientError obj)
        {
            var stringBuilder = new StringBuilder();
            var xmlSerializer = new XmlSerializer(obj.GetType());

            using (var xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
            {
                xmlSerializer.Serialize(xmlTextWriter, obj);
            }

            return stringBuilder.ToString();
        }
#else
        private static string JsonSerialize(ClientError error)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(error);
        }  
#endif
    }
}
