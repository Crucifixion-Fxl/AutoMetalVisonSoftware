using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AutoMetal.Infrastructure
{
    internal class ProcessRunner
    {
        private readonly Action<string> _log;

        public ProcessRunner(Action<string> logCallback)
        {
            _log = logCallback ?? (_ => { });
        }

        public async Task<int> RunAsync(
            string fileName,
            string arguments,
            string workingDirectory,
            CancellationToken token = default,
            Action<string> outputCallback = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                p.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        if (outputCallback != null)
                        {
                            outputCallback(e.Data);
                        }
                        else
                        {
                            _log(e.Data);
                        }
                    }
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        if (outputCallback != null)
                        {
                            outputCallback(e.Data);
                        }
                        else
                        {
                            _log(e.Data);
                        }
                    }
                };

                if (!p.Start())
                {
                    return -1;
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                using (token.Register(() =>
                {
                    try
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                    }
                    catch
                    {
                    }
                }))
                {
                    await Task.Run(() => p.WaitForExit());
                }

                return p.ExitCode;
            }
        }
    }
}
