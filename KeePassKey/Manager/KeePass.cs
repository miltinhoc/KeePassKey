using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KeePassKey.Manager
{
    public class KeePass
    {
        private static readonly List<string> _programFolders = new List<string>()
        {
            Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
            Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")
        };

        public static string FindKeePassInstallation()
        {
            foreach (string programFolder in _programFolders)
            {
                List<string> directories = Directory.EnumerateDirectories(programFolder, "KeePass Password *", SearchOption.TopDirectoryOnly).ToList();

                if (directories.Count == 0)
                    continue;

                return directories.First();
            }

            return string.Empty;
        }

        public static void UninstallNgen(string name)
        {
            List<string> ngenExecutables = Directory.GetFiles(
                path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET\\Framework64"), 
                searchPattern: "ngen.exe", 
                searchOption: SearchOption.AllDirectories).ToList();

            if (ngenExecutables.Count == 0)
                return;

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = ngenExecutables.Last(),
                Arguments = $"uninstall \"{name}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    process.WaitForExit();
                }
            }
            catch{}
        }
    }
}
