using System;
using System.Collections.Generic;
using AutoMetal.Models;

namespace AutoMetal.Services
{
    internal class AlgorithmBatchRequest
    {
        public List<string> RunImages { get; set; } = new List<string>();
        public bool ManualMode { get; set; }
        public List<string> ManualMasks { get; set; } = new List<string>();
        public Func<string, string> GetBatchName { get; set; }
        public Func<string, (string binaryPath, string standardPath, string croppedPath, string outputPath, string maskPath, string heatmapPath)> GetOutputPaths { get; set; }
        public Func<string, bool> TryPreprocess { get; set; }
        public Func<string, string, string, string, (double coverage, string maskCoveragePath)> GetCoverageFromManualMask { get; set; }
        public Func<string, string, string, (double coverage, string maskCoveragePath)> GetCoverageAutoMask { get; set; }
        public Func<string, string, double> CalculateUniformity { get; set; }
        public Action<string, string, string, bool> GenerateHeatmap { get; set; }
    }

    internal class AlgorithmBatchResponse
    {
        public List<AlgorithmBatchResult> BatchResults { get; set; } = new List<AlgorithmBatchResult>();
        public List<AlgorithmDirectoryAverage> DirectoryAverages { get; set; } = new List<AlgorithmDirectoryAverage>();
    }
}
