using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PurrNet.Editor
{
    public static class ToolChecker
    {
        public static async Task RunCommand(System.Threading.CancellationToken cancellationToken, string command, string arguments, Action<string> outputReciever = null, Action<string> errorReciever = null)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

#if !UNITY_EDITOR_WIN
            string existingPath = Environment.GetEnvironmentVariable("PATH");
            string customPath = $"{existingPath}:/usr/local/bin";
            startInfo.EnvironmentVariables["PATH"] = customPath;
#endif

            var proc = new Process { StartInfo = startInfo, };
            proc.EnableRaisingEvents = true;

            var errors = new ConcurrentQueue<string>();
            var outputs = new ConcurrentQueue<string>();

            proc.OutputDataReceived += (_, e) => outputs.Enqueue(e.Data);
            proc.ErrorDataReceived += (_, e) => errors.Enqueue(e.Data);

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            while (!proc.HasExited)
            {
                await Task.Delay(100, cancellationToken);

                pipeQueue(errors, errorReciever);
                pipeQueue(outputs, outputReciever);

                if (cancellationToken.IsCancellationRequested)
                {
                    proc.Kill();
                    break;
                }
            }

            pipeQueue(errors, errorReciever);
            pipeQueue(outputs, outputReciever);
            return;


            void pipeQueue(ConcurrentQueue<string> q, Action<string> opt)
            {
                while (!q.IsEmpty)
                {
                    if (q.TryDequeue(out string msg) && !string.IsNullOrWhiteSpace(msg))
                    {
                        opt?.Invoke(msg);
                    }
                }
            }
        }

        public static bool CheckTool(string tool, string args = "--version")
        {
            return CheckTool(tool, args, out _, out _);
        }

        public static bool CheckTool(string tool, string args, out string output)
        {
            return CheckTool(tool, args, out output, out _);
        }

        public static void RunAndForget(string tool, string args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
#if UNITY_EDITOR_WIN
                    FileName = "cmd.exe",
#else
                    FileName = "/bin/bash",
#endif
#if UNITY_EDITOR_WIN
                    Arguments = $"/c \"{tool}\" {args}",
#else
                    Arguments = $"-c \"{tool}\" {args}",
#endif
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

#if !UNITY_EDITOR_WIN
                string existingPath = System.Environment.GetEnvironmentVariable("PATH");
                string customPath = $"{existingPath}:/usr/local/bin";
                startInfo.EnvironmentVariables["PATH"] = customPath;
#endif
                var process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();
            }
            catch
            {
                // ignored
            }
        }

        public static bool CheckTool(string tool, string args, out string output, out string error)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
#if UNITY_EDITOR_WIN
                    FileName = "cmd.exe",
#else
                    FileName = "/bin/bash",
#endif
#if UNITY_EDITOR_WIN
                    Arguments = $"/c \"{tool}\" {args}",
#else
                    Arguments = $"-c \"{tool}\" {args}",
#endif
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

#if !UNITY_EDITOR_WIN
                string existingPath = System.Environment.GetEnvironmentVariable("PATH");
                string customPath = $"{existingPath}:/usr/local/bin";
                startInfo.EnvironmentVariables["PATH"] = customPath;
#endif
                var process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();

                output = process.StandardOutput.ReadToEnd();
                error  = process.StandardError.ReadToEnd();

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                output = null;
                error = null;
                return false;
            }
        }
    }
}
