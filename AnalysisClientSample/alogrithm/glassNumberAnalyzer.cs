using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMetal;
using TrtCommon;
using Yolov8;
namespace ImageAnalysis
{
    public class glassNumberAnalyzer
    {

        public static string glassSegEnginePath = AutoMetalConstants.glassSegEnginePath;
        public static string glassDectEnginePath = AutoMetalConstants.glassDectEnginePath;


        public static string GetGlassNumber(string imagePath)
        {
            return getAreaNumer(GetSegArea(imagePath));
        }


        public static Mat GetSegArea(string imagePath)
        {
            Yolov8Seg yolov8Seg = new Yolov8Seg(glassSegEnginePath);

            // 这行一定要修改类别数量
            yolov8Seg.CategNums = 1; 
            Mat image1 = Cv2.ImRead(imagePath);

            List<SegResult> segResults = yolov8Seg.Predict(new List<Mat> { image1 });
            // 获取第一个 SegResult
            SegResult firstResult = segResults[0];

            // Get the first SegData (assuming you want the first detection)
            SegData firstSegData = firstResult.datas[0];

            // Extract the mask (Mat object)
            Mat mask = firstSegData.mask;

            // 2. 确保 mask 是 CV_8U 单通道
            // 归一化到 0-255
            Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2GRAY);
            mask.ConvertTo(mask, MatType.CV_8UC1);

            // 3. 计算原始mask的最小外接矩形
            Rect boundingRect = Cv2.BoundingRect(mask);


            // 裁剪原图对应区域
            Mat croppedImage = new Mat(image1, boundingRect);

            // 旋转原图
            Mat rotatedImage = new Mat();
            Cv2.Rotate(croppedImage, rotatedImage, RotateFlags.Rotate90Counterclockwise);

            return rotatedImage;

        }

        public static string getAreaNumer(Mat mat)
        {
            Yolov8Det yolov8Det = new Yolov8Det(glassDectEnginePath);
            yolov8Det.CategNums = 10;
            List<DetResult> detResults = yolov8Det.Predict(new List<Mat> { mat });

            if (detResults == null || detResults.Count == 0 || detResults[0].datas == null || detResults[0].datas.Count == 0)
            {
                return string.Empty;
            }

            // 获取第一个结果集（假设每张图只有一个DetResult）
            DetResult detResult = detResults[0];

            // 按bbox的X坐标从左到右排序
            var sortedData = detResult.datas.OrderBy(d => d.box.X).ToList();

            // 拼接所有label
            StringBuilder sb = new StringBuilder();
            foreach (var data in sortedData)
            {
                sb.Append(glassDetectOption.labels[data.index]);
            }

            return sb.ToString();
        }
    }
}
