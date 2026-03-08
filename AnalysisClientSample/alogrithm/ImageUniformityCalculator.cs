using System;
using System.Collections.Generic;
using OpenCvSharp;
using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using System.Linq;
using System.Runtime.CompilerServices;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace ImageAnalysis
{
    public class ImageUniformityCalculator
    {
        /// <summary>
        /// 使用GLCM计算灰度图像的均匀性（能量）
        /// </summary>
        /// <param name="imagePath">图像路径</param>
        /// <param name="levels">灰度级别数（默认16）</param>
        /// <param name="distances">像素距离列表（默认[1]）</param>
        /// <param name="angles">角度列表（弧度）（默认[0, 45°, 90°, 135°]）</param>
        /// <param name="visualize">是否可视化GLCM矩阵</param>
        /// <returns>平均均匀性值（值越高表示越均匀）</returns>
        public static Tuple<double,string> CalculateUniformity(string imagePath, int levels = 16, 
            List<int> distances = null, List<double> angles = null, bool visualize = false)
        {
            // 设置默认值
            if (distances == null)
                distances = new List<int> { 1 };
            
            if (angles == null)
                angles = new List<double> { 0, Math.PI / 4, Math.PI / 2, 3 * Math.PI / 4 };

            // 读取图像并转换为灰度
            Mat img = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
            if (img.Empty())
            {
                throw new ArgumentException("图像加载失败，请检查路径。");
            }

            // 量化灰度值
            Mat imgReduced = new Mat();
            img.ConvertTo(imgReduced, MatType.CV_8UC1, (double)levels / 256.0);

            // 计算GLCM并求能量
            double uniformitySum = 0.0;
            int count = distances.Count * angles.Count;

            // 一个列表存储每个图像的名称

            List<string> uniformityFilepaths = new List<string>();

            foreach (int distance in distances)
            {
                foreach (double angle in angles)
                {
                    double[,] glcm = ComputeGLCM(imgReduced, distance, angle, levels);
                    double energy = ComputeEnergy(glcm);
                    uniformitySum += energy;

                    if (visualize)
                    {
                        //Console.WriteLine($"距离={distance}, 角度={angle * 180 / Math.PI:F0}°, 能量={energy:F4}");
                        //string uniformityFilepath = SaveGLCMMatrixImage(imagePath, sampleId, glcm, distance, angle, count);
                        //uniformityFilepaths.Add(uniformityFilepath);
                    }
                }
            }

            img.Dispose();
            imgReduced.Dispose();

            double averageUniformity = uniformitySum / count;

            // 使用 string.Join 方法，用分号连接
            string joinedString = string.Join(";", uniformityFilepaths);

            return new Tuple<double, string>(averageUniformity, joinedString);
        }

        /// <summary>
        /// 计算GLCM矩阵
        /// </summary>
        /// <param name="image">输入图像</param>
        /// <param name="distance">像素距离</param>
        /// <param name="angle">角度（弧度）</param>
        /// <param name="levels">灰度级别数</param>
        /// <returns>归一化的GLCM矩阵</returns>
        private static double[,] ComputeGLCM(Mat image, int distance, double angle, int levels)
        {
            int rows = image.Rows;
            int cols = image.Cols;
            double[,] glcm = new double[levels, levels];

            // 计算偏移量
            int deltaX = (int)Math.Round(distance * Math.Cos(angle));
            int deltaY = (int)Math.Round(distance * Math.Sin(angle));

            int totalPairs = 0;

            unsafe
            {
                // 获取图像数据
                byte* data = (byte*)image.DataPointer;
                int step = (int)image.Step();

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int newI = i + deltaY;
                        int newJ = j + deltaX;

                        // 检查边界
                        if (newI >= 0 && newI < rows && newJ >= 0 && newJ < cols)
                        {
                            int pixel1 = data[i * step + j];
                            int pixel2 = data[newI * step + newJ];

                            // 确保像素值在有效范围内
                            pixel1 = Math.Min(pixel1, levels - 1);
                            pixel2 = Math.Min(pixel2, levels - 1);

                            glcm[pixel1, pixel2]++;
                            totalPairs++;
                        }
                    }
                }
            }

            // 归一化矩阵
            if (totalPairs > 0)
            {
                for (int i = 0; i < levels; i++)
                {
                    for (int j = 0; j < levels; j++)
                    {
                        glcm[i, j] /= totalPairs;
                    }
                }
            }

            return glcm;
        }

        /// <summary>
        /// 计算GLCM矩阵的能量（均匀性）
        /// </summary>
        /// <param name="glcm">GLCM矩阵</param>
        /// <returns>能量值</returns>
        private static double ComputeEnergy(double[,] glcm)
        {
            double energy = 0.0;
            int levels = glcm.GetLength(0);

            for (int i = 0; i < levels; i++)
            {
                for (int j = 0; j < levels; j++)
                {
                    energy += glcm[i, j] * glcm[i, j];
                }
            }

            return energy;
        }

        /// <summary>
        /// 打印GLCM矩阵（用于调试）
        /// </summary>
        /// <param name="glcm">GLCM矩阵</param>
        /// <param name="distance">距离</param>
        /// <param name="angle">角度</param>
        private static string SaveGLCMMatrixImage(string imagePath,string sampleID, double[,] glcm, int distance, double angle,int cout)
        {

            int levels = glcm.GetLength(0);

            var model = new PlotModel
            {
                Title = $"GLCM矩阵 (距离={distance}, 角度={angle * 180 / Math.PI:F0}°)"
            };


            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "grayLevel_b",
                ItemsSource = Enumerable.Range(1, levels).Select(k => k.ToString()).ToArray()
            });

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "grayLevel_l",
                ItemsSource = Enumerable.Range(1, levels).Select(k => k.ToString()).ToArray()
            });


            model.Axes.Add(new LinearColorAxis
            {
                Palette = OxyPalettes.Hot(100)

            });

            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 6,
                Y0 = 0,
                Y1 = 4,
                XAxisKey = "grayLevel_b",
                YAxisKey = "grayLevel_l",
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.2,
                Data = glcm
            };

            model.Series.Add(heatMapSeries);


            var pngExporter = new PngExporter { Width = 600, Height = 400};

            string parentDirectory = Path.GetDirectoryName(imagePath);

            string fileName = Path.GetFileName(imagePath);

            fileName = parentDirectory + "\\" +
                fileName + "_" + distance + "_" + angle + ".svg";

            using (var stream = File.Create(fileName))
            {
                var exporter = new OxyPlot.SvgExporter { Width = 600, Height = 400 };
                exporter.Export(model, stream);
            }

            return fileName;

        }
    }
} 