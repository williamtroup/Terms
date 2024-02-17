using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Terms.Tools.Actions;
using Terms.UI.Tools.ViewModels;

namespace Terms.UI.Tools.Shell
{
    public static class Mstsc
    {
        public static readonly string ConnectionExecutableName = "mstsc";
        public static readonly string ConnectionExecutable = $"{ConnectionExecutableName}.exe";

        private const string ExecutablePath = @"%SystemRoot%\system32\{0}";
        private const string SendKeysCredentialStorageName = "TERMSRV";

        public static void Open(MstscProcesses mstscProcesses, List<Connection> connections, bool setCredentails = true)
        {
            foreach (Connection connection in connections)
            {
                Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(connection.BeforeOpeningProgram) && File.Exists(connection.BeforeOpeningProgram) && connection.BeforeOpeningAlwaysRunProgramBeforeConnecting)
                    {
                        OpenBeforeConnectingProgram(connection);
                    }

                    if (setCredentails)
                    {
                        SendCmdKeysForCredentails(connection);
                    }

                    OpenConnection(mstscProcesses, connection);

                    if (connection.DeleteCachedCredentialsAfterConnecting)
                    {
                        DeleteCachedCredentials(connection);
                    }
                });
            }
        }

        public static void DeleteCachedCredentials(Connection connection)
        {
            string arguments = $"/delete:{SendKeysCredentialStorageName}/{connection.Address}";

            SendCmdKeys(arguments);
        }

        private static void OpenBeforeConnectingProgram(Connection connection)
        {
            using (Process beforeConnectingProcess = new())
            {
                beforeConnectingProcess.StartInfo.FileName = connection.BeforeOpeningProgram;

                if (!string.IsNullOrEmpty(connection.BeforeOpeningArguments))
                {
                    beforeConnectingProcess.StartInfo.Arguments = connection.BeforeOpeningArguments;
                }

                if (!string.IsNullOrEmpty(connection.BeforeOpeningWorkingDirectory))
                {
                    beforeConnectingProcess.StartInfo.WorkingDirectory = connection.BeforeOpeningWorkingDirectory;
                }

                beforeConnectingProcess.Start();

                if (connection.BeforeOpeningWaitForProgramToBeClosed)
                {
                    if (connection.BeforeOpeningWaitSecondsForProgramToBeClosed > 0)
                    {
                        int millisecondsToWait = connection.BeforeOpeningWaitSecondsForProgramToBeClosed * 1000;

                        beforeConnectingProcess.WaitForExit(millisecondsToWait);
                    }
                    else
                    {
                        beforeConnectingProcess.WaitForExit();
                    }
                }
            }
        }

        private static void SendCmdKeysForCredentails(Connection connection)
        {
            string arguments = $"/generic:{SendKeysCredentialStorageName}/{connection.Address} /user:{connection.Username} /pass:{Cypher.Decrypt(connection.Password)}";

            SendCmdKeys(arguments);
        }

        private static void OpenConnection(MstscProcesses mstscProcesses, Connection connection)
        {
            using (Process process = new())
            {
                List<string> arguments = GetMstscArguments(connection);

                string commandLineArguments = string.Join(" ", arguments.ToArray());
                string executable = string.Format(ExecutablePath, ConnectionExecutable);

                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(executable);
                process.StartInfo.Arguments = commandLineArguments;
                process.Start();

                MstscProcess mstscProcess = mstscProcesses.Add(connection, process);

                if (mstscProcess != null)
                {
                    process.WaitForExit();

                    mstscProcesses.Remove(mstscProcess);
                }
            }
        }

        private static void SendCmdKeys(string arguments)
        {
            using (Process cmdkeyProcess = new())
            {
                string executable = string.Format(ExecutablePath, "cmdkey.exe");

                cmdkeyProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(executable);
                cmdkeyProcess.StartInfo.Arguments = arguments;
                cmdkeyProcess.StartInfo.CreateNoWindow = true;
                cmdkeyProcess.StartInfo.UseShellExecute = false;
                cmdkeyProcess.Start();
                cmdkeyProcess.WaitForExit();
            }
        }

        private static List<string> GetMstscArguments(Connection connection)
        {
            List<string> arguments = new()
            {
                $"/v:{connection.Address}:{connection.Port}"
            };

            if (connection.StartInFullScreenMode)
            {
                arguments.Add("/f");
            }
            else if (connection.SpanAcrossMultipleMonitors)
            {
                arguments.Add("/span");
            }
            else if (connection.UseSpecificWidthAndHeight)
            {
                arguments.Add($"/w {connection.Width}");
                arguments.Add($"/h {connection.Height}");
            }

            if (connection.LoginUsingAdminMode)
            {
                arguments.Add("/admin");
            }

            return arguments;
        }
    }
}