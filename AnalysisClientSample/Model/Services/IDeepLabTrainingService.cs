using System;
using System.Threading.Tasks;

namespace AutoMetal.Services
{
    internal interface IDeepLabTrainingService
    {
        bool IsRunning { get; }

        Task<(bool success, bool stopped, string errorMessage)> StartAsync(
            int epoch,
            int batchSize,
            string projectPath,
            string vocPath,
            string pythonExePath,
            Action<int, int> progressCallback,
            Action<string> statusCallback);

        void Stop(Action<string> statusCallback);
    }
}
