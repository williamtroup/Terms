using System.Diagnostics;

namespace Terms.Tools.Windows
{
    public static class Processes
    {
        public static bool IsRunning()
        {
            return IsRunning(Process.GetCurrentProcess().ProcessName, 1);
        }

        public static bool IsRunning(string processNameWithoutExtension)
        {
            return IsRunning(processNameWithoutExtension, 0);
        }

        public static bool IsRunning(string processNameWithoutExtension, int processesAllowed)
        {
            Process[] processes = Process.GetProcessesByName(processNameWithoutExtension);

            return processes.Length > processesAllowed;
        }

        public static bool Start(string path)
        {
            bool ran = true;

            try
            {
                Process.Start(path);
            }
            catch
            {
                ran = false;
            }

            return ran;
        }
    }
}