using System.Collections.Generic;
using System.IO;
using System.Xml;
using Terms.Tools.Settings.Interfaces;

namespace Terms.Tools.Settings
{
    public class XmlSettings(string filename = "settings.xml", string root = "Configuration", string entryName = "Setting") : IXmlSettings
    {
        private const string EntryName = "Name";
        private const string EntryValue = "Value";
        private const string EntryFormat = "/{0}/{1}/{2}[@{3}=\"{4}\"]";

        private readonly string m_filename = filename;
        private readonly string m_root = root;
        private readonly string m_entryName = entryName;

        public XmlDocument GetDocument()
        {
            Create();

            XmlDocument xmlDocument = new();
            xmlDocument.Load(m_filename);

            return xmlDocument;
        }

        public void SaveDocument(XmlDocument xmlDocument)
        {
            xmlDocument.Save(m_filename);
        }

        public string Read(string section, string setting, string defaultValue, XmlDocument xmlOverrideDocument = null)
        {
            string value = defaultValue;

            XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);
            XmlNode xmlNode = xmlDocument.DocumentElement?.SelectSingleNode(string.Format(EntryFormat, m_root, section, m_entryName, EntryName, setting));

            string newValue = xmlNode?.Attributes?.GetNamedItem(EntryValue).Value;

            if (!string.IsNullOrEmpty(newValue))
            {
                value = newValue;
            }

            return value;
        }

        public void Write(string section, string setting, string value, XmlDocument xmlOverrideDocument = null)
        {
            XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);

            if (xmlDocument.DocumentElement != null)
            {
                XmlElement xmlNode = (XmlElement) xmlDocument.DocumentElement.SelectSingleNode(string.Format(EntryFormat, m_root, section, m_entryName, EntryName, setting));

                if (xmlNode != null)
                {
                    xmlNode.Attributes.GetNamedItem(EntryValue).Value = value;
                }
                else
                {
                    xmlNode = xmlDocument.CreateElement(m_entryName);
                    xmlNode.SetAttribute(EntryValue, value);
                    xmlNode.SetAttribute(EntryName, setting);

                    if (xmlDocument.DocumentElement != null)
                    {
                        XmlNode xmlNodeRoot = xmlDocument.DocumentElement.SelectSingleNode($"/{m_root}/{section}");

                        if (xmlNodeRoot != null)
                        {
                            xmlNodeRoot.AppendChild(xmlNode);
                        }
                        else
                        {
                            xmlNodeRoot = xmlDocument.DocumentElement.SelectSingleNode($"/{m_root}");
                            xmlNodeRoot?.AppendChild(xmlDocument.CreateElement(section));

                            if (xmlDocument.DocumentElement != null)
                            {
                                xmlNodeRoot = xmlDocument.DocumentElement.SelectSingleNode($"/{m_root}/{section}");
                                xmlNodeRoot?.AppendChild(xmlNode);
                            }
                        }
                    }
                }
            }

            if (xmlOverrideDocument == null)
            {
                xmlDocument.Save(m_filename);
            }
        }

        public bool RemoveSection(string section, XmlDocument xmlOverrideDocument = null)
        {
            bool done = false;

            XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);
            XmlNode xmlNode = xmlDocument.DocumentElement?.SelectSingleNode($"/{m_root}/{section}");

            if (xmlNode != null)
            {
                xmlNode.RemoveAll();

                xmlDocument.DocumentElement.RemoveChild(xmlNode);

                if (xmlOverrideDocument == null)
                {
                    xmlDocument.Save(m_filename);
                }

                done = true;
            }

            return done;
        }

        public Dictionary<string, string> ReadAll(string section, XmlDocument xmlOverrideDocument = null)
        {
            Dictionary<string, string> items = new();

            XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);
            XmlNodeList xmlNodeList = xmlDocument.DocumentElement?.SelectNodes($"/{m_root}/{section}/{m_entryName}");

            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (xmlNode?.Attributes != null)
                    {
                        string name = xmlNode.Attributes.GetNamedItem(EntryName).Value;
                        string value = xmlNode.Attributes.GetNamedItem(EntryValue).Value;

                        items[name] = value;
                    }
                }
            }

            return items;
        }

        private XmlDocument LoadDocument(XmlDocument xmlOverrideDocument)
        {
            XmlDocument xmlDocument = xmlOverrideDocument ?? new XmlDocument();

            if (xmlOverrideDocument == null)
            {
                Create();

                xmlDocument.Load(m_filename);
            }

            return xmlDocument;
        }

        private void Create()
        {
            if (!File.Exists(m_filename))
            {
                using (FileStream fileStream = File.Open(m_filename, FileMode.Create))
                using (StreamWriter fileStreamWriter = new(fileStream))
                {
                    fileStreamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    fileStreamWriter.WriteLine("<{0}>", m_root);
                    fileStreamWriter.WriteLine("</{0}>", m_root);
                    fileStreamWriter.Close();
                }
            }
        }
    }
}