using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AutoMetal
{
    public static class AutoMetalConstants
    {
        // 显微镜坐标: 最外侧位置接收样品
        //public const int reset_x = -845;
        public const int reset_x = -50868;
        public const int reset_y = -51029;


        // 分类模型算法权重地址
        public const string model_path_cls = "C:\\Users\\SOW111\\Downloads\\TensorRT-8.6.1.6.Windows10.x86_64.cuda-12.0\\TensorRT-8.6.1.6\\bin\\best.engine";

        // 算法模型权重地址
        public const string model_path_seg = "C:\\Users\\SOW111\\Downloads\\TensorRT-8.6.1.6.Windows10.x86_64.cuda-12.0\\TensorRT-8.6.1.6\\bin\\yolov8l-seg.engine";

        // 手动模式拼接的图像保存文件夹地址
        public const string manualFolderPath = @"D:\all_images\handScan";

        // 自动模式拼接的图像保存文件夹地址
        public const string autoFolderPath = @"D:\all_images\AutoScan";


        // 局域网进行通信的IP地址
        public const string localAddress = "127.0.0.1";

        // 显微镜的通信端口
        public const string microPort = "8087";

        // UDP：与机械臂通信的IP地址及端口：从显微镜到机械臂（发送显微镜状态信号给机械臂）
        public const int clientPort = 8888;

        // UDP： 与机械臂通信的IP地址及端口：从机械臂到显微镜 （接收机械臂的传递的消息）
        public const int serverPort = 6000;


        // 横向与纵向二选一

        // 横向:自动扫描的起始坐标、结束坐标
        //public const string leftTop_x = "-20415.31";
        //public const string leftTop_y = "-11250.94";
        //public const string rightBottom_x = "22308.13";
        //public const string rightBottom_y = "10110.62";

        // 横向:设定图像处理后的宽度
        //public const int scale_width = 4608;
        //public const int scale_height = 2348;

        // 横向:设定边界裁剪的像素大小
        //public const int clipTop = 80;
        //public const int clipBottom = 50;
        //public const int clipLeft = 50;
        //public const int clipRight = 50;

        // 横向:裁剪后图像的ROI
        //public const int newWidth = scale_width / 2 - clipLeft - clipRight - 1;
        //public const int newHeight = scale_height - clipTop - clipBottom - 1;


        // 竖向: 自动扫描的起始坐标、结束坐标
        public const string leftTop_x = "-11200.69";
        public const string leftTop_y = "-22140.38";
        public const string rightBottom_x = "12480.19";
        public const string rightBottom_y = "21699.38";

        // 竖向:设定图像处理后的宽度
        public const int scale_width = 4608;
        public const int scale_height = 2348;

        // 竖向:设定边界裁剪的像素大小
        public const int clipTop = 80;
        public const int clipBottom = 50;
        public const int clipLeft = 50;
        public const int clipRight = 50;

        // 竖向:裁剪后图像的ROI
        public const int newWidth = scale_width / 2 - clipLeft - clipRight - 1;
        public const int newHeight = scale_height - clipTop - clipBottom - 1;


        // 处理的最大像素数
        public const string OPENCV_IO_MAX_IMAGE_PIXELS = "10000000000000000";


        // 窗口名称
        public const string windowName = "AutoMetal";

        // 窗口大小
        public const int windowWidth = 1300;
        public const int windowHeight = 680;


        // 聚焦位点
        public const double focusValue = -4322.38;

        // 初始光亮度
        public const int initBrightness = 16;

        // 数据库地址
        public const string dbPath = "data\\sampleData.db";


        // 视频流配置参数
        public const int DEFAULT_TARGET_FPS = 10; // 默认目标帧率
        public const int DEFAULT_SNAP_TIMEOUT = 500; // 默认snap超时时间（毫秒）
        public const bool DEFAULT_ENABLE_FPS_CONTROL = true; // 默认是否启用帧率控制


        // 玻璃编号检测trt engine地址
        public const string glassSegEnginePath = @"D:\Parameter\Meta\TensorRT\best_detect_1013.engine";

        public const string glassDectEnginePath = @"D:\Parameter\Meta\TensorRT\best_seg_1013.enginee";
    
    
        // SAM模型的ONNX路径
        public const string samEncoderOnnxPath = @"C:\Users\SOW111\Desktop\MicroProj\Metalization\bin\x64\Release\weights\encoder.onnx";
        public const string samDecoderOnnxPath = @"C:\Users\SOW111\Desktop\MicroProj\Metalization\bin\x64\Release\weights\decoder.onnx";

        // SAM模型的TRT路径textul、visual 路径
        public const string textPromptOnnxPath = @"C:\Users\SOW111\Desktop\MicroProj\Metalization\bin\x64\Release\weights\textual.onnx";
        public const string visualPromptOnnxPath = @"C:\Users\SOW111\Desktop\MicroProj\Metalization\bin\x64\Release\weights\visual.onnx";


        // 配置deeplabv3+模型的trt路径
        public const string deeplabv3PlusEnginePath = @"D:\Parameter\Meta\TensorRT\deeplabv3+.engine";


    }

    public enum IdleStatus
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idle,

        /// <summary>
        /// 忙碌状态
        /// </summary>
        Busy
    }

    public enum SegType
    {
        Glass,
        Coat
    }

}
