using System;
using System.Collections.Generic;
using Terms.Tools.Actions;

namespace Terms.UI.Tools.ViewModels.Storage
{
    [Serializable]
    public class Connections
    {
        #region Public Constants

        public const string DefaultFilename = "connections.xml";

        #endregion

        public Connections()
        {
            Groups = new List<Group>();
        }

        public void Load(string filename = DefaultFilename)
        {
            Connections connections = SerializableObject.Open<Connections>(filename);
            if (connections != null)
            {
                Groups = connections.Groups;
            }
        }

        public void Save(string filename = DefaultFilename)
        {
            SerializableObject.Save(this, filename);
        }

        public List<Group> Groups { get; private set; }
    }
}