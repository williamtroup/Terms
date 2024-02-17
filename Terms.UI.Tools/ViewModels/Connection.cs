﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels
{
    [Serializable]
    public class Connection : Observable, IDataModel
    {
        #region Private Constants

        private const int DefaultPort = 3389;
        private const string DefaultLastAccessed = "Unknown";

        #endregion

        #region Property Name

        private string m_name = string.Empty;

        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged(nameof(Name));
            } 
        }

        #endregion

        #region Property Address

        private string m_address = string.Empty;

        public string Address
        {
            get => m_address;
            set
            {
                m_address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        #endregion

        #region Property Port

        private int m_port = DefaultPort;

        public int Port
        {
            get => m_port;
            set
            {
                m_port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        #endregion

        #region Property Username

        private string m_username = string.Empty;

        public string Username
        {
            get => m_username;
            set
            {
                m_username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        #endregion

        #region Property Password

        private string m_password = string.Empty;

        public string Password
        {
            get => m_password;
            set
            {
                m_password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        #endregion

        #region Property AskForCredentials

        private bool m_askForCredentials;

        public bool AskForCredentials
        {
            get => m_askForCredentials;
            set
            {
                m_askForCredentials = value;
                OnPropertyChanged(nameof(AskForCredentials));
                OnPropertyChanged(nameof(AskForCredentialsYesNo));
            }
        }

        #endregion

        #region Property AskForCredentialsYesNo

        [XmlIgnore]
        public string AskForCredentialsYesNo => m_askForCredentials ? "Yes" : "No";

        #endregion

        #region Property StartUsingLastKnownPosition

        private bool m_startUsingLastPosition;

        public bool StartUsingLastKnownPosition
        {
            get => m_startUsingLastPosition;
            set
            {
                m_startUsingLastPosition = value;
                OnPropertyChanged(nameof(StartUsingLastKnownPosition));
            }
        }

        #endregion

        #region Property StartInFullScreenMode

        private bool m_startInFullScreenMode = true;

        public bool StartInFullScreenMode
        {
            get => m_startInFullScreenMode;
            set
            {
                m_startInFullScreenMode = value;
                OnPropertyChanged(nameof(StartInFullScreenMode));
            }
        }

        #endregion

        #region Property SpanAcrossMultipleMonitors

        private bool m_spanAcrossMultipleMonitors;

        public bool SpanAcrossMultipleMonitors
        {
            get => m_spanAcrossMultipleMonitors;
            set
            {
                m_spanAcrossMultipleMonitors = value;
                OnPropertyChanged(nameof(SpanAcrossMultipleMonitors));
            }
        }

        #endregion

        #region Property LoginUsingAdminMode

        private bool m_loginUsingAdminMode;

        public bool LoginUsingAdminMode
        {
            get => m_loginUsingAdminMode;
            set
            {
                m_loginUsingAdminMode = value;
                OnPropertyChanged(nameof(LoginUsingAdminMode));
            }
        }

        #endregion

        #region Property UseSpecificWidthAndHeight

        private bool m_useSpecificWidthAndHeight;

        public bool UseSpecificWidthAndHeight
        {
            get => m_useSpecificWidthAndHeight;
            set
            {
                m_useSpecificWidthAndHeight = value;
                OnPropertyChanged(nameof(UseSpecificWidthAndHeight));
            }
        }

        #endregion

        #region Property Width

        private int m_width = 700;

        public int Width
        {
            get => m_width;
            set
            {
                m_width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        #endregion

        #region Property Height

        private int m_height = 700;

        public int Height
        {
            get => m_height;
            set
            {
                m_height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        #endregion

        #region Property Visibility

        private Visibility m_visibility = Visibility.Visible;

        [XmlIgnore]
        public Visibility Visibility
        {
            get => m_visibility;
            set
            {
                m_visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }

        #endregion

        #region Property LastAccessed

        private string m_lastAccessed;

        public string LastAccessed
        {
            get => m_lastAccessed;
            set
            {
                m_lastAccessed = value;
                OnPropertyChanged(nameof(LastAccessed));
            }
        }

        #endregion

        #region Property Notes

        private string m_notes;

        public string Notes
        {
            get => m_notes;
            set
            {
                m_notes = string.IsNullOrEmpty(value) ? null : value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        #endregion

        #region Property BeforeOpeningProgram

        private string m_beforeOpeningProgram = string.Empty;

        public string BeforeOpeningProgram
        {
            get => m_beforeOpeningProgram;
            set
            {
                m_beforeOpeningProgram = value;
                OnPropertyChanged(nameof(BeforeOpeningProgram));
            }
        }

        #endregion

        #region Property BeforeOpeningArguments

        private string m_beforeOpeningArguments = string.Empty;

        public string BeforeOpeningArguments
        {
            get => m_beforeOpeningArguments;
            set
            {
                m_beforeOpeningArguments = value;
                OnPropertyChanged(nameof(BeforeOpeningArguments));
            }
        }

        #endregion

        #region Property BeforeOpeningWorkingDirectory

        private string m_beforeOpeningWorkingDirectory = string.Empty;

        public string BeforeOpeningWorkingDirectory
        {
            get => m_beforeOpeningWorkingDirectory;
            set
            {
                m_beforeOpeningWorkingDirectory = value;
                OnPropertyChanged(nameof(BeforeOpeningWorkingDirectory));
            }
        }

        #endregion

        #region Property BeforeOpeningWaitForProgramToBeClosed

        private bool m_beforeOpeningWaitForProgramToBeClosed;

        public bool BeforeOpeningWaitForProgramToBeClosed
        {
            get => m_beforeOpeningWaitForProgramToBeClosed;
            set
            {
                m_beforeOpeningWaitForProgramToBeClosed = value;
                OnPropertyChanged(nameof(BeforeOpeningWaitForProgramToBeClosed));
            }
        }

        #endregion

        #region Property BeforeOpeningAlwaysRunProgramBeforeConnecting

        private bool m_beforeOpeningAlwaysRunProgramBeforeConnecting;

        public bool BeforeOpeningAlwaysRunProgramBeforeConnecting
        {
            get => m_beforeOpeningAlwaysRunProgramBeforeConnecting;
            set
            {
                m_beforeOpeningAlwaysRunProgramBeforeConnecting = value;
                OnPropertyChanged(nameof(BeforeOpeningAlwaysRunProgramBeforeConnecting));
            }
        }

        #endregion

        #region Property BeforeOpeningWaitSecondsForProgramToBeClosed

        private int m_beforeOpeningWaitMillisecondsForProgramToBeClosed;

        public int BeforeOpeningWaitSecondsForProgramToBeClosed
        {
            get => m_beforeOpeningWaitMillisecondsForProgramToBeClosed;
            set
            {
                m_beforeOpeningWaitMillisecondsForProgramToBeClosed = value;
                OnPropertyChanged(nameof(BeforeOpeningWaitSecondsForProgramToBeClosed));
            }
        }

        #endregion

        #region Property LastUserCredentialNameUsed

        private string m_lastUserCredentialNameUsed = string.Empty;

        public string LastUserCredentialNameUsed
        {
            get => m_lastUserCredentialNameUsed;
            set
            {
                m_lastUserCredentialNameUsed = value;
                OnPropertyChanged(nameof(LastUserCredentialNameUsed));
            }
        }

        #endregion

        #region Property DeleteCachedCredentialsAfterConnecting

        private bool m_deleteCachedCredentialsAfterConnecting;

        public bool DeleteCachedCredentialsAfterConnecting
        {
            get => m_deleteCachedCredentialsAfterConnecting;
            set
            {
                m_deleteCachedCredentialsAfterConnecting = value;
                OnPropertyChanged(nameof(DeleteCachedCredentialsAfterConnecting));
            }
        }

        #endregion

        #region Property Enabled

        private bool m_enabled = true;

        public bool Enabled
        {
            get => m_enabled;
            set
            {
                m_enabled = value;
                OnPropertyChanged(nameof(Enabled));
                OnPropertyChanged(nameof(ForeColor));
                OnPropertyChanged(nameof(FontStyle));
            }
        }

        #endregion

        #region Property ForeColor

        [XmlIgnore]
        public SolidColorBrush ForeColor
        {
            get
            {
                SolidColorBrush foreColor = Brushes.Black;

                if (!Enabled)
                {
                    foreColor = Brushes.Gray;
                }

                return foreColor;
            }
        }

        #endregion

        #region Property FontStyle

        [XmlIgnore]
        public FontStyle FontStyle
        {
            get
            {
                FontStyle fontStyle = FontStyles.Normal;

                if (!Enabled)
                {
                    fontStyle = FontStyles.Italic;
                }

                return fontStyle;
            }
        }

        #endregion

        public void Update(Connection model)
        {
            Name = model.Name;
            Address = model.Address;
            Port = model.Port;
            Username = model.Username;
            Password = model.Password;
            AskForCredentials = model.AskForCredentials;
            StartUsingLastKnownPosition = model.StartUsingLastKnownPosition;
            StartInFullScreenMode = model.StartInFullScreenMode;
            SpanAcrossMultipleMonitors = model.SpanAcrossMultipleMonitors;
            LoginUsingAdminMode = model.LoginUsingAdminMode;
            UseSpecificWidthAndHeight = model.UseSpecificWidthAndHeight;
            Width = model.Width;
            Height = model.Height;
            Visibility = model.Visibility;
            LastAccessed = model.LastAccessed;
            Notes = model.Notes;
            BeforeOpeningProgram = model.BeforeOpeningProgram;
            BeforeOpeningArguments = model.BeforeOpeningArguments;
            BeforeOpeningWorkingDirectory = model.BeforeOpeningWorkingDirectory;
            BeforeOpeningWaitForProgramToBeClosed = model.BeforeOpeningWaitForProgramToBeClosed;
            BeforeOpeningAlwaysRunProgramBeforeConnecting = model.BeforeOpeningAlwaysRunProgramBeforeConnecting;
            BeforeOpeningWaitSecondsForProgramToBeClosed = model.BeforeOpeningWaitSecondsForProgramToBeClosed;
            LastUserCredentialNameUsed = model.LastUserCredentialNameUsed;
            DeleteCachedCredentialsAfterConnecting = model.DeleteCachedCredentialsAfterConnecting;
            Enabled = model.Enabled;
        }

        public void ResetLastAccessed()
        {
            LastAccessed = DefaultLastAccessed;
        }

        public void ResetPort()
        {
            Port = DefaultPort;
        }
    }
}