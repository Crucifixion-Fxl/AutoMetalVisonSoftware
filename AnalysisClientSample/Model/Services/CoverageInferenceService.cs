using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMetal.Algorithms;
using OpenCvSharp;

namespace AutoMetal.Services
{
    internal class CoverageInferenceService : ICoverageInferenceService
    {
        private readonly Action<string> _log;

        public CoverageInferenceService(Action<string> logCallback)
        {
            _log = logCallback ?? (_ => { });
        }

        public (bool success, int total, int successCount, int failedCount, string errorMessage) RunCoverageBatch(
            string testSetDir,
            string enginePath,
            Action<int, int> progressCallback,
            Action<string> statusCallback)
        {
            if (string.IsNullOrWhiteSpace(testSetDir) || !Directory.Exists(testSetDir))
            {
                return (false, 0, 0, 0, "请先选择有效的测试集路径");
            }
            if (string.IsNullOrWhiteSpace(enginePath) || !File.Exists(enginePath))
            {
                return (false, 0, 0, 0, "请先选择有效的Engine文件路径");
            }

            try
            {
                CoverageAnalyzer.SetModelPath(enginePath);
                _log($"已切换Coverage模型Engine：{enginePath}");
            }
            catch (Exception ex)
            {
                return (false, 0, 0, 0, $"Engine加载失败：{ex.Message}");
            }

            var supportedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff"
            };
            var images = Directory.EnumerateFiles(testSetDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => supportedExt.Contains(Path.GetExtension(p)))
                .OrderBy(p => p)
                .ToList();
            if (images.Count == 0)
            {
                return (false, 0, 0, 0, "测试集目录中没有可推理图像");
            }

            int successCount = 0;
            int failedCount = 0;
            progressCallback?.Invoke(0, images.Count);

            for (int i = 0; i < images.Count; i++)
            {
                string imagePath = images[i];
                string fileName = Path.GetFileName(imagePath);
                try
                {
                    using (var image = Cv2.ImRead(imagePath))
                    {
                        if (image.Empty())
                        {
                            failedCount++;
                            _log($"推理失败(图像读取为空): {fileName}");
                        }
                        else
                        {
                            string outputPath = Path.Combine(
                                Path.GetDirectoryName(imagePath),
                                Path.GetFileNameWithoutExtension(imagePath) + "_mask" + Path.GetExtension(imagePath));
                            double coverage = CoverageAnalyzer.detectImage(imagePath, null, outputPath);
                            successCount++;
                            _log($"推理完成: {fileName}, 覆盖率={coverage:F6}, 输出={outputPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _log($"推理异常: {fileName}, 错误={ex.Message}");
                }

                progressCallback?.Invoke(i + 1, images.Count);
                statusCallback?.Invoke($"推理状态：进行中 {i + 1}/{images.Count}");
            }

            statusCallback?.Invoke($"推理状态：完成 成功{successCount} 失败{failedCount}");
            _log($"测试集批量推理结束，总数={images.Count}，成功={successCount}，失败={failedCount}");
            return (true, images.Count, successCount, failedCount, string.Empty);
        }
    }
}
