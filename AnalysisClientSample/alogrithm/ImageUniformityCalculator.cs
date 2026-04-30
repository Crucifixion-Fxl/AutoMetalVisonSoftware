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
        public class MaskUniformityResult
        {
            public double MuMaskRaw { get; set; }
            public double MuCorrected { get; set; }
            public double SigmaCorrected { get; set; }
            public double UniformityU { get; set; }
            public int CoatingPixelCount { get; set; }
            public string HeatmapPath { get; set; }
        }

        /// <summary>
        /// 仅保留mask版本均匀性接口：直接返回 UniformityU
        /// </summary>
        public static double CalculateUniformity(
            string imagePath,
            string maskPath,
            int gaussianKsize = 101,
            double gaussianSigma = 0.0,
            bool applyIlluminationCorrection = false)
        {
            return CalculateUniformityByMask(
                imagePath,
                maskPath,
                gaussianKsize,
                gaussianSigma,
                applyIlluminationCorrection).UniformityU;
        }

        /// <summary>
        /// 基于mask的均匀性计算（与Python版规则一致）：
        /// 1) mask非纯黑色(>0)为统计区
        /// 2) 无mask时默认整图为镀膜区
        /// 3) 仅在镀膜区统计均值/方差/均匀性
        /// 4) U = 1 - sigma / mu
        /// </summary>
        public static MaskUniformityResult CalculateUniformityByMask(
            string imagePath,
            string maskPath = null,
            int gaussianKsize = 101,
            double gaussianSigma = 0.0,
            bool applyIlluminationCorrection = false)
        {
            if (gaussianKsize <= 0 || gaussianKsize % 2 == 0)
            {
                throw new ArgumentException("gaussianKsize 必须是正奇数");
            }

            Mat imageGray = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
            if (imageGray.Empty())
            {
                throw new ArgumentException($"图像加载失败，请检查路径: {imagePath}");
            }

            Mat coatingMask = BuildCoatingMask(imageGray.Size(), maskPath);
            int coatingPixelCount = Cv2.CountNonZero(coatingMask);
            if (coatingPixelCount == 0)
            {
                imageGray.Dispose();
                coatingMask.Dispose();
                return new MaskUniformityResult
                {
                    MuMaskRaw = double.NaN,
                    MuCorrected = double.NaN,
                    SigmaCorrected = double.NaN,
                    UniformityU = double.NaN,
                    CoatingPixelCount = 0,
                    HeatmapPath = string.Empty
                };
            }

            // 镀膜区原始均值（仅镀膜区参与）
            double muMaskRaw = Cv2.Mean(imageGray, coatingMask).Val0;

            Mat imageFloat = new Mat();
            imageGray.ConvertTo(imageFloat, MatType.CV_64FC1);

            Mat corrected = imageFloat.Clone();
            if (applyIlluminationCorrection)
            {
                // 非镀膜区填充为muMaskRaw，用于估计背景
                Mat filled = imageFloat.Clone();
                Mat nonCoatingMask = new Mat();
                Cv2.BitwiseNot(coatingMask, nonCoatingMask);
                filled.SetTo(new Scalar(muMaskRaw), nonCoatingMask);

                Mat background = new Mat();
                Cv2.GaussianBlur(filled, background, new Size(gaussianKsize, gaussianKsize), gaussianSigma, gaussianSigma);

                // corrected = image - background + muMaskRaw（仅镀膜区）
                Mat temp = new Mat();
                Cv2.Subtract(imageFloat, background, temp);
                Cv2.Add(temp, new Scalar(muMaskRaw), temp);
                temp.CopyTo(corrected, coatingMask);
                Cv2.Min(corrected, new Scalar(255.0), corrected);
                Cv2.Max(corrected, new Scalar(0.0), corrected);

                temp.Dispose();
                background.Dispose();
                nonCoatingMask.Dispose();
                filled.Dispose();
            }

            // 仅镀膜区统计 corrected 的均值与方差
            Cv2.MeanStdDev(corrected, out Scalar muScalar, out Scalar stdScalar, coatingMask);
            double muCorrected = muScalar.Val0;
            double sigmaCorrected = stdScalar.Val0;
            double uniformityU = Math.Abs(muCorrected) > 1e-12
                ? 1.0 - sigmaCorrected / muCorrected
                : double.NaN;

            imageGray.Dispose();
            imageFloat.Dispose();
            corrected.Dispose();
            coatingMask.Dispose();

            return new MaskUniformityResult
            {
                MuMaskRaw = muMaskRaw,
                MuCorrected = muCorrected,
                SigmaCorrected = sigmaCorrected,
                UniformityU = uniformityU,
                CoatingPixelCount = coatingPixelCount,
                HeatmapPath = string.Empty
            };
        }

        private static Mat BuildCoatingMask(OpenCvSharp.Size imageSize, string maskPath)
        {
            // 仅处理有mask版本
            if (string.IsNullOrWhiteSpace(maskPath) || !File.Exists(maskPath))
            {
                throw new ArgumentException("mask文件不存在，当前版本仅支持有mask计算。");
            }

            Mat maskGray = Cv2.ImRead(maskPath, ImreadModes.Grayscale);
            if (maskGray.Empty())
            {
                throw new ArgumentException($"mask读取失败: {maskPath}");
            }

            if (maskGray.Size() != imageSize)
            {
                throw new ArgumentException($"原图与mask尺寸不一致: image={imageSize}, mask={maskGray.Size()}");
            }

            // 业务规则：非纯黑色区域参与均匀性统计（黑色区域不参与）
            Mat coatingMask = new Mat();
            Cv2.Threshold(maskGray, coatingMask, 0, 255, ThresholdTypes.Binary);
            maskGray.Dispose();
            return coatingMask;
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