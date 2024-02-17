using System.Collections.Generic;
using System.Xml;

namespace Terms.Tools.Settings.Interfaces
{
    public interface IXmlSettings
    {
        XmlDocument GetDocument();
        void SaveDocument(XmlDocument xmlDocument);
        string Read(string section, string setting, string defaultValue, XmlDocument xmlOverrideDocument = null);
        void Write(string section, string setting, string value, XmlDocument xmlOverrideDocument = null);
        bool RemoveSection(string section, XmlDocument xmlOverrideDocument = null);
        Dictionary<string, string> ReadAll(string section, XmlDocument xmlOverrideDocument = null);
    }
}