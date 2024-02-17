using System;
using System.Collections.Generic;
using Terms.Tools.Actions;

namespace Terms.UI.Tools.ViewModels.Storage;

[Serializable]
public class Credentials
{
    private const string DefaultFilename = "credentials.xml";

    public Credentials()
    {
        UserCredentials = new List<Credential>();
    }

    public void Load(string filename = DefaultFilename)
    {
        Credentials credentials = SerializableObject.Open<Credentials>(filename);
        if (credentials != null)
        {
            UserCredentials = credentials.UserCredentials;
        }
    }

    public void Save(string filename = DefaultFilename)
    {
        SerializableObject.Save(this, filename);
    }

    public int Count
    {
        get
        {
            int count = 0;

            foreach (Credential credential in UserCredentials)
            {
                if (credential.Enabled)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public List<Credential> UserCredentials { get; private set; }
}