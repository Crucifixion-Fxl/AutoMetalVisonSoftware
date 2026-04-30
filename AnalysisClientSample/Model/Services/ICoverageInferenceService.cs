using System;

namespace AutoMetal.Services
{
    internal interface ICoverageInferenceService
    {
        (bool success, int total, int successCount, int failedCount, string errorMessage) RunCoverageBatch(
            string testSetDir,
            string enginePath,
            Action<int, int> progressCallback,
            Action<string> statusCallback);
    }
}
