using AutoMetal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMetal;
using TensorRtSharp.Custom;
using OpenCvSharp;
using System.Drawing.Printing;
using NumSharp;
using System.IO;
using TrtCommon;

namespace ImageAnalysis
{
    public class CoverageAnalyzer
    {
        // 算法模型基于deepLabV3+，模型输入尺寸 512x512，输出2通道
        public static string modelPath = AutoMetalConstants.deeplabv3PlusEnginePath;
        public static Nvinfer predictor = new Nvinfer(modelPath);



        public static double detectImage(string imagePath)
        {
            // 读取并转换颜色
            Mat image = Cv2.ImRead(imagePath);
            Cv2.CvtColor(image, image, ColorConversionCodes.BGR2RGB);

            Mat oldImg = image.Clone();

            int orininal_h = image.Height;
            int orininal_w = image.Width;

            // resize
            Mat image_data = new Mat();
            Cv2.Resize(image, image_data, new Size(512, 512), 0, 0, InterpolationFlags.Cubic);

            // 转 float
            float[] inputdata = MatToNormalizedFloatArray(image_data);

            // 推理
            predictor.LoadInferenceData("images", inputdata);
            predictor.infer();

            float[] outputRes = predictor.GetInferenceResult("output");

            Mat mask = CreateMaskFromSoftmax(outputRes);

            // mask resize 回原尺寸
            Cv2.Resize(mask, mask, new Size(orininal_w, orininal_h), 0, 0, InterpolationFlags.Cubic);

            Mat blended = new Mat();
            Cv2.CvtColor(image, image, ColorConversionCodes.RGB2BGR);
            Cv2.AddWeighted(image, 0.6, mask, 0.4, 0.0, blended);

            // =========================
            // ⭐ 生成保存路径
            // =========================
            string dir = Path.GetDirectoryName(imagePath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
            string ext = Path.GetExtension(imagePath);

            string savePath = Path.Combine(dir, nameWithoutExt + "_mask" + ext);

            Cv2.ImWrite(savePath, blended);

            return getRatio(mask);
        }

        public static double getRatio(Mat image)
        {
            if (image.Empty())
                throw new ArgumentException("Input Mat is empty");

            if (image.Channels() != 3)
                throw new ArgumentException("Input Mat must be a 3-channel image");

            int totalPixels = image.Rows * image.Cols;

            // 创建单通道 mask，黑色像素设为 255，其他为 0
            Mat mask = new Mat();
            Cv2.InRange(image, new Scalar(0, 0, 0), new Scalar(0, 0, 0), mask);

            // 统计黑色像素个数
            int blackPixels = Cv2.CountNonZero(mask);

            // 返回比例
            return (double)blackPixels / totalPixels;
        }


        public static Mat CreateMaskFromSoftmax(float[] modelOutput, int width = 512, int height = 512)
        {
            if (modelOutput.Length != 2 * width * height)
            {
                throw new ArgumentException("输入数组长度必须为 2 * width * height");
            }

            // 1. 应用softmax
            float[] softmaxOutput = ApplySoftmaxToModelOutput(modelOutput, 2, width, height);

            // 创建mask图像
            Mat mask = new Mat(height, width, MatType.CV_8UC3, new Scalar(0, 0, 0));

            // 遍历每个像素位置
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    // 获取两个类别的概率值
                    float probClass0 = softmaxOutput[index];                    // 第一个类别概率
                    float probClass1 = softmaxOutput[index + width * height];   // 第二个类别概率

                    // 选择概率较大的类别
                    if (probClass1 > probClass0)  // 假设类别1是我们感兴趣的目标
                    {
                        mask.Set(y, x, new Vec3b(0, 0, 0));
                    }
                    else
                    {
                        mask.Set(y, x, new Vec3b(255, 0, 255));  // 设置为紫色
                    }
                }
            }

            // 

            //Cv2.ImWrite(@"C:\Users\SOW111\Desktop\mask.jpg",mask);



            return mask;
        }

        public static float[] ApplySoftmaxToModelOutput(float[] modelOutput, int numClasses, int height, int width)
        {
            float[] result = new float[height * width * numClasses];

            // 遍历每个像素
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 先找最大值（数值稳定性）
                    float maxVal = float.NegativeInfinity;
                    for (int c = 0; c < numClasses; c++)
                    {
                        int idx = c * height * width + y * width + x; // CHW
                        if (modelOutput[idx] > maxVal)
                            maxVal = modelOutput[idx];
                    }

                    // 计算 exp 和
                    float sum = 0f;
                    for (int c = 0; c < numClasses; c++)
                    {
                        int idx = c * height * width + y * width + x;
                        sum += (float)Math.Exp(modelOutput[idx] - maxVal);
                    }

                    // 计算 softmax
                    for (int c = 0; c < numClasses; c++)
                    {
                        int idx = c * height * width + y * width + x;
                        result[idx] = (float)Math.Exp(modelOutput[idx] - maxVal) / sum;
                    }
                }
            }

            return result;
        }

        public static float[] MatToNormalizedFloatArray(Mat mat, bool normalize = true)
        {
            if (mat.Empty())
                throw new ArgumentException("Mat is empty");

            int channels = mat.Channels();  // 应该是 3
            int height = mat.Height;
            int width = mat.Width;
            int hw = height * width;

            float[] floatArray = new float[channels * hw];

            // CHW: [R平面][G平面][B平面]
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec3b pixel = mat.At<Vec3b>(y, x); // 已经是 RGB 顺序: [R,G,B]
                    int idx = y * width + x;

                    if (normalize)
                    {
                        floatArray[0 * hw + idx] = pixel.Item0 / 255.0f; // R
                        floatArray[1 * hw + idx] = pixel.Item1 / 255.0f; // G
                        floatArray[2 * hw + idx] = pixel.Item2 / 255.0f; // B
                    }
                    else
                    {
                        floatArray[0 * hw + idx] = pixel.Item0;
                        floatArray[1 * hw + idx] = pixel.Item1;
                        floatArray[2 * hw + idx] = pixel.Item2;
                    }
                }
            }

            return floatArray;
        }

    }
}
