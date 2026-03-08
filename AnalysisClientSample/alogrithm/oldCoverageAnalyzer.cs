using System;
using System.IO;
using OpenCvSharp;

namespace ImageAnalysis
{
    public class oldCoverageAnalyzer
    {
        /// <summary>
        /// 覆盖率分析结果
        /// </summary>
        public class CoverageResult
        {
            public double CoveragePercentage { get; set; }  // 覆盖率百分比
            public int TotalArea { get; set; }              // 总面积（像素）
            public int GrayArea { get; set; }               // 灰色区域面积（像素）
            public int CoveredArea { get; set; }            // 覆盖区域面积（像素）
            public bool Success { get; set; }               // 分析是否成功
            public string Message { get; set; }             // 结果消息
            
            public CoverageResult()
            {
                Success = false;
                Message = "";
            }
        }

        /// <summary>
        /// 连接轮廓的方法
        /// </summary>
        public enum ContourConnectionMethod
        {
            Dilation,   // 膨胀法
            Distance    // 距离变换法
        }

        /// <summary>
        /// 计算图像覆盖率（从文件路径）
        /// </summary>
        /// <param name="imagePath">图像文件路径</param>
        /// <param name="sThreshold">饱和度阈值（默认50）</param>
        /// <param name="vMin">亮度最小值（默认40）</param>
        /// <param name="vMax">亮度最大值（默认200）</param>
        /// <param name="method">轮廓连接方法（默认膨胀）</param>
        /// <param name="kernelSize">膨胀核大小（默认5）</param>
        /// <param name="maxGap">最大间隙（默认10）</param>
        /// <returns>覆盖率分析结果</returns>
        public static CoverageResult CalculateCoverage(string imagePath, 
            int sThreshold = 50, int vMin = 40, int vMax = 200,
            ContourConnectionMethod method = ContourConnectionMethod.Dilation,
            int kernelSize = 5, int maxGap = 10)
        {
            var result = new CoverageResult();
            
            try
            {
                // 1. 加载图像
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Color);
                if (image.Empty())
                {
                    result.Message = "图像无法加载，请检查路径";
                    return result;
                }

                // 使用Mat重载方法
                var matResult = CalculateCoverage(image, sThreshold, vMin, vMax, method, kernelSize, maxGap);
                
                // 清理资源
                image.Dispose();
                
                return matResult;
            }
            catch (Exception ex)
            {
                result.Message = $"分析过程中发生错误: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 计算图像覆盖率（从Mat对象）
        /// </summary>
        /// <param name="image">输入图像Mat对象</param>
        /// <param name="sThreshold">饱和度阈值（默认50）</param>
        /// <param name="vMin">亮度最小值（默认40）</param>
        /// <param name="vMax">亮度最大值（默认200）</param>
        /// <param name="method">轮廓连接方法（默认膨胀）</param>
        /// <param name="kernelSize">膨胀核大小（默认5）</param>
        /// <param name="maxGap">最大间隙（默认10）</param>
        /// <returns>覆盖率分析结果</returns>
        public static CoverageResult CalculateCoverage(Mat image, 
            int sThreshold = 50, int vMin = 40, int vMax = 200,
            ContourConnectionMethod method = ContourConnectionMethod.Dilation,
            int kernelSize = 5, int maxGap = 10)
        {
            var result = new CoverageResult();
            
            try
            {
                if (image.Empty())
                {
                    result.Message = "输入图像为空";
                    return result;
                }

                // 2. 提取灰色区域
                Mat grayMask = ExtractGrayRegion(image, sThreshold, vMin, vMax);
                
                // 3. 连接轮廓
                Mat processedMask = ConnectContours(grayMask, method, kernelSize, maxGap);
                
                // 4. 计算面积和覆盖率
                result.TotalArea = image.Width * image.Height;
                result.GrayArea = Cv2.CountNonZero(processedMask);
                result.CoveredArea = result.TotalArea - result.GrayArea;
                result.CoveragePercentage = (double)result.CoveredArea / result.TotalArea * 100.0;
                
                result.Success = true;
                result.Message = "覆盖率计算成功";
                
                // 清理资源
                grayMask.Dispose();
                processedMask.Dispose();
                
                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"分析过程中发生错误: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 提取灰色区域
        /// </summary>
        /// <param name="image">输入图像</param>
        /// <param name="sThreshold">饱和度阈值</param>
        /// <param name="vMin">亮度最小值</param>
        /// <param name="vMax">亮度最大值</param>
        /// <returns>灰色区域掩膜</returns>
        private static Mat ExtractGrayRegion(Mat image, int sThreshold, int vMin, int vMax)
        {
            // 转换为HSV色彩空间
            Mat hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            
            // 定义灰色范围
            Scalar lowerGray = new Scalar(0, 0, vMin);
            Scalar upperGray = new Scalar(180, sThreshold, vMax);
            
            // 创建掩膜
            Mat mask = new Mat();
            Cv2.InRange(hsv, lowerGray, upperGray, mask);
            
            // 清理资源
            hsv.Dispose();
            
            return mask;
        }

        /// <summary>
        /// 连接轮廓
        /// </summary>
        /// <param name="mask">输入掩膜</param>
        /// <param name="method">连接方法</param>
        /// <param name="kernelSize">膨胀核大小</param>
        /// <param name="maxGap">最大间隙</param>
        /// <returns>处理后的掩膜</returns>
        private static Mat ConnectContours(Mat mask, ContourConnectionMethod method, int kernelSize, int maxGap)
        {
            Mat processedMask = new Mat();
            
            switch (method)
            {
                case ContourConnectionMethod.Dilation:
                    // 膨胀法
                    Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
                    Cv2.Dilate(mask, processedMask, kernel, iterations: 1);
                    kernel.Dispose();
                    break;
                    
                case ContourConnectionMethod.Distance:
                    // 距离变换法
                    Mat inverseMask = new Mat();
                    Cv2.BitwiseNot(mask, inverseMask);
                    
                    Mat distTransform = new Mat();
                    Cv2.DistanceTransform(inverseMask, distTransform, DistanceTypes.L2,DistanceTransformMasks.Mask5);
                    
                    Cv2.Threshold(distTransform, processedMask, maxGap, 255, ThresholdTypes.Binary);
                    processedMask.ConvertTo(processedMask, MatType.CV_8UC1);
                    
                    // 清理资源
                    inverseMask.Dispose();
                    distTransform.Dispose();
                    break;
                    
                default:
                    mask.CopyTo(processedMask);
                    break;
            }
            
            return processedMask;
        }

        /// <summary>
        /// 批量计算多个图像的覆盖率
        /// </summary>
        /// <param name="imagePaths">图像路径列表</param>
        /// <param name="sThreshold">饱和度阈值</param>
        /// <param name="vMin">亮度最小值</param>
        /// <param name="vMax">亮度最大值</param>
        /// <param name="method">轮廓连接方法</param>
        /// <param name="kernelSize">膨胀核大小</param>
        /// <param name="maxGap">最大间隙</param>
        /// <returns>每个图像的覆盖率结果</returns>
        public static System.Collections.Generic.Dictionary<string, CoverageResult> BatchCalculateCoverage(
            System.Collections.Generic.List<string> imagePaths,
            int sThreshold = 50, int vMin = 40, int vMax = 200,
            ContourConnectionMethod method = ContourConnectionMethod.Dilation,
            int kernelSize = 5, int maxGap = 10)
        {
            var results = new System.Collections.Generic.Dictionary<string, CoverageResult>();
            
            foreach (string imagePath in imagePaths)
            {
                try
                {
                    var result = CalculateCoverage(imagePath, sThreshold, vMin, vMax, method, kernelSize, maxGap);
                    results[Path.GetFileName(imagePath)] = result;
                    
                    if (result.Success)
                    {
                        Console.WriteLine($"✓ {Path.GetFileName(imagePath)}: 覆盖率 {result.CoveragePercentage:F2}%");
                    }
                    else
                    {
                        Console.WriteLine($"✗ {Path.GetFileName(imagePath)}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = new CoverageResult
                    {
                        Success = false,
                        Message = $"处理异常: {ex.Message}"
                    };
                    results[Path.GetFileName(imagePath)] = errorResult;
                    Console.WriteLine($"✗ {Path.GetFileName(imagePath)}: 处理异常");
                }
            }
            
            return results;
        }

        /// <summary>
        /// 可视化分析结果（保存调试图像）
        /// </summary>
        /// <param name="imagePath">图像路径</param>
        /// <param name="outputDir">输出目录</param>
        /// <param name="sThreshold">饱和度阈值</param>
        /// <param name="vMin">亮度最小值</param>
        /// <param name="vMax">亮度最大值</param>
        /// <param name="method">轮廓连接方法</param>
        /// <param name="kernelSize">膨胀核大小</param>
        /// <param name="maxGap">最大间隙</param>
        /// <returns>分析结果</returns>
        public static CoverageResult VisualizeAnalysis(string imagePath, string outputDir = "coverage_debug",
            int sThreshold = 50, int vMin = 40, int vMax = 200,
            ContourConnectionMethod method = ContourConnectionMethod.Dilation,
            int kernelSize = 5, int maxGap = 10)
        {
            try
            {
                // 创建输出目录
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // 加载图像
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Color);
                if (image.Empty())
                {
                    return new CoverageResult { Success = false, Message = "图像加载失败" };
                }

                // 提取灰色区域
                Mat grayMask = ExtractGrayRegion(image, sThreshold, vMin, vMax);
                
                // 连接轮廓
                Mat processedMask = ConnectContours(grayMask, method, kernelSize, maxGap);
                
                // 计算覆盖率
                var result = CalculateCoverage(image, sThreshold, vMin, vMax, method, kernelSize, maxGap);
                
                // 保存调试图像
                string fileName = Path.GetFileNameWithoutExtension(imagePath);
                Cv2.ImWrite(Path.Combine(outputDir, $"{fileName}_original.jpg"), image);
                Cv2.ImWrite(Path.Combine(outputDir, $"{fileName}_gray_mask.jpg"), grayMask);
                Cv2.ImWrite(Path.Combine(outputDir, $"{fileName}_processed_mask.jpg"), processedMask);
                
                // 创建结果可视化图像
                Mat resultImage = image.Clone();
                Mat coloredMask = new Mat();
                Cv2.CvtColor(processedMask, coloredMask, ColorConversionCodes.GRAY2BGR);
                coloredMask.SetTo(new Scalar(0, 0, 255), processedMask); // 红色显示灰色区域
                
                Mat overlay = new Mat();
                Cv2.AddWeighted(resultImage, 0.7, coloredMask, 0.3, 0, overlay);
                Cv2.ImWrite(Path.Combine(outputDir, $"{fileName}_coverage_overlay.jpg"), overlay);
                
                Console.WriteLine($"调试图像已保存到: {outputDir}");
                Console.WriteLine($"覆盖率: {result.CoveragePercentage:F2}%");
                
                // 清理资源
                image.Dispose();
                grayMask.Dispose();
                processedMask.Dispose();
                resultImage.Dispose();
                coloredMask.Dispose();
                overlay.Dispose();
                
                return result;
            }
            catch (Exception ex)
            {
                return new CoverageResult { Success = false, Message = $"可视化分析失败: {ex.Message}" };
            }
        }
    }
} 