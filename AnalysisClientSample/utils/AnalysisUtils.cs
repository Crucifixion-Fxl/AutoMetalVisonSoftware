using OpenCvSharp.Dnn;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TensorRtSharp.Custom;
using TensorRtSharp;
using System.Security.Policy;
using System.IO;
using TrtCommon;    
using System.Runtime.CompilerServices;
using OpenCvSharp.Internal.Vectors;
using System.Security.Cryptography;

namespace AutoMetal
{
    public static class AnalysisUtils
    {
        public static Tuple<List<SegResult>, Mat, Dictionary<int, double>> SegInfer(String model_path, List<Mat> images, string outImgPath)
        {
            Yolov8Seg yolov8Seg = new Yolov8Seg(model_path);
            List<SegResult> segResults = yolov8Seg.Predict(images); // 如果分割不出来任何的异常，这里的segRes就会报错
            Dictionary<int, double> ratioMap = getSegRatio(segResults[0]);
            Mat re_image1 = Visualize.DrawSegResult(segResults[0], images[0],SegType.Coat);

            // 保存图像的位置
            Console.WriteLine();
            Cv2.ImWrite(outImgPath, re_image1);
            return Tuple.Create(segResults, re_image1, ratioMap);
        }

        // 遍历所有类别并更新类别占比
        public static Dictionary<int, double> getSegRatio(SegResult segResult)
        {
            Dictionary<int, double> ratioMap = new Dictionary<int, double>();

            for (int i = 0; i < segResult.count; i++)
            {
                // 计算比例并进行更新
                ratioMap.Add(segResult.datas[i].index, CountNonZeroPixels(segResult.datas[i].mask));

                Console.WriteLine("类别:" + MetalOption.labels[segResult.datas[i].index] + ",比例:" + CountNonZeroPixels(segResult.datas[i].mask));
            }

            return ratioMap;
        }

        public static double CountNonZeroPixels(Mat mat)
        {
            if (mat.Channels() != 3)
            {
                throw new ArgumentException("输入必须是3通道Mat");
            }

            int count = 0;

            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    Vec3b pixel = mat.At<Vec3b>(i, j);
                    if (pixel.Item0 != 0 || pixel.Item1 != 0 || pixel.Item2 != 0)
                    {
                        count++;
                    }
                }
            }
            return 1.0 * count / (mat.Rows * mat.Cols);
        }


        public static Tuple<int, float> Infer(String model_path, List<Mat> images)
        {  
            Nvinfer predictor = new Nvinfer(model_path);
            Dims InputDims = predictor.GetBindingDimensions("images");
            int BatchNum = InputDims.d[0];

            Tuple<int, float> final_res = null; // Initialize the variable to avoid CS0165 error  
            for (int begImgNo = 0; begImgNo < images.Count; begImgNo += BatchNum)
            {
                DateTime start = DateTime.Now;
                int endImgNo = Math.Min(images.Count, begImgNo + BatchNum);
                int batchNum = endImgNo - begImgNo;
                List<Mat> normImgBatch = new List<Mat>();
                int imageLen = 3 * 640 * 640;
                float[] inputData = new float[2 * imageLen];
                for (int ino = begImgNo; ino < endImgNo; ino++)
                {
                    Mat input_mat = CvDnn.BlobFromImage(images[ino], 1.0 / 255.0, new OpenCvSharp.Size(640, 640), (Scalar)0, true, false);
                    float[] data = new float[imageLen];
                    Marshal.Copy(input_mat.Ptr(0), data, 0, imageLen);
                    Array.Copy(data, 0, inputData, ino * imageLen, imageLen);
                }
                predictor.LoadInferenceData("images", inputData);

                DateTime end = DateTime.Now;
                Console.WriteLine("[ INFO ] Input image data processing time: " + (end - start).TotalMilliseconds + " ms.");
                predictor.infer();
                start = DateTime.Now;
                predictor.infer();
                end = DateTime.Now;
                Console.WriteLine("[ INFO ] Model inference time: " + (end - start).TotalMilliseconds + " ms.");
                start = DateTime.Now;


                float[] outputData = predictor.GetInferenceResult("output0");
                for (int i = 0; i < batchNum; ++i)
                {
                    Console.WriteLine(string.Format("\n[ INFO ] Classification Top {0} result : \n", 10));
                    Console.WriteLine("[ INFO ] classid probability");
                    Console.WriteLine("[ INFO ] ------- -----------");
                    float[] data = new float[2];
                    Array.Copy(outputData, i * 2, data, 0, 2);
                    List<int> sortResult = Argsort(new List<float>(data));

                    final_res = Tuple.Create(sortResult[0], data[sortResult[0]]);

                    for (int j = 0; j < 2; ++j)
                    {
                        string msg = "";
                        msg += ("index: " + sortResult[j] + "\t");
                        msg += ("score: " + data[sortResult[j]] + "\t");
                        Console.WriteLine("[ INFO ] " + msg);
                    }
                }
                end = DateTime.Now;
                Console.WriteLine("[ INFO ] Inference result processing time: " + (end - start).TotalMilliseconds + " ms.");
            }
            return final_res;

        }

        public static List<int> Argsort(List<float> array)
        {
            int arrayLen = array.Count;
            List<float[]> newArray = new List<float[]> { };
            for (int i = 0; i < arrayLen; i++)
            {
                newArray.Add(new float[] { array[i], i });
            }
            newArray.Sort((a, b) => b[0].CompareTo(a[0]));
            List<int> arrayIndex = new List<int>();
            foreach (float[] item in newArray)
            {
                arrayIndex.Add((int)item[1]);
            }
            return arrayIndex;
        }

        public static (string experimentId, string sampleId) ParseExperimentSampleId(string idString)
        {
            if (string.IsNullOrWhiteSpace(idString))
                throw new ArgumentException("输入字符串不能为空或空白", nameof(idString));

            var parts = idString.Split('-'); // 最多分割成2部分

            if (parts.Length != 2)
                throw new FormatException($"字符串'{idString}'格式不正确，应为'实验ID-样品ID'");

            string experimentId = parts[0].Trim();
            string sampleId = parts[1].Trim();

            if (string.IsNullOrEmpty(experimentId))
                throw new FormatException("实验ID不能为空");

            if (string.IsNullOrEmpty(sampleId))
                throw new FormatException("样品ID不能为空");

            return (experimentId, sampleId);
        }

        public static string GetNextImageFileName(string folderPath, string baseNameFormat = "{0}.jpg")
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"目录不存在: {folderPath}");
            }

            int nextNumber = 1;

            while (true)
            {
                string fileName = string.Format(baseNameFormat, nextNumber);
                string fullPath = Path.Combine(folderPath, fileName);

                if (!File.Exists(fullPath))
                {
                    return fileName;
                }

                nextNumber++;
            }
        }


        public static string AppendSuffixToFileName(string filePath, string suffix)
        {
            // 获取文件所在目录
            string directory = Path.GetDirectoryName(filePath);

            // 获取不带扩展名的文件名 + 后缀
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string newFileName = $"{fileNameWithoutExt}{suffix}";

            // 获取原扩展名（含点，如 ".jpg"）
            string extension = Path.GetExtension(filePath);

            // 组合新路径
            return Path.Combine(directory, newFileName + extension);
        }

    }
   }
