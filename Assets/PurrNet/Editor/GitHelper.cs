using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    public static class GitHelper
    {
        static bool? _installed;

        public static bool installed
        {
            get
            {
                _installed ??= IsGitInstalled();
                return _installed.Value;
            }
        }

        public static bool CheckGit()
        {
            if (!GitHelper.installed)
            {
                if (EditorUtility.DisplayDialog("Git not installed",
                        "Git is not installed on this machine.\n" +
                        "Please install it, restart UnityHub then try again.", "Install", "Cancel"))
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.WindowsEditor:
                            Application.OpenURL("https://git-scm.com/download/win");
                            break;
                        case RuntimePlatform.OSXEditor:
                            Application.OpenURL("https://git-scm.com/download/mac");
                            break;
                        case RuntimePlatform.LinuxEditor:
                            Application.OpenURL("https://git-scm.com/download/linux");
                            break;
                        default:
                            Application.OpenURL("https://git-scm.com/downloads");
                            break;
                    }
                }
                return false;
            }

            return true;
        }

        static bool IsGitInstalled()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
