using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Motic.Analysis.Net;
using System.IO;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Net.Sockets;
using System.Net;
using ImageAnalysis;
using System.Threading.Tasks;
using System.Linq;
using AutoMetalDataBase;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;

namespace AutoMetal
{
    public partial class AutoMetal : Form
    {
        // UDP 通信服务 - 使用同一个端口接收和发送
        private UdpClient udpClient;

        // UDP 开启状态
        private bool isRunning = false;

        // UDP 监听线程
        private Thread listenThread;

        // 目标通信端点
        private IPEndPoint _targetEndPoint;

        // 设置状态：判断显微镜当前是否处于空闲状态:1-空闲 0-忙碌
        private IdleStatus is_free = IdleStatus.Idle;

        // Moetic 通信客户端
        private static AnalysisClient m_analysis;

        // 图像分辨率
        private int m_width = 0;
        private int m_height = 0;

        // 回调打印函数
        private delegate void LogTextCallback(string text);

        // 设置观察是否处于扫描状态
        private bool is_autoScanState = false;

        // 尝试创建视频流
        private volatile bool is_steaming = false;
        private CancellationTokenSource _cancellationTokenSource;

        // 键盘控制开关
        private bool isKeyboardControlEnabled = false;
        private float moveStep = 1.0f; // 移动步长

        // CheckBox控件引用（假设名为checkBoxKeyboardControl）
        private CheckBox checkBoxKeyboardControl;

        // 可重用的图像缓冲区，避免重复分配内存
        private byte[] reusableImageBuffer = null;

        // 使用 BindingList 而不是 List，以便支持 DataGridView 的自动刷新
        private BindingList<SampleDBHelper.SampleData> _bindingList;

        // 软件运行时长相关
        private DateTime startTime;
        private System.Windows.Forms.Timer runtimeTimer;

        public AutoMetal()
        {
            // 初始化组件及窗口固定
            InitializeComponent();

            // 初始化默认状态
            initDefaultConfig();

            // 初始化客户端
            m_analysis = new AnalysisClient();

            // 初始化运行时长显示
            InitializeRuntimeDisplay();

        }

        private void initDefaultConfig() {

            // 窗口名
            this.Text = AutoMetalConstants.windowName;

            // 固定单线边框
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // 开启最小化
            this.MinimizeBox = true;

            // 关闭最大化
            this.MaximizeBox = false;

            // 开启控制栏
            this.ControlBox = true;

            // 窗口大小调整
            this.Size = new System.Drawing.Size(AutoMetalConstants.windowWidth, AutoMetalConstants.windowHeight);

            // 固定最大、小化窗口
            this.MaximumSize = this.MinimumSize = this.Size;

            // 设置server通信端口
            this.udpPort.Text = AutoMetalConstants.serverPort.ToString();

            // 设置显微镜通信地址：固定
            textBoxIP.Text = AutoMetalConstants.localAddress;

            // 设置显微镜的通信端口: 固定
            textBoxPort.Text = AutoMetalConstants.microPort;

            // 订阅窗口结束事件
            this.FormClosed += OnFormClosed;

            // 初始化样品数据库
            SampleDBHelper.Initialize(AutoMetalConstants.dbPath);

            // 初始化键盘控制CheckBox监听
            InitializeKeyboardControlCheckBox();

        }

 
        private void InitializeRuntimeDisplay()
        {
            // 记录启动时间
            startTime = DateTime.Now;

            // 查找用于显示运行时长的Label控件
            // 如果您的Label名称不同，请修改这里的名称

            if (this.runtimeLabel != null)
            {
                SafeAppendLog("运行时长显示组件已找到");

                // 创建定时器，每秒更新一次
                runtimeTimer = new System.Windows.Forms.Timer();
                runtimeTimer.Interval = 1000; // 1秒
                runtimeTimer.Tick += RuntimeTimer_Tick;
                runtimeTimer.Start();

                SafeAppendLog("运行时长显示已启动");
            }
            else
            {
                SafeAppendLog("未找到运行时长显示组件，请手动添加名为 'runtimeLabel' 的Label控件");
            }
        }

        // 定时器事件处理
        private void RuntimeTimer_Tick(object sender, EventArgs e)
        {
            UpdateRuntimeDisplay();
        }

        // 更新运行时长显示
        private void UpdateRuntimeDisplay()
        {
            if (runtimeLabel != null)
            {
                TimeSpan runtime = DateTime.Now - startTime;
                string runtimeText = $"运行时长: {runtime.Hours:D2}:{runtime.Minutes:D2}:{runtime.Seconds:D2}";

                if (runtimeLabel.InvokeRequired)
                {
                    runtimeLabel.Invoke(new Action(() => runtimeLabel.Text = runtimeText));
                }
                else
                {
                    runtimeLabel.Text = runtimeText;
                }
            }
        }

        // 获取当前运行时长
        public TimeSpan GetRuntime()
        {
            return DateTime.Now - startTime;
        }

        // 获取格式化的运行时长字符串
        public string GetRuntimeString()
        {
            TimeSpan runtime = DateTime.Now - startTime;
            return $"{runtime.Hours:D2}:{runtime.Minutes:D2}:{runtime.Seconds:D2}";
        }

