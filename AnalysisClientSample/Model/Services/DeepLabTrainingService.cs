using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMetal.Infrastructure;

namespace AutoMetal.Services
{
    internal class DeepLabTrainingService : IDeepLabTrainingService
    {
        private readonly Action<string> _log;
        private readonly Regex _epochRegex = new Regex(@"Epoch\s+(\d+)\s*/\s*(\d+)", RegexOptions.IgnoreCase);
        private readonly ProcessRunner _runner;
        private CancellationTokenSource _cts;

        public bool IsRunning { get; private set; }

        public DeepLabTrainingService(Action<string> logCallback)
        {
            _log = logCallback ?? (_ => { });
            _runner = new ProcessRunner(_log);
        }

        public async Task<(bool success, bool stopped, string errorMessage)> StartAsync(
            int epoch,
            int batchSize,
            string projectPath,
            string vocPath,
            string pythonExePath,
            Action<int, int> progressCallback,
            Action<string> statusCallback)
        {
            if (IsRunning)
            {
                return (false, false, "训练已在进行中");
            }

            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
            {
                return (false, false, $"训练项目目录不存在：{projectPath}");
            }

            string trainScript = Path.Combine(projectPath, "train.py");
            if (!File.Exists(trainScript))
            {
                return (false, false, $"未找到训练脚本：{trainScript}");
            }

            IsRunning = true;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            progressCallback?.Invoke(0, epoch);
            statusCallback?.Invoke($"训练状态：启动中(1/{epoch})");

            string quotedTrain = $"\"{trainScript}\"";
            string quotedVocPath = $"\"{vocPath}\"";
            string saveDir = Path.Combine(projectPath, "logs");
            string quotedSaveDir = $"\"{saveDir}\"";
            string activateBat = Path.Combine(projectPath, ".venv", "Scripts", "activate.bat");
            string trainingArgs = $"{quotedTrain} --epochs {epoch} --batch-size {batchSize} --voc-path {quotedVocPath} --save-dir {quotedSaveDir}";

            string cmdArgs;
            if (File.Exists(activateBat))
            {
                cmdArgs = $"/c \"call \"\"{activateBat}\"\" && python {trainingArgs}\"";
            }
            else
            {
                string pythonExe = File.Exists(pythonExePath) ? pythonExePath : "python";
                cmdArgs = $"/c \"\"{pythonExe}\"\" {trainingArgs}";
            }

            try
            {
                int exitCode = await _runner.RunAsync("cmd.exe", cmdArgs, projectPath, _cts.Token, line =>
                {
                    HandleOutput(line, epoch, progressCallback, statusCallback);
                });
                if (_cts.IsCancellationRequested)
                {
                    statusCallback?.Invoke("训练状态：已停止");
                    _log("训练已停止");
                    return (false, true, string.Empty);
                }

                if (exitCode == 0)
                {
                    progressCallback?.Invoke(epoch, epoch);
                    statusCallback?.Invoke("训练状态：已完成");
                    _log("训练已完成");
                    return (true, false, string.Empty);
                }

                statusCallback?.Invoke($"训练状态：失败(ExitCode={exitCode})");
                return (false, false, $"训练失败，退出码：{exitCode}");
            }
            catch (Exception ex)
            {
                statusCallback?.Invoke("训练状态：异常");
                return (false, false, $"训练异常：{ex.Message}");
            }
            finally
            {
                IsRunning = false;
            }
        }

        public void Stop(Action<string> statusCallback)
        {
            if (!IsRunning)
            {
                return;
            }

            statusCallback?.Invoke("训练状态：停止中...");
            _cts?.Cancel();
        }

        private void HandleOutput(string line, int fallbackTotalEpoch, Action<int, int> progressCallback, Action<string> statusCallback)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            string text = line.Trim();
            _log($"[train] {text}");

            var match = _epochRegex.Match(text);
            if (!match.Success)
            {
                return;
            }

            if (!int.TryParse(match.Groups[1].Value, out int current))
            {
                return;
            }

            int total = fallbackTotalEpoch;
            if (int.TryParse(match.Groups[2].Value, out int parsedTotal))
            {
                total = parsedTotal;
            }
            if (total <= 0) total = 1;
            if (current < 0) current = 0;
            if (current > total) current = total;

            progressCallback?.Invoke(current, total);
            statusCallback?.Invoke($"训练状态：训练中({current}/{total})");
        }
    }
}
