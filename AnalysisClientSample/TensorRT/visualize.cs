using AutoMetal;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrtCommon
{
    public struct CocoOption
    {
        public static List<string> labels = new List<string>{
            "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck",
            "boat", "traffic light","fire hydrant","stop sign", "parking meter", "bench", "bird", "cat", "dog",
            "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe","backpack", "umbrella","handbag",
            "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat",
            "baseball glove","skateboard", "surfboard","tennis racket", "bottle", "wine glass", "cup", "fork",
            "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange","broccoli", "carrot","hot dog",
            "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet",
            "tvmonitor", "laptop", "mouse","remote","keyboard", "cell phone", "microwave", "oven", "toaster",
            "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush"};
    }

    public struct glassSegOption
    {
        public static List<string> labels = new List<string>
        {
            "Number_Area"
        };
     }

    public struct glassDetectOption
    {
        public static List<string> labels = new List<string>
        {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"
        };
    }


    public struct MetalOption
    {
        // 后续如果有细粒度的分割划分，可以在这里修改
        public static List<string> labels = new List<string>
        {
            "abnormal"
        };
    }


    public struct Dotav1Option
    {
        public static List<string> labels = new List<string>{ "plane","ship", "storage tank", "baseball diamond", "tennis court",
            "basketball court", "ground track field", "harbor",  "bridge",  "large vehicle",  "small vehicle",
            "helicopter",  "roundabout", "soccer ball field", "swimming pool" };
    }


    public static class Visualize
    {

        /// <summary>
        /// Result drawing
        /// </summary>
        /// <param name="result">recognition result</param>
        /// <param name="image">image</param>
        /// <returns></returns>
        public static Mat DrawDetResult(DetResult result, Mat image,SegType segType)
        {
            // Draw recognition results on the image
            for (int i = 0; i < result.count; i++)
            {
                //Console.WriteLine(result.rects[i]);
                Cv2.Rectangle(image, result.datas[i].box, new Scalar(0, 0, 255), 2, LineTypes.Link8);
                Cv2.Rectangle(image, new Point(result.datas[i].box.TopLeft.X, result.datas[i].box.TopLeft.Y + 30),
                    new Point(result.datas[i].box.BottomRight.X, result.datas[i].box.TopLeft.Y), new Scalar(0, 255, 255), -1);
                if (segType == SegType.Glass)
                {
                    Cv2.PutText(image, glassDetectOption.labels[result.datas[i].index] + "-" + result.datas[i].score.ToString("0.00"),
                        new Point(result.datas[i].box.X, result.datas[i].box.Y + 25),
                        HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 0, 0), 2);

                }
            }
            return image;
        }

        public static Mat DrawSegResult(SegResult result, Mat image,SegType segType)
        {
            Mat maskedImg = new Mat(); // 一张图像里面可能有多种类别
            // Draw recognition results on the image
            for (int i = 0; i < result.count; i++)
            {
                Cv2.Rectangle(image, result.datas[i].box, new Scalar(0, 0, 255), 2, LineTypes.Link8);
                Cv2.Rectangle(image, new Point(result.datas[i].box.TopLeft.X, result.datas[i].box.TopLeft.Y + 30),
                    new Point(result.datas[i].box.BottomRight.X, result.datas[i].box.TopLeft.Y), new Scalar(0, 255, 255), -1);
               
                if(segType == SegType.Glass)
                {
                    Cv2.PutText(image, glassSegOption.labels[result.datas[i].index] + "-" + result.datas[i].score.ToString("0.00"),
                        new Point(result.datas[i].box.X, result.datas[i].box.Y + 25),
                        HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 0, 0), 2);
                }
                else
                {
                    Cv2.PutText(image, MetalOption.labels[result.datas[i].index] + "-" + result.datas[i].score.ToString("0.00"),
                        new Point(result.datas[i].box.X, result.datas[i].box.Y + 25),
                        HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 0, 0), 2);
                }
                    Cv2.AddWeighted(image, 0.5, result.datas[i].mask, 0.5, 0, maskedImg);
            }
            // 如果这里没有分割出任何图像就返回原图像
            if (result.count == 0)
            {
                return image;
            }
            else
            {
                return maskedImg;
            }

        }
    }
}