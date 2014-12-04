using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FirmarXades
{
    public static class Serializer
    {
        public static XmlDocument SerializeToXmlDocument(object input, Type type, bool omitXmlDeclaration)
        {
            return SerializeToXmlDocument(input, type, omitXmlDeclaration, new XmlSerializerNamespaces());
        }


        public static XmlDocument SerializeToXmlDocument(object input, Type type, bool omitXmlDeclaration, XmlSerializerNamespaces ns)
        {
            XmlSerializer ser = new XmlSerializer(type);

            XmlDocument xd = null;

            using (MemoryStream memStm = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = omitXmlDeclaration;

                XmlWriter writer = XmlWriter.Create(memStm, settings);

                ser.Serialize(writer, input, ns);
               


                memStm.Position = 0;

                XmlReaderSettings readersettings = new XmlReaderSettings();
                readersettings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, readersettings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }

            return xd;
        }

    }
}
