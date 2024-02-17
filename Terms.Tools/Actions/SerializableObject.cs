using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Terms.Tools.Actions
{
    public static class SerializableObject
    {
        public static T Open<T>(string fileName)
        {
            T returnObject = default(T);

            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(fileName);

                    string outerXml = xmlDocument.OuterXml;

                    using (StringReader stringReader = new StringReader(outerXml))
                    {
                        Type loadType = typeof(T);
                        XmlSerializer xmlSerializer = new XmlSerializer(loadType);

                        using (XmlReader xmlReader = new XmlTextReader(stringReader))
                        {
                            returnObject = (T) xmlSerializer.Deserialize(xmlReader);
                            xmlReader.Close();
                        }

                        stringReader.Close();
                    }
                }
                catch
                {
                    // TODO: Adding logging here.
                }
            }

            return returnObject;
        }

        public static void Save<T>(T serializableObject, string fileName)
        {
            if (serializableObject != null)
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    XmlSerializer xmlSerializer = new XmlSerializer(serializableObject.GetType());

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        xmlSerializer.Serialize(memoryStream, serializableObject);
                        memoryStream.Position = 0;
                        xmlDocument.Load(memoryStream);
                        xmlDocument.Save(fileName);
                        memoryStream.Close();
                    }
                }
                catch
                {
                    // TODO: Adding logging here.
                }
            }
        }
    }
}