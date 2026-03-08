using OpenCvSharp.Dnn;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TensorRtSharp.Custom;
using TensorRtSharp;

namespace ImageAnalysis
{
    public class Classifier{

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

    }
}