        // 停止运行时长显示
        public void StopRuntimeDisplay()
        {
            if (runtimeTimer != null)
            {
                runtimeTimer.Stop();
                runtimeTimer.Dispose();
                runtimeTimer = null;
            }
        }

        // 手动设置运行时长显示组件
        public void SetRuntimeDisplayLabel(Label label)
        {
            // 停止之前的定时器
            StopRuntimeDisplay();

            runtimeLabel = label;

            if (runtimeLabel != null)
            {
                // 创建定时器，每秒更新一次
                runtimeTimer = new System.Windows.Forms.Timer();
                runtimeTimer.Interval = 1000; // 1秒
                runtimeTimer.Tick += RuntimeTimer_Tick;
                runtimeTimer.Start();

                SafeAppendLog("运行时长显示组件已手动设置并启动");
            }
        }

        // 初始化键盘控制CheckBox
        private void InitializeKeyboardControlCheckBox()
        {
            // 查找名为checkBoxKeyboardControl的CheckBox控件
            // 如果您的CheckBox名称不同，请修改这里的名称
            checkBoxKeyboardControl = this.Controls.Find("checkBoxKeyboardControl", true).FirstOrDefault() as CheckBox;

            if (checkBoxKeyboardControl != null)
            {
                // 订阅CheckBox状态变化事件
                checkBoxKeyboardControl.CheckedChanged += OnKeyboardControlCheckBoxChanged;
                SafeAppendLog("键盘控制CheckBox已初始化");
            }
            else
            {
                SafeAppendLog("警告: 未找到键盘控制CheckBox控件");
            }
        }

