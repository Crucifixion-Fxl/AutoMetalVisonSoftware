using AutoMetal;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrtCommon;

namespace ImageAnalysis
{
    public static class AbnormalAnalyzer
    {
        public static Tuple<Dictionary<string, double>,string> abnormalAnalysis(String imgPath)
        {
            Mat image = Cv2.ImRead(imgPath);
            List<Mat> images = new List<Mat>() { image };
            Tuple<int, float> res = AnalysisUtils.Infer(AutoMetalConstants.model_path_cls, images);

            string outImgPath = AnalysisUtils.AppendSuffixToFileName(imgPath, "_abnormal");
            Tuple<List<SegResult>, Mat, Dictionary<int, double>> seg_res = AnalysisUtils.SegInfer(AutoMetalConstants.model_path_seg, images, outImgPath);


            Dictionary<int, double> intKeyDict = seg_res.Item3; // 从元组获取

            // 目标字典
            Dictionary<string, double> stringKeyDict = new Dictionary<string, double>();

            
            foreach (var kvp in intKeyDict)
            {
                int classId = kvp.Key;
                double score = kvp.Value;

                // 检查索引是否在 labels 范围内
                if (classId >= 0 && classId < MetalOption.labels.Count)
                {
                    string label = MetalOption.labels[classId];
                    stringKeyDict[label] = score;
                }
                else
                {
                    // 如果索引超出范围，使用默认名称（可选）
                    stringKeyDict[$"Unknown_{classId}"] = score;
                }
            }

            return Tuple.Create(stringKeyDict, outImgPath);

        }
    }
}
