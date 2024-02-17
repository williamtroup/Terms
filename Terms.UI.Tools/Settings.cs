namespace Terms.UI.Tools;

public static class Settings
{
    public const string Window = "Window";
    public const string WindowNameFormat = "{0}{1}";

    public static class MainWindow
    {
        public const string Display = "Display";
        public const string StartUp = "StartUp";
        public const string Connections = "Connections";

        public const string GroupSectionWidth = "265";
        public const string LastSelectedGroup = "-1";
        public const string LastSelectedConnection = "-1";
        public const string ShowConfirmationMessagesForFileActions = "1";
        public const string MinimizeMainWindowWhenAConnectionIsOpened = "0";
        public const string ShowOpenNewConnectionArea = "0";
        public const string AllowUserCredentialsPickingWhenOpeningConnections = "1";
        public const string PreviousOpenNewConnectionEntered = "";
        public const string RememberTheLastPickedUserCredentialsForConnections = "0";
        public const string ShowLoadingSplashScreen = "1";
        public const string AllowMultipleInstancesToBeUsed = "0";
        public const string ShowInTaskBar = "1";
        public const string ShowConnectionManagementButtonsInTheTitleBar = "1";
        public const string ShowConnectionsOpenInTitleBarWhenAvailable = "1";
        public const string RunningSectionWidth = "265";
        public const string ShowTheRunningConnectionsSection = "0";
        public const string ShowConfirmationMessageBeforeClosingMainWindow = "1";
        public const string MinimizeAllOtherConnectionsWhenFocusingConnection = "0";
        public const string FadeMainWindowInOutOnStartupShutdown = "1";
        public const string UpdateLastAccessedForPingedConnections = "0";
        public const string UpdateLastAccessedWhenConnectionsAreShutdownOrRestarted = "0";
    }

    public static class AddNewGroupWindow
    {
        public const string AddNewGroupOptions = "AddNewGroupWindowOptions";

        public const string CloseWindowAfterAdding = "0";
    }

    public static class AddNewConnectionWindow
    {
        public const string AddNewConnectionOptions = "AddNewConnectionWindowOptions";

        public const string CloseWindowAfterAdding = "0";
    }

    public static class AddNewUserCredentialWindow
    {
        public const string AddNewUserCredentialOptions = "AddNewUserCredentialWindowOptions";

        public const string CloseWindowAfterAdding = "0";
    }

    public static class SearchWindow
    {
        public const string SearchOptions = "SearchWindowOptions";

        public const string ShowOptions = "1";
        public const string MatchCase = "0";
        public const string MatchOnlyUsedConnections = "0";
        public const string MakeTransparentWhenFocusIsLost = "1";
        public const string SearchType = "";
        public const string LastSearch = "";
        public const string ShowAllMatchingItems = "0";
        public const string WasOpenedOnExit = "0";
        public const string SearchArea = "";
    }

    public static class PingWindow
    {
        public const string PingOptions = "PingWindowOptions";

        public const string AutomaticallyScrollToTheBottom = "1";

        public const int TotalTimeBetweenEachPing = 2000;
        public const int TotalTimeBetweenEachFailedPing = 5000;
    }

    public static class ConnectionStatusesWindow
    {
        public const string ConnectionStatusesOptions = "ConnectionStatusesWindowOptions";

        public const string AutomaticallyScrollToTheBottom = "1";
        public const string OnlyShowUnavailableConnections = "0";
    }

    public static class ShutdownConnectionsWindow
    {
        public const string ShutdownConnectionOptions = "ShutdownConnectionWindowOptions";

        public const string LastMessage = "";
        public const string LastDelay = "0";
    }

    public static class ConnectionDetailsWindow
    {
        public const string ConnectionDetailsOptions = "ConnectionDetailsWindowOptions";

        public const string OnlyShowTheRunningServices = "0";
    }

    public static class ConnectionServicesWindow
    {
        public const string ConnectionServicesOptions = "ConnectionServicesWindowOptions";

        public const string LastUsedServiceName = "";
        public const string AutomaticallyScrollToTheBottom = "1";
        public const string OnlyShowTheUnsuccessfulServiceStatuses = "0";
    }
}