        // CheckBox状态变化事件处理
        private void OnKeyboardControlCheckBoxChanged(object sender, EventArgs e)
        {
            if (checkBoxKeyboardControl != null)
            {
                EnableKeyboardControl(checkBoxKeyboardControl.Checked);
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine("程序结束，显微镜复位");

            // 停止运行时长显示
            StopRuntimeDisplay();

            // 记录最终运行时长
            TimeSpan finalRuntime = DateTime.Now - startTime;
            SafeAppendLog($"程序总运行时长: {finalRuntime.Hours:D2}:{finalRuntime.Minutes:D2}:{finalRuntime.Seconds:D2}");

            int id;
            m_analysis.setXY(out id, 0, 0);

        }

        // 键盘控制相关方法
        private void EnableKeyboardControl(bool enable)
        {
            isKeyboardControlEnabled = enable;

            // 同步更新CheckBox状态（避免循环触发事件）
            if (checkBoxKeyboardControl != null && checkBoxKeyboardControl.Checked != enable)
            {
                checkBoxKeyboardControl.CheckedChanged -= OnKeyboardControlCheckBoxChanged; // 临时取消事件监听
                checkBoxKeyboardControl.Checked = enable;
                checkBoxKeyboardControl.CheckedChanged += OnKeyboardControlCheckBoxChanged; // 重新添加事件监听
            }

            if (enable)
            {
                this.KeyPreview = true; // 启用键盘预览
                this.KeyDown += OnKeyDown; // 订阅键盘按下事件
                SafeAppendLog("键盘控制已启用 - 使用方向键控制移动，R键复位到原点");
            }
            else
            {
                this.KeyPreview = false; // 禁用键盘预览
                this.KeyDown -= OnKeyDown; // 取消订阅键盘按下事件
                SafeAppendLog("键盘控制已禁用");
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!isKeyboardControlEnabled || m_analysis == null)
                return;

            int id;
            float currentX, currentY;

            // 获取当前位置
            m_analysis.getXY(out currentX, out currentY);

            switch (e.KeyCode)
            {
                case Keys.Up:
                    // 向上移动
                    m_analysis.setXY(out id, currentX, currentY + moveStep);
                    SafeAppendLog($"向上移动: ({currentX:F2}, {currentY + moveStep:F2})");
                    // 移动后调用snap_Click函数
                    snap_Click(sender, e);
                    break;

                case Keys.Down:
                    // 向下移动
                    m_analysis.setXY(out id, currentX, currentY - moveStep);
                    SafeAppendLog($"向下移动: ({currentX:F2}, {currentY - moveStep:F2})");
                    // 移动后调用snap_Click函数
                    snap_Click(sender, e);
                    break;

                case Keys.Left:
                    // 向左移动
                    m_analysis.setXY(out id, currentX - moveStep, currentY);
                    SafeAppendLog($"向左移动: ({currentX - moveStep:F2}, {currentY:F2})");
                    // 移动后调用snap_Click函数
                    snap_Click(sender, e);
                    break;

                case Keys.Right:
                    // 向右移动
                    m_analysis.setXY(out id, currentX + moveStep, currentY);
                    SafeAppendLog($"向右移动: ({currentX + moveStep:F2}, {currentY:F2})");
                    // 移动后调用snap_Click函数
                    snap_Click(sender, e);
                    break;

                case Keys.R:
                    // 复位到原点
                    m_analysis.setXY(out id, 0, 0);
                    SafeAppendLog("复位到原点: (0, 0)");
                    // 复位后调用snap_Click函数
                    snap_Click(sender, e);
                    break;
            }

            e.Handled = true; // 标记事件已处理
        }

        // 设置移动步长
        public void SetMoveStep(float step)
        {
            if (step > 0)
            {
                moveStep = step;
                SafeAppendLog($"移动步长设置为: {step:F2}");
            }
            else
            {
                SafeAppendLog("移动步长必须大于0");
            }
        }

        // 获取当前键盘控制状态
        public bool IsKeyboardControlEnabled()
        {
            return isKeyboardControlEnabled;
        }

        private bool Connect(string strIP, int port)
        {
            if (!m_analysis.connect(strIP.ToCharArray(), port))
            {
                MessageBox.Show("连接失败");
                return false;
            }

            m_analysis.setCallBack(EventCallback);

            // 默认初始化一些参数
            // 1.图像分辨率
            int x, y;
            int[] w = new int[4];
            int[] h = new int[4];

            int c = 4;

            Moac_retCode ret;

            ret = m_analysis.getResolution(out x, out y, w, h, ref c);

            if (ret == Moac_retCode.RC_FINISH)
            {
                m_width = x;
                m_height = y;
            }
            else
            {
                LogText("GetResolution Error = " + ret);
            }

            // 设置自动模式，可通过SDK控制
            m_analysis.setAutoControl(true);

            // 设置初始亮度值
            m_analysis.setBrightness(AutoMetalConstants.initBrightness);

            // 设置拼接扫描模式-0-每个视场不自动对焦
            ret = m_analysis.scanMode(0, 0, 0, 0);

            // 初始位置位于最外侧
            int id;
            m_analysis.setXY(out id, AutoMetalConstants.reset_x, AutoMetalConstants.reset_y);

            // 初始状态设置为空闲
            is_free = IdleStatus.Idle;

            return true;
        }

        private void LogText(string str)
        {
            if (this.listBoxInfo.InvokeRequired)
            {
                LogTextCallback d = new LogTextCallback(LogText);
                this.Invoke(d, new object[] { str });
            }
            else
            {
                listBoxInfo.Items.Add(str);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string strIP = textBoxIP.Text;

            int port;
            if (strIP.Length < 7 || !int.TryParse(textBoxPort.Text, out port))
            {
                MessageBox.Show("无效的IP地址或端口号");
                return;
            }
            if (!Connect(strIP, port))
            {
                MessageBox.Show("连接失败");
                return;
            }
            listBoxInfo.Items.Add("Connected: " + strIP + ":" + port);
        }

        private void EventCallback(int id, int status)
        {
            if (id == 0)
            {
                if (status == 0)
                {
                    LogText("disconnected!");
                }
                else if (status == 1)
                {
                    LogText(id + "Connected!");
                }
            } 
            else if ((id > 1 && is_autoScanState) && status == 1)
            {
                is_autoScanState = false;

                ImageProcessor.ProcessResult processedImage = ImageProcessor.ProcessImage(samplePath.Text);

                string croppedImagePath = samplePath.Text.Replace(".jpg", "_cropped.jpg");

                Cv2.ImWrite(croppedImagePath, processedImage.CroppedImage);

                Bitmap bitmap = BitmapConverter.ToBitmap(processedImage.CroppedImage);

                Ori_picture.Image = bitmap;

                //// 识别出来的玻璃编号ID
                var sampleID_reco = glassNumberAnalyzer.GetGlassNumber(samplePath.Text);

                // 均匀性计算
                Tuple<double, string> nUniformity = ImageUniformityCalculator.CalculateUniformity(croppedImagePath);


                double nUniformityValue = nUniformity.Item1;


                SafeUpdateTextBox(textBox6, nUniformity.Item2);


                // 覆盖率计算
                double nConverageRate = CoverageAnalyzer.detectImage(croppedImagePath);

                                             
                SafeUpdateTextBox(textBox5, nConverageRate.ToString());


                var sample = new SampleDBHelper.SampleData
                {
                   SampleId = sampleID_reco,
                   Coverage = nConverageRate,
                   OriginalImagePath = samplePath.Text,
                   CroppedImagePath = croppedImagePath,
                   Uniformity = nUniformityValue,
                   UniformityAnalysisImagePath = nUniformity.Item2,
                   CoverageAnalysisImagePath = null
                };

                SampleDBHelper.UpsertSample(sample);

                int id_unuse;
                m_analysis.setXY(out id_unuse, AutoMetalConstants.reset_x, AutoMetalConstants.reset_y);

                Thread.Sleep(5000);

                is_free = IdleStatus.Idle;

                SendMessageAysnc("complete");

            }
        }

        private void btnSetXY_Click(object sender, EventArgs e)
        {
            int id;
            float x, y;

            if (float.TryParse(textXY1.Text, out x) && float.TryParse(textXY2.Text, out y))
            {
                Moac_retCode ret = m_analysis.setXY(out id, x, y);
                listBoxInfo.Items.Add("setXY=" + ret + ",id=" + id);
            }
            else
            {
                MessageBox.Show("无效的坐标值！");
            }

        }

        private void btnSetZ_Click(object sender, EventArgs e)
        {
            int id;
            float z;
            if (float.TryParse(textZ.Text, out z))
            {
                Moac_retCode ret = m_analysis.setZ(out id, z);
                listBoxInfo.Items.Add("setZ=" + ret + ",id=" + id);
            }
            else
            {
                MessageBox.Show("无效的坐标值！");
            }
        }

        private void btnSetBrightness_Click(object sender, EventArgs e)
        {
            int val;
            if (int.TryParse(textBrightness.Text, out val))
            {
                Moac_retCode ret = m_analysis.setBrightness(val);
                listBoxInfo.Items.Add("setBrightess=" + ret + ",value=" + val);
            }
            else
            {
                MessageBox.Show("无效的亮度值！");
            }
        }

        private void btnSetContrast_Click(object sender, EventArgs e)
        {
            int val;
            if (int.TryParse(textContrast.Text, out val))
            {
                Moac_retCode ret = m_analysis.setContrast(val);
                listBoxInfo.Items.Add("setContrast=" + ret + ",value=" + val);
            }
            else
            {
                MessageBox.Show("无效的对比度值!");
            }

        }

        private void btnAutoFocus_Click(object sender, EventArgs e)
        {
            if (m_analysis != null)
            {
                int val = 0;
                int.TryParse(textFocus.Text, out val);
                int id;
                Moac_retCode ret = m_analysis.autoFocus(out id, val);
                listBoxInfo.Items.Add("autoFocus=" + ret + ",id=" + id);
            }
        }

        private void btnAutoScan_Click(object sender, EventArgs e)
        {
            PerformAutoScan();
        }

        private void PerformAutoScan()
        {

            float x0, x1, y0, y1;

            // 设置自动扫描状态
            is_autoScanState = true;

            if (float.TryParse(textX0.Text, out x0) &&
                float.TryParse(textY0.Text, out y0) &&
                float.TryParse(textX1.Text, out x1) &&
                float.TryParse(textY1.Text, out y1)
                )
            {

                string name = AnalysisUtils.GetNextImageFileName(AutoMetalConstants.manualFolderPath);
                int id;

                Moac_retCode ret = m_analysis.autoScan(out id, x0, y0, x1, y1, name);

                SafeAppendLog("autoScan=" + ret + ",id=" + id);

            }
            else
            {
                MessageBox.Show("无效的坐标值！");
            }

        }

        private void Hand_PerformAutoScan(string expID, string sampleID)
        {
            // 这里使用expID作为每一种实验的ID
            // 这里使用sampleID作为样品的名称（范围应该是1-5）
            // 后续应该在这里加上日期

            string dateString = DateTime.Now.ToString("yyyyMMdd");
            string folderPath = Path.Combine(AutoMetalConstants.autoFolderPath + "\\" + $"{dateString}", $"{expID}");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            m_analysis.setFilePath(folderPath);


            SafeUpdateTextBox(samplePath, folderPath + "\\" + sampleID + ".jpg");

            float x0, x1, y0, y1;

            // 设置自动扫描状态
            is_autoScanState = true;

            if (float.TryParse(textX0.Text, out x0) &&
                float.TryParse(textY0.Text, out y0) &&
                float.TryParse(textX1.Text, out x1) &&
                float.TryParse(textY1.Text, out y1)
                )
            {

                string name = sampleID + ".jpg";
                int id;

                Moac_retCode ret = m_analysis.autoScan(out id, x0, y0, x1, y1, name);


                SafeAppendLog("autoScan=" + ret + ",id=" + id);

            }
            else
            {
                MessageBox.Show("无效的坐标值！");
            }
        }

        private void snap_Click(object sender, EventArgs e)
        {
            // 计算所需的图像缓冲区大小
            int requiredSize = m_width * m_height * 3;

            // 如果缓冲区不存在或大小不匹配，则重新创建
            if (reusableImageBuffer == null || reusableImageBuffer.Length != requiredSize)
            {
                reusableImageBuffer = new byte[requiredSize];
            }

            // 使用可重用的缓冲区
            int size = requiredSize;
            if (m_analysis.snap(reusableImageBuffer, ref size, true) == Moac_retCode.RC_FINISH)
            {
                using (MemoryStream ms = new MemoryStream(reusableImageBuffer))
                {
                    // 释放旧图像资源
                    if (Ori_picture.Image != null)
                    {
                        Ori_picture.Image.Dispose();
                    }
                    Ori_picture.Image = Image.FromStream(ms);
                }
            }
        }


        // 执行具体算法逻辑的函数
        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void SafeUpdateRichTextBox(System.Windows.Forms.RichTextBox textBox, string value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action<System.Windows.Forms.RichTextBox, string>(SafeUpdateRichTextBox), textBox, value);
            }
            else
            {
                textBox.Text = value;
            }
        }


