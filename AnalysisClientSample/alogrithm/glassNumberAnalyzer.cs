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
        public static string glassDectEnginePath = AutoMetalConstants.glassDectEnginePath;


        public static string GetGlassNumber(string imagePath)
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

            // 按bbox的Y坐标从上到下排序
            var sortedData = detResult.datas.OrderBy(d => d.box.Y).ToList();

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
