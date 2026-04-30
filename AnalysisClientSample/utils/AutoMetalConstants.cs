using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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


        // 分类模型算法权重地址（默认值）
        public const string default_model_path_cls = "C:\\Users\\SOW111\\Downloads\\TensorRT-8.6.1.6.Windows10.x86_64.cuda-12.0\\TensorRT-8.6.1.6\\bin\\best.engine";

        // 算法模型权重地址（默认值）
        public const string default_model_path_seg = "C:\\Users\\SOW111\\Downloads\\TensorRT-8.6.1.6.Windows10.x86_64.cuda-12.0\\TensorRT-8.6.1.6\\bin\\yolov8l-seg.engine";

        // DeepLab 训练项目路径（train.py 所在目录）
        public const string deepLabProjectPath = @"C:\Users\SOW111\Desktop\MicroProj\deeplabv3-plus-pytorch";

        // DeepLab 训练数据路径（VOCdevkit）
        public const string deepLabVocPath = @"C:\Users\SOW111\Desktop\MicroProj\deeplabv3-plus-pytorch\VOCdevkit";

        // 优先使用该解释器（如存在），否则回退到系统python
        public const string deepLabPythonExe = @"C:\Users\SOW111\Desktop\MicroProj\deeplabv3-plus-pytorch\.venv\Scripts\python.exe";

        // 手动模式拼接的图像保存文件夹地址（默认值）
        public const string default_manualFolderPath = @"D:\all_images\handScan";

        // 自动模式拼接的图像保存文件夹地址（默认值）
        public const string default_autoFolderPath = @"D:\all_images\AutoScan";

        // 运行时可修改参数（未加载用户配置时保持默认值）
        public static string model_path_cls = default_model_path_cls;
        public static string model_path_seg = default_model_path_seg;
        public static string manualFolderPath = default_manualFolderPath;
        public static string autoFolderPath = default_autoFolderPath;


        // 局域网进行通信的IP地址
        public const string localAddress = "127.0.0.1";

        // 显微镜的通信端口
        public const string microPort = "8087";

        // UDP：与机械臂通信的IP地址及端口：从显微镜到机械臂（发送显微镜状态信号给机械臂）
        public const int clientPort = 6699;

        // UDP： 与机械臂通信的IP地址及端口：从机械臂到显微镜 （接收机械臂的传递的消息）
        public const int serverPort = 6656;


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


        // 竖向: 正常模式自动扫描的起始坐标、结束坐标
        public const string normal_leftTop_x = "-11200.69";
        public const string normal_leftTop_y = "-22140.38";
        public const string normal_rightBottom_x = "12480.19";
        public const string normal_rightBottom_y = "21699.38";

        // 竖向: 测试模式（用于快速测试对接流程）的起始坐标、结束坐标
        public const string test_leftTop_x = "-11200.69";
        public const string test_leftTop_y = "-11400.38";
        public const string test_rightBottom_x = "-12000.69";
        public const string test_rightBottom_y = "-12000.38";

        // 竖向:设定图像处理后的宽度（默认值）
        public const int default_scale_width = 4608;
        public const int default_scale_height = 2348;

        // 竖向:设定边界裁剪的像素大小（默认值）
        public const int default_clipTop = 80;
        public const int default_clipBottom = 50;
        public const int default_clipLeft = 50;
        public const int default_clipRight = 50;

        // 运行时可修改参数（未加载用户配置时保持默认值）
        public static int scale_width = default_scale_width;
        public static int scale_height = default_scale_height;
        public static int clipTop = default_clipTop;
        public static int clipBottom = default_clipBottom;
        public static int clipLeft = default_clipLeft;
        public static int clipRight = default_clipRight;

        // 竖向:裁剪后图像的ROI（运行时动态计算）
        public static int newWidth => scale_width / 2 - clipLeft - clipRight - 1;
        public static int newHeight => scale_height - clipTop - clipBottom - 1;


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

        // 初始对比度
        public const int initContrast = 0;

        // 初始XY与Z显示值（用于WinForm输入框默认展示）
        public const int initX = 0;
        public const int initY = 0;
        public const int initZ = 0;

        // 数据库地址
        public const string dbPath = "D:\\Parameter\\Meta\\DB\\cv.db";


        // 视频流配置参数
        public const int DEFAULT_TARGET_FPS = 10; // 默认目标帧率
        public const int DEFAULT_SNAP_TIMEOUT = 500; // 默认snap超时时间（毫秒）
        public const bool DEFAULT_ENABLE_FPS_CONTROL = true; // 默认是否启用帧率控制


        // 玻璃编号检测trt engine地址
        public const string glassDectEnginePath = @"D:\Parameter\Meta\TensorRT\weights\number_reco.engine";


        // 配置deeplabv3+模型的trt路径
        public const string deeplabv3PlusEnginePath = @"D:\Parameter\Meta\TensorRT\weights\coverage_cal.engine";

        private static readonly string SettingsFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user-parameter-settings.json");

        public static void LoadUserSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return;
                }

                var json = File.ReadAllText(SettingsFilePath, Encoding.UTF8);
                var settings = JsonConvert.DeserializeObject<ParameterSettings>(json);
                if (settings == null)
                {
                    return;
                }

                ApplyParameterSettings(settings);
            }
            catch
            {
                // 读取失败时保持默认值，避免影响主流程
            }
        }

        public static void SaveUserSettings(ParameterSettings settings)
        {
            ApplyParameterSettings(settings);
            var json = JsonConvert.SerializeObject(GetCurrentParameterSettings(), Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json, Encoding.UTF8);
        }

        public static ParameterSettings GetCurrentParameterSettings()
        {
            return new ParameterSettings
            {
                ModelPathCls = model_path_cls,
                ModelPathSeg = model_path_seg,
                ManualFolderPath = manualFolderPath,
                AutoFolderPath = autoFolderPath,
                ScaleWidth = scale_width,
                ScaleHeight = scale_height,
                ClipTop = clipTop,
                ClipBottom = clipBottom,
                ClipLeft = clipLeft,
                ClipRight = clipRight
            };
        }

        private static void ApplyParameterSettings(ParameterSettings settings)
        {
            model_path_cls = string.IsNullOrWhiteSpace(settings.ModelPathCls) ? default_model_path_cls : settings.ModelPathCls;
            model_path_seg = string.IsNullOrWhiteSpace(settings.ModelPathSeg) ? default_model_path_seg : settings.ModelPathSeg;
            manualFolderPath = string.IsNullOrWhiteSpace(settings.ManualFolderPath) ? default_manualFolderPath : settings.ManualFolderPath;
            autoFolderPath = string.IsNullOrWhiteSpace(settings.AutoFolderPath) ? default_autoFolderPath : settings.AutoFolderPath;

            scale_width = settings.ScaleWidth > 0 ? settings.ScaleWidth : default_scale_width;
            scale_height = settings.ScaleHeight > 0 ? settings.ScaleHeight : default_scale_height;
            clipTop = settings.ClipTop >= 0 ? settings.ClipTop : default_clipTop;
            clipBottom = settings.ClipBottom >= 0 ? settings.ClipBottom : default_clipBottom;
            clipLeft = settings.ClipLeft >= 0 ? settings.ClipLeft : default_clipLeft;
            clipRight = settings.ClipRight >= 0 ? settings.ClipRight : default_clipRight;
        }

        public class ParameterSettings
        {
            public string ModelPathCls { get; set; }
            public string ModelPathSeg { get; set; }
            public string ManualFolderPath { get; set; }
            public string AutoFolderPath { get; set; }
            public int ScaleWidth { get; set; }
            public int ScaleHeight { get; set; }
            public int ClipTop { get; set; }
            public int ClipBottom { get; set; }
            public int ClipLeft { get; set; }
            public int ClipRight { get; set; }
        }


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