        private void SafeUpdateTextBox(System.Windows.Forms.TextBox textBox, string value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action<System.Windows.Forms.TextBox, string>(SafeUpdateTextBox), textBox, value);
            }
            else
            {
                textBox.Text = value;
            }
        }


        // 引用监听函数
        private void ListenForMessages()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    Console.WriteLine($"收到来自 {remoteEP} 的消息: {message}");

                    if (message.Equals("query", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("query");
                        // 使用发送方的端点回复
                        SendMessageToEndpoint(is_free.ToString(), remoteEP);
                    }
                    else if (message.Equals("get", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("get");
                        // 使用发送方的端点回复
                        SendMessageToEndpoint(is_free.ToString(), remoteEP);
                    }
                    else if (message.StartsWith("place:", StringComparison.OrdinalIgnoreCase))
                    {
                        // 放置状态完成后 显微镜处于忙碌状态
                        is_free = 0;

                        message = message.Substring(6);

                        // 安全更新UI（跨线程调用）
                        SafeAppendLog($"来自 {remoteEP}: {message}");

                        // 实验ID 样品序号
                        var (expId, sampleId) = AnalysisUtils.ParseExperimentSampleId(message);
                        SafeAppendLog($"开始进行检测: 实验ID: {expId}, 样品ID: {sampleId}");

                        // 更新表面组件
                        SafeUpdateTextBox(expID, expId);
                        SafeUpdateTextBox(sampleID, sampleId);

                        // 触发: 自动扫描
                        SafeUpdateTextBox(textX0, AutoMetalConstants.leftTop_x);
                        SafeUpdateTextBox(textY0, AutoMetalConstants.leftTop_y);
                        SafeUpdateTextBox(textX1, AutoMetalConstants.rightBottom_x);
                        SafeUpdateTextBox(textY1, AutoMetalConstants.rightBottom_y);

                        Hand_PerformAutoScan(expId, sampleId);
                    }
                }
                catch (SocketException ex)
                {
                    if (isRunning)
                        SafeAppendLog($"接收错误: {ex.Message}");
                }
                catch (ObjectDisposedException)
                {
                    // 当udpClient被关闭时退出循环
                    break;
                }
            }
        }

        // 线程安全的日志追加方法
        private void SafeAppendLog(string message)
        {
            if (listBoxInfo.InvokeRequired)
            {
                // 通过委托跨线程调用
                listBoxInfo.Invoke(new Action<string>(SafeAppendLog), message);
            }
            else
            {
                // 直接操作UI控件
                listBoxInfo.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
                listBoxInfo.TopIndex = listBoxInfo.Items.Count - 1; // 自动滚动到最后一行
            }
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            float cx, cy;
            m_analysis.getXY(out cx, out cy);
            Console.WriteLine(cx);
            Console.WriteLine(cy);

            if (isRunning)
            {
                listBoxInfo.Items.Add("UDP通信服务器已经正在运行");
                return;
            }
            if (!int.TryParse(udpPort.Text, out int port))
            {
                listBoxInfo.Items.Add("请输入有效的端口号");
                return;
            }

            try
            {
                // 使用同一个端口进行接收和发送
                udpClient = new UdpClient(port);
                listenThread = new Thread(new ThreadStart(ListenForMessages));
                listenThread.IsBackground = true;
                listenThread.Start();
                isRunning = true;
                listBoxInfo.Items.Add($"UDP 服务器已启动，监听端口: {port}");

                // 设置默认目标端点（用于主动发送消息）
                _targetEndPoint = new IPEndPoint(IPAddress.Parse(AutoMetalConstants.localAddress), AutoMetalConstants.clientPort);
            }
            catch (Exception ex)
            {
                listBoxInfo.Items.Add($"启动服务器失败: {ex.Message}");
            }
        }

        private void btnCloseServer_Click(object sender, EventArgs e)
        {
            isRunning = false;
            udpClient?.Close();
            listenThread?.Join(); // 等待线程结束
            listBoxInfo.Items.Add("UDP通信端口已经关闭");
        }

        private async Task SendMessageAysnc(string message)
        {
            try
            {
                Console.WriteLine("要发送的信号是" + message);
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                int len_sendBytes = await udpClient.SendAsync(sendBytes, sendBytes.Length, _targetEndPoint);
                Console.WriteLine($"已发送{len_sendBytes}字节到{_targetEndPoint}");
            }
            catch (Exception ex) {
                Console.WriteLine($"发送消息失败:{ex.Message}");
            }
        }

        // 发送消息到指定端点
        private async Task SendMessageToEndpoint(string message, IPEndPoint targetEndpoint)
        {
            try
            {
                Console.WriteLine($"要发送的信号是 {message} 到 {targetEndpoint}");
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                int len_sendBytes = await udpClient.SendAsync(sendBytes, sendBytes.Length, targetEndpoint);
                Console.WriteLine($"已发送{len_sendBytes}字节到{targetEndpoint}");
            }
            catch (Exception ex) {
                Console.WriteLine($"发送消息失败:{ex.Message}");
            }
        }

        // 设置视频流帧率（常量模式，仅显示当前值）
        public void SetVideoFPS(int fps)
        {
            SafeAppendLog($"当前帧率: {AutoMetalConstants.DEFAULT_TARGET_FPS} FPS (常量模式，无法修改)");
        }

        // 设置snap超时时间（常量模式，仅显示当前值）
        public void SetSnapTimeout(int timeoutMs)
        {
            SafeAppendLog($"当前超时时间: {AutoMetalConstants.DEFAULT_SNAP_TIMEOUT}ms (常量模式，无法修改)");
        }

        // 启用/禁用帧率控制（常量模式，仅显示当前值）
        public void EnableFPSControl(bool enable)
        {
            SafeAppendLog($"当前帧率控制: {(AutoMetalConstants.DEFAULT_ENABLE_FPS_CONTROL ? "启用" : "禁用")} (常量模式，无法修改)");
        }

        // 获取当前视频流状态
        public string GetVideoStreamStatus()
        {
            return $"帧率: {AutoMetalConstants.DEFAULT_TARGET_FPS} FPS, 超时: {AutoMetalConstants.DEFAULT_SNAP_TIMEOUT}ms, 帧率控制: {(AutoMetalConstants.DEFAULT_ENABLE_FPS_CONTROL ? "启用" : "禁用")}";
        }

        // 切换键盘控制状态
        public void ToggleKeyboardControl()
        {
            if (checkBoxKeyboardControl != null)
            {
                // 通过CheckBox来切换状态
                checkBoxKeyboardControl.Checked = !checkBoxKeyboardControl.Checked;
            }
            else
            {
                // 如果没有CheckBox，直接切换状态
                EnableKeyboardControl(!isKeyboardControlEnabled);
            }
        }

        // 获取键盘控制状态信息
        public string GetKeyboardControlStatus()
        {
            return $"键盘控制: {(isKeyboardControlEnabled ? "启用" : "禁用")}, 移动步长: {moveStep:F2}";
        }

        // 手动设置键盘控制CheckBox引用
        public void SetKeyboardControlCheckBox(CheckBox checkBox)
        {
            // 如果之前有CheckBox，先取消事件监听
            if (checkBoxKeyboardControl != null)
            {
                checkBoxKeyboardControl.CheckedChanged -= OnKeyboardControlCheckBoxChanged;
            }

            checkBoxKeyboardControl = checkBox;

            if (checkBoxKeyboardControl != null)
            {
                // 订阅CheckBox状态变化事件
                checkBoxKeyboardControl.CheckedChanged += OnKeyboardControlCheckBoxChanged;
                // 同步当前状态
                checkBoxKeyboardControl.Checked = isKeyboardControlEnabled;
                SafeAppendLog("键盘控制CheckBox已手动设置");
            }
        }

        private void AddColumn(string propertyName, string headerText)
        {
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName, // 绑定到属性名
                HeaderText = headerText          // 列标题
            });
        }

        private void btn_getAllData_Click(object sender, EventArgs e)
        {
            // 获取数据
            var samples = SampleDBHelper.GetAllSamples();
            _bindingList = new BindingList<SampleDBHelper.SampleData>(samples);

            // 禁用自动列生成
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _bindingList;

            // 清除现有列（避免重复添加）
            dataGridView1.Columns.Clear();

            // 手动添加要显示的列
            AddColumn("SampleId", "样本ID");
            AddColumn("Coverage", "覆盖率");
            AddColumn("OriginalImagePath", "原始图像路径");
            AddColumn("CroppedImagePath", "裁剪后的图像路径");
            AddColumn("Uniformity", "均匀度");
            AddColumn("CreatedAt", "创建时间");
            AddColumn("UpdatedAt", "更新时间");

        }

        private void btnSearchByDate_Click_1(object sender, EventArgs e)
        {
            DateTime selectedDate = dateTimePicker1.Value.Date; // 获取选择的日期（忽略时间部分）
            var samples = SampleDBHelper.GetSamplesByDate(selectedDate);
            _bindingList = new BindingList<SampleDBHelper.SampleData>(samples);

            // 禁用自动列生成
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _bindingList;

            // 清除现有列（避免重复添加）
            dataGridView1.Columns.Clear();

            // 手动添加要显示的列
            AddColumn("SampleId", "样本ID");
            AddColumn("Coverage", "覆盖率");
            AddColumn("OriginalImagePath", "原始图像路径");
            AddColumn("CroppedImagePath", "裁剪后的图像路径");
            AddColumn("Uniformity", "均匀度");
            AddColumn("CreatedAt", "创建时间");
            AddColumn("UpdatedAt", "更新时间");
        }

        private void btnSearchCoverage_Click(object sender, EventArgs e)
        {
            double coverage;
            double uniformity;
            bool hasCoverage = double.TryParse(textCoverage.Text, out coverage);
            bool hasUniformity = double.TryParse(textUniformity.Text, out uniformity);

            if (!hasCoverage && !hasUniformity)
            {
                MessageBox.Show("请输入有效的覆盖率或均匀度值。");
                return;
            }

            var samples = SampleDBHelper.GetSamplesByCoverageAndUniformity(
                hasCoverage ? coverage : (double?)null,
                hasUniformity ? uniformity : (double?)null);

            _bindingList = new BindingList<SampleDBHelper.SampleData>(samples);

            // 禁用自动列生成
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _bindingList;

            // 清除现有列（避免重复添加）
            dataGridView1.Columns.Clear();

            // 手动添加要显示的列
            AddColumn("SampleId", "样本ID");
            AddColumn("Coverage", "覆盖率");
            AddColumn("OriginalImagePath", "原始图像路径");
            AddColumn("CroppedImagePath", "裁剪后的图像路径");
            AddColumn("Uniformity", "均匀度");
            AddColumn("CreatedAt", "创建时间");
            AddColumn("UpdatedAt", "更新时间");
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSampleSearch_Click(object sender, EventArgs e)
        {
            var sampleID = dbSampleText.Text;

            if (sampleID == "")
            {
                MessageBox.Show("请输入样品ID");
            }
            else
            {
                var sampleData = SampleDBHelper.GetSampleById(sampleID);

                if (sampleData == null)
                {
                    MessageBox.Show("查询的样品不存在");
                }
                else
                {
                    // 原图展示
                    if (sampleData.OriginalImagePath != "")
                    {
                        if (File.Exists(sampleData.OriginalImagePath))
                        {
                            var image_1 = Image.FromFile(sampleData.OriginalImagePath);

                            PictureBoxHelper.EnableImageInteraction(picBoxSample, image_1);

                            //添加浮动按钮
                            PictureBoxHelper.AddInternalControls(picBoxSample);
                        }
                        else
                        {
                            MessageBox.Show("文件不存在:" + sampleData.OriginalImagePath);
                        }

                    }

                    // 覆盖率
                    if (sampleData.CoverageAnalysisImagePath != "")
                    {
                        if (File.Exists(sampleData.CoverageAnalysisImagePath))
                        {
                            var image_3 = Image.FromFile(sampleData.CoverageAnalysisImagePath);

                            PictureBoxHelper.EnableImageInteraction(picBoxSampleAbnormal, image_3);

                            //添加浮动按钮
                            PictureBoxHelper.AddInternalControls(picBoxSampleAbnormal);
                        }
                        else
                        {
                            MessageBox.Show("文件不存在:" + sampleData.CoverageAnalysisImagePath);
                        }
                    }

                    // 均匀性指标图
                    if (sampleData.UniformityAnalysisImagePath != "")
                    {
                        List<string> filePaths = sampleData.UniformityAnalysisImagePath.Split(';').ToList();

                        //InitializeUniformPlotGrid(filePaths);

                        Console.WriteLine(filePaths);
                        DisplayImagesInTableLayoutPanel(filePaths);


                    }
                }
            }
        }

        private void DisplayImagesInTableLayoutPanel(List<string> filePaths)
        {
            // 清除现有控件
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();

            // 设置行列数（每行显示3张图片）
            int columns = 2;
            int rows = (int)Math.Ceiling((double)filePaths.Count / columns);

            tableLayoutPanel1.ColumnCount = columns;
            tableLayoutPanel1.RowCount = rows;

            // 设置列宽和行高（使用百分比填充）
            for (int i = 0; i < columns; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }
            for (int i = 0; i < rows; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            }

            // 加载并显示图片
            for (int i = 0; i < filePaths.Count; i++)
            {
                try
                {
                    // 创建PictureBox控件（直接作为单元格内容）
                    PictureBox pictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom,  // 保持比例缩放
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(2)
                    };

                    // 加载图片
                    if (File.Exists(filePaths[i]))
                    {
                        Image img = Path.GetExtension(filePaths[i]).ToLower() == ".svg"
                            ? LoadSvgImage(filePaths[i])
                            : Image.FromFile(filePaths[i]);
                        PictureBoxHelper.EnableImageInteraction(pictureBox, img);
                        PictureBoxHelper.AddInternalControls(pictureBox);
                    }
                    else
                    {
                        pictureBox.Image = CreatePlaceholderImage($"图片未找到\n{filePaths[i]}");
                    }

                    // 添加到TableLayoutPanel
                    int row = i / columns;
                    int column = i % columns;
                    tableLayoutPanel1.Controls.Add(pictureBox, column, row);
                }
                catch (Exception ex)
                {
                    Label errorLabel = new Label
                    {
                        Text = $"加载失败: {Path.GetFileName(filePaths[i])}\n{ex.Message}",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    int row = i / columns;
                    int column = i % columns;
                    tableLayoutPanel1.Controls.Add(errorLabel, column, row);
                }
            }
        }
        // SVG图像加载方法
        private Image LoadSvgImage(string svgFilePath)
        {
            try
            {
                // 使用Svg.NET库加载SVG
                var svgDocument = Svg.SvgDocument.Open(svgFilePath);

                // 转换为Bitmap
                return svgDocument.Draw();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SVG加载错误: {ex.Message}");
                return CreatePlaceholderImage($"SVG加载失败\n{Path.GetFileName(svgFilePath)}");
            }
        }

        // 创建占位图像（保持不变）
        private Image CreatePlaceholderImage(string text)
        {
            Bitmap bmp = new Bitmap(400, 300);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Arial", 10))
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(text, font, Brushes.Black,
                        new Rectangle(0, 0, bmp.Width, bmp.Height), sf);
                }
            }
            return bmp;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSearchByDateAndBatchId_Click(object sender, EventArgs e)
        {
            DateTime dateTime = dateTimePicker2.Value;
            int batchId = int.Parse(BatchId.Text);
            var samples = SampleDBHelper.GetSamplesByDateAndBatch(dateTime, batchId);

            // 处理最多5个样品数据的渲染
            for (int i = 1; i <= 5; i++)
            {

                Tuple<double, string> res = ImageUniformityCalculator.CalculateUniformity(samples[i - 1].OriginalImagePath);
                Console.WriteLine($"{res}");
                // 查找对应的控件
                var uniLabel = this.Controls.Find($"batchUniLabel_{i}", true).FirstOrDefault() as Label;
                var pictureBox = this.Controls.Find($"batchPictureBox{i}", true).FirstOrDefault() as PictureBox;
                var covLabel = this.Controls.Find($"batchCovLabel_{i}", true).FirstOrDefault() as Label;

                if (i <= samples.Count)
                {
                    // 有数据，渲染控件
                    var sample = samples[i - 1];

                    // 渲染均匀度标签
                    if (uniLabel != null)
                    {
                        uniLabel.Text = $"均匀度: {sample.Uniformity:F2}%";
                        uniLabel.ForeColor = sample.Uniformity < 90 ? Color.Red : Color.Green;
                        uniLabel.Visible = true;
                        uniLabel.Tag = sample;
                    }

                    // 渲染覆盖率标签
                    if (covLabel != null)
                    {
                        covLabel.Text = $"覆盖率: {sample.Coverage:F2}%";
                        covLabel.Visible = true;
                        covLabel.Tag = sample;
                    }

                    // 渲染图片
                    if (pictureBox != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(sample.OriginalImagePath) && File.Exists(sample.OriginalImagePath))
                            {
                                using (var stream = new FileStream(sample.OriginalImagePath, FileMode.Open, FileAccess.Read))
                                {
                                    var image = Image.FromStream(stream);


                                    PictureBoxHelper.EnableImageInteraction(pictureBox, image);

                                    //添加浮动按钮
                                    PictureBoxHelper.AddInternalControls(pictureBox);
                                }
                            }
                            else
                            {
                                pictureBox.Image = null; // 或者设置默认图片
                            }
                        }
                        catch
                        {
                            pictureBox.Image = null;
                        }
                        pictureBox.Visible = true;
                        pictureBox.Tag = sample;
                    }
                }
                else
                {
                    // 没有数据，清空控件
                    if (uniLabel != null)
                    {
                        uniLabel.Text = $"样品{i}: 无数据";
                        uniLabel.ForeColor = Color.Gray;
                        uniLabel.Visible = true;
                        uniLabel.Tag = null;
                    }

                    if (covLabel != null)
                    {
                        covLabel.Text = "";
                        covLabel.Visible = false;
                        covLabel.Tag = null;
                    }

                    if (pictureBox != null)
                    {
                        pictureBox.Image = null;
                        pictureBox.Visible = false;
                        pictureBox.Tag = null;
                    }
                }
            }

            // 显示结果信息
            if (samples.Count == 0)
            {
                MessageBox.Show($"未找到批次 {batchId} 在 {dateTime:yyyy-MM-dd} 的样品数据",
                               "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }

        private void btnCalculateSimilarity_Click(object sender, EventArgs e)
        {
            DateTime dateTime = dateTimePicker2.Value;
            int batchId = int.Parse(BatchId.Text);

            var samples = SampleDBHelper.GetSamplesByDateAndBatch(dateTime, batchId);


            // 提取 OriginalImagePath
            List<string> imagePaths = samples
                .Where(s => !string.IsNullOrEmpty(s.OriginalImagePath))
                .Select(s => s.OriginalImagePath)
                .ToList();

            // 检查是否找到图像
            if (imagePaths.Count == 0)
            {
                Console.WriteLine("未找到任何图像路径");
                return;
            }

            // 输出找到的图像路径
            Console.WriteLine($"找到 {imagePaths.Count} 张图像:");
            foreach (var path in imagePaths)
            {
                Console.WriteLine($"  {Path.GetFileName(path)}");
            }

            // 计算平均SSIM
            try
            {
                double averageSSIM = SimilarityAnalyzer.CalculateAverageSSIM(imagePaths);

                textConsistency.Text = averageSSIM.ToString();

                Console.WriteLine($"\n批次平均图像质量(SSIM): {averageSSIM:F6}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"计算SSIM时出错: {ex.Message}");
            }


        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private int ExtractNumber(string name)
        {
            var match = Regex.Match(name, @"\d+");
            return match.Success ? int.Parse(match.Value) : int.MaxValue;
        }

        private void btn_FuncTest_Click(object sender, EventArgs e)
        {
            // 图片文件夹路径
            string imageDir = @"C:\Users\SOW111\Desktop\origin";

            // 结果保存路径
            string saveTxtPath = Path.Combine(imageDir, "result.txt");

            // 支持的图片格式
            string[] extensions = { ".jpg", ".png", ".bmp", ".jpeg" };

            // 获取所有图片并按数字顺序排序
            var imageFiles = Directory.GetFiles(imageDir)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(f => ExtractNumber(Path.GetFileNameWithoutExtension(f)))
                .ToList();

            StringBuilder sb = new StringBuilder();

            // 表头
            sb.AppendLine("ImageName\t均匀性\t覆盖率");

            foreach (var imagePath in imageFiles)
            {
                string imageName = Path.GetFileName(imagePath);

                // 裁剪：与主流程一致，先 Process 再保存裁剪图
                ImageProcessor.ProcessResult processedImage = ImageProcessor.ProcessImage(imagePath);
                string croppedImagePath = Path.Combine(Path.GetDirectoryName(imagePath),
                    Path.GetFileNameWithoutExtension(imagePath) + "_cropped.jpg");
                Cv2.ImWrite(croppedImagePath, processedImage.CroppedImage);

                // 计算 glassnumber（基于原图）
                string glassNumber = glassNumberAnalyzer.GetGlassNumber(imagePath);

                // ===== 计算均匀性（基于裁剪图） =====
                Tuple<double, string> res = ImageUniformityCalculator.CalculateUniformity(croppedImagePath);
                double resValue = res.Item1;

                // ===== 计算覆盖率（基于裁剪图） =====
                double ratio = CoverageAnalyzer.detectImage(croppedImagePath);

                // 写入一行
                sb.AppendLine($"{imageName}\t{resValue:F6}\t{ratio:F6}");
            }

            // 一次性写入 txt
            File.WriteAllText(saveTxtPath, sb.ToString(), Encoding.UTF8);

            MessageBox.Show("处理完成，结果已保存到 result.txt");
        }

    }


}