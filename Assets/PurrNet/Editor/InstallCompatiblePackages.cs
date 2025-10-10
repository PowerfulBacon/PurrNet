using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PurrNet.Editor
{
    public static class InstallCompatiblePackages
    {
        const string PACKAGES = "Tools/PurrNet/Packages";

        [UsedImplicitly]
        public static void VerifyEdgegap()
        {
#if EDGEGAP_PURRNET_SUPPORT
            VerifyEdgegapInternal();
#endif
        }

        [UsedImplicitly]
        private static void OpenDockerInstallInstructions()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    Application.OpenURL("https://docs.docker.com/desktop/setup/install/windows-install/");
                    break;
                case RuntimePlatform.OSXEditor:
                    Application.OpenURL("https://docs.docker.com/desktop/setup/install/mac-install/");
                    break;
                case RuntimePlatform.LinuxEditor:
                    Application.OpenURL("https://docs.docker.com/desktop/setup/install/linux/");
                    break;
                default:
                    Application.OpenURL("https://docs.docker.com/engine/install/");
                    break;
            }
        }

#if EDGEGAP_PURRNET_SUPPORT
        static bool HasLinuxBuildSupport()
        {
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
                return true;
            return false;
        }

        static float TryFindProgress(string output)
        {
            try
            {
                int pIndex = output.IndexOf("%", System.StringComparison.Ordinal);
                if (pIndex == -1)
                    return 0;

                int findPreceeingSpace = output.LastIndexOf(' ', pIndex);
                if (findPreceeingSpace == -1)
                    return 0;
                var percentageString = output.Substring(findPreceeingSpace + 1, pIndex - findPreceeingSpace - 1);

                if (!float.TryParse(percentageString, out float progress))
                    return 0;

                return progress / 100f;
            }
            catch
            {
                return 0;
            }
        }


        [MenuItem(PACKAGES + "/Edgegap/Verify Requirements", priority = 100)]
        static void VerifyEdgegapInternal()
        {
            EditorUtility.DisplayProgressBar("Verifying Edgegap Requirements", "Please wait...", 0.5f);

            var summary = new System.Text.StringBuilder();

            string buttonText;
            System.Action buttonAction;

            if (!ToolChecker.CheckTool("docker"))
            {
                summary.AppendLine("Docker is not installed.");
                buttonText = "Install Docker";
                buttonAction = OpenDockerInstallInstructions;
            }
            else if (!ToolChecker.CheckTool("docker", "ps"))
            {
                summary.AppendLine("Docker is not running.");
                summary.AppendLine("If you have docker desktop installed, please start it.");
                buttonText = "Open Docker Docs";
                buttonAction = OpenDockerInstallInstructions;
            }
            else if (!HasLinuxBuildSupport())
            {
                summary.AppendLine($"Linux build support is not installed for {Application.unityVersion}.");
                buttonText = "Install";
                buttonAction = () =>
                {
                    var unityVersion = Application.unityVersion;
                    var args = $"install-modules --version {unityVersion} -m linux-mono linux-il2cpp";

                    bool linuxMono = false;
                    bool linuxIl2cpp = false;
                    string lastOutput = string.Empty;
                    float progress = 0f;

                    var tokenSource = new CancellationTokenSource();
                    var command = Task.Run(() => ToolChecker.RunCommand(tokenSource.Token, UnityHubHelper.Path, $"{UnityHubHelper.Headless} {args}", output =>
                    {
                        lastOutput = output;
                        progress = TryFindProgress(output);

                        if (output.Contains("already installed"))
                        {
                            linuxMono = linuxMono || output.Contains("linux-mono");
                            linuxIl2cpp = linuxIl2cpp || output.Contains("linux-il2cpp");
                        }

                    }), tokenSource.Token);

                    while (!command.IsCompleted)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Installing Linux Modules", lastOutput, progress))
                        {
                            tokenSource.Cancel();
                            return;
                        }
                        Thread.Sleep(100);
                    }

                    EditorUtility.ClearProgressBar();

                    if (linuxMono && linuxIl2cpp)
                    {
                        EditorUtility.DisplayDialog("Installing Linux Modules", "Linux Modules Already Installed", "Ok");
                    }
                    else if (EditorUtility.DisplayDialog("Linux Modules Installed", "Please restart Unity", "Quit Unity", "Cancel"))
                        EditorApplication.Exit(0);
                };
            }
            else
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Edgegap Requirements Verified", "All requirements are met.", "Ok");
                return;
            }

            string cancelButton = buttonText != "Ok" ? "Cancel" : string.Empty;

            EditorUtility.ClearProgressBar();

            if (EditorUtility.DisplayDialog("Missing Edgegap Requirements", summary.ToString(), buttonText, cancelButton))
                buttonAction.Invoke();
        }

        [MenuItem(PACKAGES + "/Edgegap/Update Edgegap", priority = 101)]
        public static void UpdateEdgegap()
        {
            Client.Remove("com.edgegap.unity-servers-plugin");
            Client.Add(
                "https://github.com/edgegap/edgegap-unity-plugin.git#partner/purrnet-source");
            Client.Resolve();
        }

        [MenuItem(PACKAGES + "/Edgegap/Uninstall Edgegap", priority = 102)]
        public static void UninstallEdgegap()
        {
            if (EditorUtility.DisplayDialog("Uninstall Edgegap", "This will remove Edgegap from the package manager. Do you want to continue?", "Yes", "No"))
            {
                Client.Remove("com.edgegap.unity-servers-plugin");
                SymbolsHelper.RemoveSymbol("EDGEGAP_PLUGIN_SERVERS");
                Client.Resolve();
            }
        }
#else
        [MenuItem(PACKAGES + "/Install Edgegap", priority = 100)]
        public static async void InstallEdgegap()
        {
            if (!GitHelper.CheckGit())
                return;

            Client.Add("https://github.com/edgegap/edgegap-unity-plugin.git#partner/purrnet-source");
            Client.Resolve();
        }
#endif

#if UNITASK_PURRNET_SUPPORT
        [MenuItem(PACKAGES + "/Uninstall UniTask", priority = 101)]
        public static void UninstallUniTask()
        {
            if (EditorUtility.DisplayDialog("Uninstall UniTask", "This will remove UniTask from the package manager. Do you want to continue?", "Yes", "No"))
            {
                Client.Remove("com.cysharp.unitask");
                Client.Resolve();
            }
        }
#else
        [MenuItem(PACKAGES + "/Install UniTask", priority = 104)]
        public static void InstallUniTask()
        {
            if (!GitHelper.CheckGit())
                return;

            if (EditorUtility.DisplayDialog("Install UniTask",
                    "This will install UniTask from the package manager. Do you want to continue?", "Yes", "No"))
            {
                Client.Add("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
                Client.Resolve();
            }
        }
#endif
    }
}
