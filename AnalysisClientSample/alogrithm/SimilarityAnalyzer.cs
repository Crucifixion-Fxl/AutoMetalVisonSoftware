using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalysis
{
    public class SimilarityAnalyzer
    {
        //public static double Compare_SSIM(string imageFilePath_1, string imageFilePath_2)
        //{
        //    var image1 = Cv2.ImRead(imageFilePath_1);
        //    var image2Tmp = Cv2.ImRead(imageFilePath_2);

        //    // 确保相同大小
        //    var image2 = new Mat();
        //    Cv2.Resize(image2Tmp, image2, new OpenCvSharp.Size(image1.Size().Width, image1.Size().Height));

        //    // 转换为浮点数
        //    var validImage1 = new Mat();
        //    var validImage2 = new Mat();
        //    image1.ConvertTo(validImage1, MatType.CV_32F);
        //    image2.ConvertTo(validImage2, MatType.CV_32F);

        //    // 参数设置
        //    double C1 = 6.5025, C2 = 58.5225;
        //    int kernelSize = 11;
        //    double sigma = 1.5;

        //    // 计算平方和乘积
        //    Mat image1_1 = validImage1.Mul(validImage1);
        //    Mat image2_2 = validImage2.Mul(validImage2);
        //    Mat image1_2 = validImage1.Mul(validImage2);

        //    // 高斯模糊 - 使用相同的边界处理
        //    Mat gausBlur1 = new Mat(), gausBlur2 = new Mat(), gausBlur12 = new Mat();
        //    Cv2.GaussianBlur(validImage1, gausBlur1, new OpenCvSharp.Size(kernelSize, kernelSize), sigma, sigma, BorderTypes.Reflect101);
        //    Cv2.GaussianBlur(validImage2, gausBlur2, new OpenCvSharp.Size(kernelSize, kernelSize), sigma, sigma, BorderTypes.Reflect101);
        //    Cv2.GaussianBlur(image1_2, gausBlur12, new OpenCvSharp.Size(kernelSize, kernelSize), sigma, sigma, BorderTypes.Reflect101);

        //    // 计算平方的均值
        //    Mat squreAvg1 = new Mat(), squreAvg2 = new Mat();
        //    Cv2.GaussianBlur(image1_1, squreAvg1, new OpenCvSharp.Size(kernelSize, kernelSize), sigma, sigma, BorderTypes.Reflect101);
        //    Cv2.GaussianBlur(image2_2, squreAvg2, new OpenCvSharp.Size(kernelSize, kernelSize), sigma, sigma, BorderTypes.Reflect101);

        //    // 计算方差和协方差
        //    Mat u1Squre = gausBlur1.Mul(gausBlur1);
        //    Mat u2Squre = gausBlur2.Mul(gausBlur2);
        //    Mat imageAvgProduct = gausBlur1.Mul(gausBlur2);

        //    Mat imageConvariance = gausBlur12 - imageAvgProduct;
        //    Mat imageVariance1 = squreAvg1 - u1Squre;
        //    Mat imageVariance2 = squreAvg2 - u2Squre;

        //    // 修复问题：将 MatExpr 转换为 Mat 后再进行加法运算
        //    Mat numerator = (imageAvgProduct * 2 + new Scalar(C1)).Mul(imageConvariance * 2 + new Scalar(C2));
        //    Mat denominator = (u1Squre + u2Squre + new Scalar(C1)).Mul(imageVariance1 + imageVariance2 + new Scalar(C2));

        //    Mat ssim = new Mat();
        //    Cv2.Divide(numerator, denominator, ssim);

        //    // 计算平均值
        //    Scalar meanSsim = Cv2.Mean(ssim);

        //    // 添加容差检查
        //    double result = (meanSsim.Val0 + meanSsim.Val1 + meanSsim.Val2) / 3;

        //    // 对于相同图像，结果应该非常接近1（考虑浮点精度）
        //    if (Math.Abs(result - 1.0) < 1e-10)
        //    {
        //        return 1.0;
        //    }

        //    return result;
        //}

        public static double CalculateAverageSSIM(List<string> imagePaths)
        {
            if (imagePaths == null || imagePaths.Count < 2)
            {
                throw new ArgumentException("至少需要2张图像进行比较");
            }

            double totalSSIM = 0;
            int comparisonCount = 0;

            // 计算所有相邻图像对的SSIM
            for (int i = 0; i < imagePaths.Count - 1; i++)
            {
                for (int j = i + 1; j < imagePaths.Count; j++)
                {
                    try
                    {
                        double ssim = getMSSIM(imagePaths[i], imagePaths[j]);

                        Console.WriteLine("计算结果：");
                        Console.WriteLine(ssim);
                        totalSSIM += ssim;
                        comparisonCount++;

                        Console.WriteLine($"图像 {Path.GetFileName(imagePaths[i])} 与 {Path.GetFileName(imagePaths[j])} 的SSIM: {ssim:F6}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"计算 {imagePaths[i]} 和 {imagePaths[j]} 的SSIM时出错: {ex.Message}");
                    }
                }
            }

            if (comparisonCount == 0)
            {
                throw new InvalidOperationException("无法计算任何图像对的SSIM");
            }

            double averageSSIM = totalSSIM / comparisonCount;
            Console.WriteLine($"\n平均SSIM: {averageSSIM:F6} (基于 {comparisonCount} 次比较)");

            return averageSSIM;
        }

        public static double getMSSIM(string imgOnePath, string imgTwoPath)
        {
            Bitmap img1 = SimilarityAnalyzer.BitmapRead(imgOnePath);
            Bitmap img2 = SimilarityAnalyzer.BitmapRead(imgTwoPath);

            Mat i1 = OpenCvSharp.Extensions.BitmapConverter.ToMat(img1);
            Mat i2 = OpenCvSharp.Extensions.BitmapConverter.ToMat(img2);

            // 确保两张图像大小相同
            if (i1.Size() != i2.Size())
            {
                // 调整第二张图像的大小以匹配第一张
                i2 = i2.Resize(i1.Size());
            }


            const double C1 = 6.5025, C2 = 58.5225;
            /***************************** INITS **********************************/
            MatType d = MatType.CV_32F;

            Mat I1 = new Mat(), I2 = new Mat();
            i1.ConvertTo(I1, d);           // cannot calculate on one byte large values
            i2.ConvertTo(I2, d);

            Mat I2_2 = I2.Mul(I2);        // I2^2
            Mat I1_2 = I1.Mul(I1);        // I1^2
            Mat I1_I2 = I1.Mul(I2);        // I1 * I2

            /***********************PRELIMINARY COMPUTING ******************************/

            Mat mu1 = new Mat(), mu2 = new Mat();   //
            Cv2.GaussianBlur(I1, mu1, new OpenCvSharp.Size(11, 11), 1.5);
            Cv2.GaussianBlur(I2, mu2, new OpenCvSharp.Size(11, 11), 1.5);

            Mat mu1_2 = mu1.Mul(mu1);
            Mat mu2_2 = mu2.Mul(mu2);
            Mat mu1_mu2 = mu1.Mul(mu2);

            Mat sigma1_2 = new Mat(), sigma2_2 = new Mat(), sigma12 = new Mat();

            Cv2.GaussianBlur(I1_2, sigma1_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma1_2 -= mu1_2;

            Cv2.GaussianBlur(I2_2, sigma2_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma2_2 -= mu2_2;

            Cv2.GaussianBlur(I1_I2, sigma12, new OpenCvSharp.Size(11, 11), 1.5);
            sigma12 -= mu1_mu2;


            // FORMULA
            Mat t1, t2, t3;

            t1 = 2 * mu1_mu2 + new Scalar(C1);
            t2 = 2 * sigma12 + new Scalar(C2);
            t3 = t1.Mul(t2);              // t3 = ((2*mu1_mu2 + C1).*(2*sigma12 + C2))

            // Fix for CS0019: Convert the double to a MatExpr using the addition of a Scalar
            t1 = mu1_2 + mu2_2 + new Scalar(C1);
            t1 = mu1_2 + mu2_2 + new Scalar(C1);
            // Fix for CS0019: Convert the double to a MatExpr using the addition of a Scalar
            t1 = mu1_2 + mu2_2 + new Scalar(C1);
            t2 = sigma1_2 + sigma2_2 + new Scalar(C2);
            t2 = sigma1_2 + sigma2_2 + new Scalar(C2);
            t1 = t1.Mul(t2);               // t1 =((mu1_2 + mu2_2 + C1).*(sigma1_2 + sigma2_2 + C2))

            Mat ssim_map = new Mat();
            Cv2.Divide(t3, t1, ssim_map);      // ssim_map =  t3./t1;

            Scalar mssim = Cv2.Mean(ssim_map);// mssim = average of ssim map



            SSIMResult result = new SSIMResult();
            result.diff = ssim_map;
            result.mssim = mssim;


            return result.score;
        }

        public static Bitmap BitmapRead(string fileName)
        {
            try
            {
                Bitmap bmp = new Bitmap(Image.FromFile(fileName));
                return bmp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    };

    public class SSIMResult
    {
        public double score
        {
            get
            {
                return (mssim.Val0 + mssim.Val1 + mssim.Val2) / 3;
            }
        }
        public Scalar mssim;
        public Mat diff;
}
}