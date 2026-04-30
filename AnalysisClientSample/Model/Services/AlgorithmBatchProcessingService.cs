using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMetal.Models;

namespace AutoMetal.Services
{
    internal class AlgorithmBatchProcessingService : IAlgorithmBatchProcessingService
    {
        public AlgorithmBatchResponse Execute(AlgorithmBatchRequest request)
        {
            var batchResults = new List<AlgorithmBatchResult>();
            foreach (var imagePath in request.RunImages)
            {
                var result = new AlgorithmBatchResult
                {
                    BatchName = request.GetBatchName(imagePath),
                    SampleName = Path.GetFileName(imagePath),
                    Status = "成功"
                };

                if (request.ManualMode)
                {
                    string sampleNameNoExt = Path.GetFileNameWithoutExtension(imagePath);
                    string matchedMaskPath = request.ManualMasks
                        .FirstOrDefault(path => string.Equals(Path.GetFileNameWithoutExtension(path), sampleNameNoExt, StringComparison.OrdinalIgnoreCase));
                    if (string.IsNullOrWhiteSpace(matchedMaskPath) || !File.Exists(matchedMaskPath))
                    {
                        result.Status = "失败：缺少同名Mask";
                        batchResults.Add(result);
                        continue;
                    }
                }

                try
                {
                    if (!request.TryPreprocess(imagePath))
                    {
                        result.Status = "失败：预处理失败";
                        batchResults.Add(result);
                        continue;
                    }

                    var outputPaths = request.GetOutputPaths(imagePath);
                    string croppedPath = outputPaths.croppedPath;
                    double coverage;
                    string maskCoveragePath;

                    if (request.ManualMode)
                    {
                        string sampleNameNoExt = Path.GetFileNameWithoutExtension(imagePath);
                        string matchedMaskPath = request.ManualMasks
                            .First(path => string.Equals(Path.GetFileNameWithoutExtension(path), sampleNameNoExt, StringComparison.OrdinalIgnoreCase));
                        var manualRes = request.GetCoverageFromManualMask(
                            croppedPath,
                            matchedMaskPath,
                            outputPaths.maskPath,
                            outputPaths.outputPath);
                        coverage = manualRes.coverage;
                        maskCoveragePath = manualRes.maskCoveragePath;
                    }
                    else
                    {
                        var autoRes = request.GetCoverageAutoMask(
                            croppedPath,
                            outputPaths.maskPath,
                            outputPaths.outputPath);
                        coverage = autoRes.coverage;
                        maskCoveragePath = autoRes.maskCoveragePath;
                    }

                    result.Coverage = coverage;
                    string outputCoveragePath = outputPaths.outputPath;
                    result.AlgorithmImagePath = File.Exists(outputCoveragePath) ? outputCoveragePath : croppedPath;

                    if (Math.Abs(coverage) < 1e-12)
                    {
                        result.Uniformity = double.NaN;
                        result.Status = "覆盖率为0，均匀性记为NaN（不参与平均）";
                    }
                    else if (!File.Exists(outputCoveragePath))
                    {
                        result.Uniformity = double.NaN;
                        result.Status = "失败：缺少用于均匀性计算的mask";
                    }
                    else
                    {
                        result.Uniformity = request.CalculateUniformity(croppedPath, maskCoveragePath);
                        request.GenerateHeatmap(croppedPath, maskCoveragePath, outputPaths.heatmapPath, request.ManualMode);
                    }
                }
                catch (Exception ex)
                {
                    result.Status = $"失败：{ex.Message}";
                }

                batchResults.Add(result);
            }

            var successRows = batchResults
                .Where(r => r.Coverage.HasValue
                            && r.Uniformity.HasValue
                            && !double.IsNaN(r.Uniformity.Value)
                            && r.Coverage.Value > 0)
                .ToList();

            var averages = successRows
                .GroupBy(r => string.IsNullOrWhiteSpace(r.BatchName) ? "未知目录" : r.BatchName)
                .OrderBy(g => g.Key)
                .Select(g => new AlgorithmDirectoryAverage
                {
                    BatchName = g.Key,
                    CoverageAvg = g.Average(x => x.Coverage.Value),
                    UniformityAvg = g.Average(x => x.Uniformity.Value),
                    SampleCount = g.Count()
                })
                .ToList();

            return new AlgorithmBatchResponse
            {
                BatchResults = batchResults,
                DirectoryAverages = averages
            };
        }
    }
}
