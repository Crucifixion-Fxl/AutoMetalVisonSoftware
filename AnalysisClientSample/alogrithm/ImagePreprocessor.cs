using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AutoMetal;
using OpenCvSharp;


namespace ImageAnalysis
{
    public class ImageProcessor
    {
        /// <summary>
        /// 图像处理结果
        /// </summary>
        public class ProcessResult
        {
            public Mat CorrectedImage { get; set; }
            public Mat CroppedImage { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }

            public ProcessResult()
            {
                Success = false;
                Message = "";
            }

            public void Dispose()
            {
                CorrectedImage?.Dispose();
                CroppedImage?.Dispose();
            }
        }

        /// <summary>
        /// 处理图像：进行透视矫正并切除右边50%宽度
        /// </summary>
        /// <param name="imagePath">图像路径</param>
        /// <param name="areaThreshold">面积阈值（默认100000）</param>
        /// <param name="binaryThreshold">二值化阈值（默认50）</param>
        /// <returns>包含矫正图像和裁剪图像的结果</returns>
        public static ProcessResult ProcessImage(string imagePath, double areaThreshold = 1000000, double binaryThreshold = 50)
        {
            var result = new ProcessResult();
            
            try
            {
                // OpenCV对读入的图像大小有限制 因此需要设置环境变量控制
                Environment.SetEnvironmentVariable("OPENCV_IO_MAX_IMAGE_PIXELS", AutoMetalConstants.OPENCV_IO_MAX_IMAGE_PIXELS);

                // 第1步：读取图像并二值化
                Mat image_1 = Cv2.ImRead(imagePath, ImreadModes.Color);

                Mat image = new Mat();


                // 方法2：直接使用Rotate函数（需要OpenCvSharp 4.x以上版本）
                Mat rotated = new Mat();

                // RotateFlags.Rotate90Counterclockwise = 逆时针旋转90度
                Cv2.Rotate(image_1, rotated, RotateFlags.Rotate90Counterclockwise);



                Cv2.Resize(rotated, image, new Size(AutoMetalConstants.scale_width, AutoMetalConstants.scale_height));



                Cv2.ImWrite(imagePath, image);


                if (image.Empty())
                {
                    result.Message = "图像加载失败，请检查路径。";
                    return result;
                }

                Mat gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                Mat binary = new Mat();
                // 固定阈值法
                //Cv2.Threshold(gray, binary, binaryThreshold, 255, ThresholdTypes.Binary);

                // 三角阈值法: 效果更好
                Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Triangle);

                Cv2.ImWrite(@"C:\Users\SOW111\Desktop\FuncTest_thri_binary.jpg", binary);

                // 第2步：查找轮廓并过滤小面积
                Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // 过滤小面积轮廓
                var validContours = contours.Where(cnt => Cv2.ContourArea(cnt) > areaThreshold).ToList();

                if (validContours.Count == 0)
                {
                    result.Message = $"未找到面积大于 {areaThreshold} 的有效轮廓。";
                    image.Dispose();
                    gray.Dispose();
                    binary.Dispose();
                    return result;
                }

                // 找到最大轮廓
                var maxContour = validContours.OrderByDescending(cnt => Cv2.ContourArea(cnt)).First();


                // 第3步：拟合最小外接矩形并透视校正
                var rect = Cv2.MinAreaRect(maxContour);
                Point2f[] boxPoints = Cv2.BoxPoints(rect);


                // 打印四个角点
                Console.WriteLine("Box Corners:");
                for (int i = 0; i < boxPoints.Length; i++)
                {
                    Console.WriteLine($"Point {i + 1}: X = {boxPoints[i].X}, Y = {boxPoints[i].Y}");
                }

                // 对矩形点进行排序
                Point2f[] orderedBox = OrderPoints(boxPoints);

                // 打印四个角点
                Console.WriteLine("orderedBox Corners:");
                for (int i = 0; i < orderedBox.Length; i++)
                {
                    Console.WriteLine($"Point {i + 1}: X = {orderedBox[i].X}, Y = {orderedBox[i].Y}");
                }

                // 计算目标矩形的宽度和高度
                double height = Distance(orderedBox[0], orderedBox[1]);
                double width = Distance(orderedBox[0], orderedBox[3]);

                // 定义目标矩形的宽度和高度
                Point2f[] dstPoints = new Point2f[]
                {
                    new Point2f(0, 0),
                    new Point2f(0, (float)height - 1),
                    new Point2f((float)width - 1, (float)height - 1),
                    new Point2f((float)width - 1, 0)
                };

                // 透视变换
                Mat transformMatrix = Cv2.GetPerspectiveTransform(orderedBox, dstPoints);
                Mat warped = new Mat();
                Cv2.WarpPerspective(image, warped, transformMatrix, new Size((int)width, (int)height));

                // 第4步：同时切除左边10%、右边50%，以及高度方向两端各10%
                int originalWidth = warped.Width;
                int originalHeight = warped.Height;

                int cutLeftWidth = (int)(originalWidth * 0.07);  // 左边切除10%
                int cutRightWidth = originalWidth / 2;          // 右边切除50%
                int cutTopHeight = (int)(originalHeight * 0.03); // 上边切除10%
                int cutBottomHeight = (int)(originalHeight * 0.03); // 下边切除10%

                // 计算最终保留的宽度和高度
                int finalWidth = cutRightWidth - cutLeftWidth;
                int finalHeight = originalHeight - cutTopHeight - cutBottomHeight;

                // 直接创建包含所有裁剪的矩形区域
                Rect finalCropRect = new Rect(cutLeftWidth, cutTopHeight, finalWidth, finalHeight);
                Mat cropped = new Mat(warped, finalCropRect);

                // 边界裁剪（如果需要的话，这里使用相对较小的边界裁剪值）
                Rect roi = new Rect(
                    AutoMetalConstants.clipLeft,
                    AutoMetalConstants.clipTop,
                    finalWidth - AutoMetalConstants.clipLeft - AutoMetalConstants.clipRight,
                    finalHeight - AutoMetalConstants.clipTop - AutoMetalConstants.clipBottom
                );
                cropped = new Mat(cropped, roi);


                // 设置结果
                result.CorrectedImage = warped.Clone();
                result.CroppedImage = cropped.Clone();
                result.Success = true;
                result.Message = "图像处理成功。";

                // 清理资源
                image.Dispose();
                gray.Dispose();
                binary.Dispose();
                transformMatrix.Dispose();
                warped.Dispose();
                cropped.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"图像处理过程中发生错误: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 处理图像（从Mat对象）
        /// </summary>
        /// <param name="inputImage">输入图像Mat对象</param>
        /// <param name="areaThreshold">面积阈值</param>
        /// <param name="binaryThreshold">二值化阈值</param>
        /// <returns>处理结果</returns>
        public static ProcessResult ProcessImage(Mat inputImage, double areaThreshold = 100000, double binaryThreshold = 10)
        {
            var result = new ProcessResult();
            
            try
            {
                if (inputImage.Empty())
                {
                    result.Message = "输入图像为空。";
                    return result;
                }

                // 第1步：转换为灰度并二值化
                Mat gray = new Mat();
                Cv2.CvtColor(inputImage, gray, ColorConversionCodes.BGR2GRAY);

                Mat binary = new Mat();
                Cv2.Threshold(gray, binary, binaryThreshold, 255, ThresholdTypes.Binary);

                // 第2步：查找轮廓并过滤小面积
                Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                var validContours = contours.Where(cnt => Cv2.ContourArea(cnt) > areaThreshold).ToList();

                if (validContours.Count == 0)
                {
                    result.Message = $"未找到面积大于 {areaThreshold} 的有效轮廓。";
                    gray.Dispose();
                    binary.Dispose();
                    return result;
                }

                var maxContour = validContours.OrderByDescending(cnt => Cv2.ContourArea(cnt)).First();

                // 第3步：透视校正
                var rect = Cv2.MinAreaRect(maxContour);
                Point2f[] boxPoints = Cv2.BoxPoints(rect);
                Point2f[] orderedBox = OrderPoints(boxPoints);

                double width = Distance(orderedBox[0], orderedBox[1]);
                double height = Distance(orderedBox[0], orderedBox[3]);

                Point2f[] dstPoints = new Point2f[]
                {
                    new Point2f(0, 0),
                    new Point2f((float)width - 1, 0),
                    new Point2f((float)width - 1, (float)height - 1),
                    new Point2f(0, (float)height - 1)
                };

                Mat transformMatrix = Cv2.GetPerspectiveTransform(orderedBox, dstPoints);
                Mat warped = new Mat();
                Cv2.WarpPerspective(inputImage, warped, transformMatrix, new Size((int)width, (int)height));

                // 第4步：切除右边50%
                int cutWidth = warped.Width / 2;
                Rect cropRect = new Rect(0, 0, cutWidth, warped.Height);
                Mat cropped = new Mat(warped, cropRect);

                result.CorrectedImage = warped.Clone();
                result.CroppedImage = cropped.Clone();
                result.Success = true;
                result.Message = "图像处理成功。";

                // 清理资源
                gray.Dispose();
                binary.Dispose();
                transformMatrix.Dispose();
                warped.Dispose();
                cropped.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"图像处理过程中发生错误: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 批量处理图像
        /// </summary>
        /// <param name="imagePaths">图像路径列表</param>
        /// <param name="outputDir">输出目录</param>
        /// <param name="areaThreshold">面积阈值</param>
        /// <param name="binaryThreshold">二值化阈值</param>
        /// <returns>处理结果统计</returns>
        public static Dictionary<string, bool> BatchProcessImages(List<string> imagePaths, string outputDir, 
            double areaThreshold = 100000, double binaryThreshold = 10)
        {
            var results = new Dictionary<string, bool>();
            
            foreach (string imagePath in imagePaths)
            {
                try
                {
                    var result = ProcessImage(imagePath, areaThreshold, binaryThreshold);
                    
                    if (result.Success)
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(imagePath);
                        string correctedPath = System.IO.Path.Combine(outputDir, $"{fileName}_corrected.jpg");
                        string croppedPath = System.IO.Path.Combine(outputDir, $"{fileName}_cropped.jpg");
                        
                        Cv2.ImWrite(correctedPath, result.CorrectedImage);
                        Cv2.ImWrite(croppedPath, result.CroppedImage);
                        
                        results[imagePath] = true;
                        Console.WriteLine($"✓ 成功处理: {imagePath}");
                    }
                    else
                    {
                        results[imagePath] = false;
                        Console.WriteLine($"✗ 处理失败: {imagePath} - {result.Message}");
                    }
                    
                    result.Dispose();
                }
                catch (Exception ex)
                {
                    results[imagePath] = false;
                    Console.WriteLine($"✗ 处理异常: {imagePath} - {ex.Message}");
                }
            }
            
            return results;
        }

        /// <summary>
        /// 对矩形的四个顶点进行排序
        /// </summary>
        /// <param name="points">四个顶点</param>
        /// <returns>排序后的顶点：左上、右上、右下、左下</returns>
        private static Point2f[] OrderPoints(Point2f[] points)
        {
            var ordered = new Point2f[4];
            
            // 计算和与差
            var sums = points.Select(p => p.X + p.Y).ToArray();
            var diffs = points.Select(p => p.X - p.Y).ToArray();
            
            // 左上角：和最小
            int topLeftIndex = Array.IndexOf(sums, sums.Min());
            ordered[0] = points[topLeftIndex];
            
            // 右下角：和最大
            int bottomRightIndex = Array.IndexOf(sums, sums.Max());
            ordered[2] = points[bottomRightIndex];
            
            // 右上角：差最小
            int topRightIndex = Array.IndexOf(diffs, diffs.Min());
            ordered[1] = points[topRightIndex];
            
            // 左下角：差最大
            int bottomLeftIndex = Array.IndexOf(diffs, diffs.Max());
            ordered[3] = points[bottomLeftIndex];
            
            return ordered;
        }

        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        /// <param name="p1">第一个点</param>
        /// <param name="p2">第二个点</param>
        /// <returns>距离</returns>
        private static double Distance(Point2f p1, Point2f p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// 可视化处理过程（调试用）
        /// </summary>
        /// <param name="imagePath">图像路径</param>
        /// <param name="areaThreshold">面积阈值</param>
        /// <param name="binaryThreshold">二值化阈值</param>
        public static void VisualizeProcess(string imagePath, double areaThreshold = 100000, double binaryThreshold = 10)
        {
            try
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Color);
                if (image.Empty())
                {
                    Console.WriteLine("图像加载失败。");
                    return;
                }

                // 转换为灰度并二值化
                Mat gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                Mat binary = new Mat();
                Cv2.Threshold(gray, binary, binaryThreshold, 255, ThresholdTypes.Binary);

                // 查找轮廓
                Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                var validContours = contours.Where(cnt => Cv2.ContourArea(cnt) > areaThreshold).ToList();

                if (validContours.Count == 0)
                {
                    Console.WriteLine($"未找到面积大于 {areaThreshold} 的有效轮廓。");
                    return;
                }

                var maxContour = validContours.OrderByDescending(cnt => Cv2.ContourArea(cnt)).First();

                // 绘制轮廓和矩形
                Mat visualImage = image.Clone();
                var rect = Cv2.MinAreaRect(maxContour);
                Point2f[] boxPoints = Cv2.BoxPoints(rect);
                Point[] intBoxPoints = boxPoints.Select(p => new Point((int)p.X, (int)p.Y)).ToArray();
                
                Cv2.DrawContours(visualImage, new Point[][] { maxContour }, -1, new Scalar(255, 0, 0), 3);
                Cv2.DrawContours(visualImage, new Point[][] { intBoxPoints }, -1, new Scalar(0, 255, 0), 3);

                // 保存中间结果
                Cv2.ImWrite("debug_original.jpg", image);
                Cv2.ImWrite("debug_binary.jpg", binary);
                Cv2.ImWrite("debug_contours.jpg", visualImage);

                Console.WriteLine("调试图像已保存：debug_original.jpg, debug_binary.jpg, debug_contours.jpg");

                // 清理资源
                image.Dispose();
                gray.Dispose();
                binary.Dispose();
                visualImage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"可视化过程中发生错误: {ex.Message}");
            }
        }
    }
} 