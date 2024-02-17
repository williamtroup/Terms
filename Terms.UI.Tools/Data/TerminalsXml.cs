using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Xml;
using Terms.UI.Tools.ViewModels;

namespace Terms.UI.Tools.Data
{
    public class TerminalsXml
    {
        #region Private Variables

        private string m_filename;
        private Group m_ungroupedEntriesGroupViewModel;

        #endregion

        public bool Read(string filter, string title, List<Group> groups)
        {
            bool hasBeenRead = false;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != null && result.Value)
            {
                CreateUngroupedGroup();

                m_filename = openFileDialog.FileName;

                ReadFavourates(groups);

                if (m_ungroupedEntriesGroupViewModel.Connections.Count > 0)
                {
                    groups.Add(m_ungroupedEntriesGroupViewModel);
                }

                hasBeenRead = true;
            }

            return hasBeenRead;
        }

        private void ReadFavourates(ICollection<Group> groups)
        {
            XmlDocument xmlDocument = LoadDocument(m_filename);
            XmlNodeList xmlNodeList = xmlDocument.DocumentElement?.SelectNodes("/favorites/favorite");

            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (xmlNode != null)
                    {
                        ReadFavourate(xmlNode, groups);
                    }
                }
            }
        }

        private void ReadFavourate(XmlNode xmlNode, ICollection<Group> groups)
        {
            XmlNode xmlServerNameNode = null;
            XmlNode xmlNameNode = null;
            XmlNode xmlTagsNode = null;

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "serverName" when xmlServerNameNode == null:
                        xmlServerNameNode = childNode;
                        break;

                    case "name" when xmlNameNode == null:
                        xmlNameNode = childNode;
                        break;

                    case "tags" when xmlTagsNode == null:
                        xmlTagsNode = childNode;
                        break;
                }
            }

            if (xmlServerNameNode != null && xmlNameNode != null && xmlTagsNode != null)
            {
                string serverName = xmlServerNameNode.InnerText;
                string name = xmlNameNode.InnerText;
                string tags = xmlTagsNode.InnerText;

                Connection connection = new Connection
                {
                    Name = name,
                    Address = serverName,
                    AskForCredentials = true
                };

                if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(tags))
                {
                    bool addUnderNewGroup = true;

                    foreach (Group group in groups)
                    {
                        if (string.Equals(group.Name, tags, StringComparison.CurrentCultureIgnoreCase))
                        {
                            AddToGroup(connection, group);

                            addUnderNewGroup = false;
                            break;
                        }
                    }

                    if (addUnderNewGroup)
                    {
                        Group group = new Group
                        {
                            Name = tags
                        };

                        group.Connections.Add(connection);
                        groups.Add(group);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(tags))
                    {
                        AddToGroup(connection, m_ungroupedEntriesGroupViewModel);
                    }
                }
            }
        }

        private static void AddToGroup(Connection connection, Group group)
        {
            bool connectionFound = false;

            foreach (Connection searchConnectionViewModel in group.Connections)
            {
                if (string.Equals(searchConnectionViewModel.Name, connection.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    connectionFound = true;
                    break;
                }
            }

            if (!connectionFound)
            {
                group.Connections.Add(connection);
            }
        }

        private void CreateUngroupedGroup()
        {
            string ungroupEntriesName = $"Ungrouped_{DateTime.Now:HH_mm_ss_yyyy_MM_dd}";

            m_ungroupedEntriesGroupViewModel = new Group
            {
                Name = ungroupEntriesName
            };
        }

        private static XmlDocument LoadDocument(string filename)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);

            return xmlDocument;
        }
    }
}