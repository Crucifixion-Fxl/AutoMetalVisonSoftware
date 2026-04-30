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
using AutoMetal.Algorithms;
using System.Threading.Tasks;
using System.Linq;
using AutoMetal.Data;
using AutoMetal.Shared;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using AutoMetal.Services;
using AutoMetal.Infrastructure;
using AutoMetal.Models;
using AutoMetal.Presenters;

namespace AutoMetal
{
    public partial class AutoMetal : Form, IAutoMetalTrainingView
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
        private int is_free =1;

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


        // 可重用的图像缓冲区，避免重复分配内存
        private byte[] reusableImageBuffer = null;

        // 使用 BindingList 而不是 List，以便支持 DataGridView 的自动刷新
        private BindingList<SampleDBHelper.SampleData> _bindingList;

        // 软件运行时长相关
        private DateTime startTime;
        private System.Windows.Forms.Timer runtimeTimer;

        // 算法处理页签相关
        private static readonly string[] AlgImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };
        private readonly Regex _algSampleNameRegex = new Regex(@"^\d{12}$");
        private string _algSelectedImagePath = string.Empty;
        private string _algPreprocessedImagePath = string.Empty;
        private string _algBinaryImagePath = string.Empty;
        private string _algStandardImagePath = string.Empty;
        private string _algCroppedImagePath = string.Empty;
        private string _algOutputImagePath = string.Empty;
        private string _algHeatmapImagePath = string.Empty;
        private List<AlgorithmDirectoryAverage> _algDirectoryAverages = new List<AlgorithmDirectoryAverage>();
        private readonly IModelConversionService _modelConvertService;
        private readonly IDeepLabTrainingService _trainingService;
        private readonly ICoverageInferenceService _inferenceService;
        private readonly IAlgorithmBatchProcessingService _algorithmBatchService;
        private readonly IFileDialogService _dialogService;
        private readonly IWinFormsLogWriter _logWriter;
        private readonly INotificationService _notificationService;
        private readonly IImagePreviewService _imagePreviewService;
        private readonly AutoMetalTrainingPresenter _trainingPresenter;

        public AutoMetal()
        {
            // 初始化组件及窗口固定
            InitializeComponent();
            _modelConvertService = new ModelConversionService(SafeAppendLog);
            _trainingService = new DeepLabTrainingService(SafeAppendLog);
            _inferenceService = new CoverageInferenceService(SafeAppendLog);
            _algorithmBatchService = new AlgorithmBatchProcessingService();
            _dialogService = new FileDialogService();
            _logWriter = new WinFormsLogWriter(80);
            _notificationService = new NotificationService();
            _imagePreviewService = new ImagePreviewService(SafeShowWarning);
            _trainingPresenter = new AutoMetalTrainingPresenter(
                this,
                _modelConvertService,
                _trainingService,
                _inferenceService,
                _dialogService);

            // 加载用户自定义参数（无配置时保持默认值）
            AutoMetalConstants.LoadUserSettings();

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

            // 默认显示值：避免 Brightness/Contrast/XY/Z 输入框为空
            textBrightness.Text = AutoMetalConstants.initBrightness.ToString();
            textContrast.Text = AutoMetalConstants.initContrast.ToString();
            textXY1.Text = AutoMetalConstants.initX.ToString();
            textXY2.Text = AutoMetalConstants.initY.ToString();
            textZ.Text = AutoMetalConstants.initZ.ToString();

            // 订阅窗口结束事件
            this.FormClosed += OnFormClosed;

            // 初始化样品数据库
            SampleDBHelper.Initialize(AutoMetalConstants.dbPath);

            // 默认选择正常模式
            radioButton1.Checked = true;

            // 打开参数设置界面
            setingsToolStripMenuItem.Click += setingsToolStripMenuItem_Click;

            // 初始化算法处理页签交互
            InitializeAlgorithmTabHandlers();
            InitializeTrainingTabHandlers();
            InitializeDatabaseTabHandlers();
            InitializeIndustrialVisualTheme();
            InitializeDatabaseTabIndustrialStyle();

        }

        private void InitializeDatabaseTabIndustrialStyle()
        {
            // 数据库页采用浅灰 + 白底的工业化面板布局，提升层次感与可读性。
            tabPage2.BackColor = Color.FromArgb(240, 244, 248);
            dBGridView.BackColor = Color.FromArgb(240, 244, 248);
            dBGridView.Padding = new Padding(12);

            var toolbarPanel = new Panel
            {
                Name = "panelDbToolbar",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(dBGridView.ClientSize.Width - 24, 72),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblDate = new Label
            {
                Text = "日期筛选",
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 71, 79),
                Location = new System.Drawing.Point(16, 13)
            };
            var lblMetric = new Label
            {
                Text = "指标筛选",
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 71, 79),
                Location = new System.Drawing.Point(500, 13)
            };

            dateTimePicker1.Location = new System.Drawing.Point(16, 36);
            dateTimePicker1.Size = new System.Drawing.Size(160, 23);
            dateTimePicker1.CalendarMonthBackground = Color.White;

            btnSearchByDate.Location = new System.Drawing.Point(186, 35);
            btnSearchByDate.Size = new System.Drawing.Size(100, 28);
            btnSearchByDate.Text = "按日期查询";
            ApplyIndustrialButtonStyle(btnSearchByDate, false);

            btn_getAllData.Location = new System.Drawing.Point(296, 35);
            btn_getAllData.Size = new System.Drawing.Size(106, 28);
            btn_getAllData.Text = "查看全部";
            ApplyIndustrialButtonStyle(btn_getAllData, true);

            label36.Location = new System.Drawing.Point(500, 40);
            label36.ForeColor = Color.FromArgb(84, 96, 105);
            textCoverage.Location = new System.Drawing.Point(560, 36);
            textCoverage.Size = new System.Drawing.Size(78, 23);

            label37.Location = new System.Drawing.Point(650, 40);
            label37.ForeColor = Color.FromArgb(84, 96, 105);
            textUniformity.Location = new System.Drawing.Point(710, 36);
            textUniformity.Size = new System.Drawing.Size(78, 23);

            btnSearchCoverageAndUniformity.Location = new System.Drawing.Point(802, 35);
            btnSearchCoverageAndUniformity.Size = new System.Drawing.Size(110, 28);
            btnSearchCoverageAndUniformity.Text = "指标查询";
            ApplyIndustrialButtonStyle(btnSearchCoverageAndUniformity, false);

            toolbarPanel.Controls.Add(lblDate);
            toolbarPanel.Controls.Add(lblMetric);
            toolbarPanel.Controls.Add(dateTimePicker1);
            toolbarPanel.Controls.Add(btnSearchByDate);
            toolbarPanel.Controls.Add(btn_getAllData);
            toolbarPanel.Controls.Add(label36);
            toolbarPanel.Controls.Add(textCoverage);
            toolbarPanel.Controls.Add(label37);
            toolbarPanel.Controls.Add(textUniformity);
            toolbarPanel.Controls.Add(btnSearchCoverageAndUniformity);

            dBGridView.Controls.Add(toolbarPanel);
            dBGridView.Controls.SetChildIndex(toolbarPanel, 0);

            dataGridView1.Location = new System.Drawing.Point(12, 94);
            dataGridView1.Size = new System.Drawing.Size(dBGridView.ClientSize.Width - 24, dBGridView.ClientSize.Height - 106);
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 49, 68);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            dataGridView1.ColumnHeadersHeight = 34;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.RowTemplate.Height = 28;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(223, 240, 255);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.FromArgb(18, 35, 54);
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 252);
            dataGridView1.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.ForeColor = Color.FromArgb(33, 43, 54);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        }

        private void InitializeDatabaseTabHandlers()
        {
            dataGridView1.CellDoubleClick -= dataGridView1_CellDoubleClick;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            LoadAllDatabaseRows();
        }

        private static void ApplyIndustrialButtonStyle(Button button, bool emphasized)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);

            if (emphasized)
            {
                button.BackColor = Color.FromArgb(39, 93, 156);
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = Color.FromArgb(31, 77, 133);
            }
            else
            {
                button.BackColor = Color.FromArgb(245, 248, 251);
                button.ForeColor = Color.FromArgb(41, 56, 70);
                button.FlatAppearance.BorderColor = Color.FromArgb(196, 205, 214);
            }
        }

        private void InitializeIndustrialVisualTheme()
        {
            var rootBackColor = Color.FromArgb(234, 239, 245);
            BackColor = rootBackColor;

            menuStrip1.BackColor = Color.FromArgb(28, 41, 56);
            menuStrip1.ForeColor = Color.FromArgb(234, 241, 248);
            menuStrip1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
            ApplyToolStripTheme(menuStrip1.Items);

            ApplyIndustrialThemeToControlTree(this);
        }

        private void ApplyToolStripTheme(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.ForeColor = Color.FromArgb(234, 241, 248);
                item.BackColor = Color.FromArgb(28, 41, 56);
                item.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);

                if (item is ToolStripDropDownItem dropDownItem)
                {
                    ApplyToolStripTheme(dropDownItem.DropDownItems);
                }
            }
        }

        private void ApplyIndustrialThemeToControlTree(Control root)
        {
            foreach (Control control in root.Controls)
            {
                ApplyIndustrialTheme(control);
                if (control.HasChildren)
                {
                    ApplyIndustrialThemeToControlTree(control);
                }
            }
        }

        private void ApplyIndustrialTheme(Control control)
        {
            if (control is TabControl tabControl)
            {
                tabControl.Appearance = TabAppearance.Normal;
                tabControl.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
                tabControl.Padding = new System.Drawing.Point(16, 6);
                tabControl.BackColor = Color.FromArgb(226, 233, 241);
                return;
            }

            if (control is TabPage tabPage)
            {
                tabPage.BackColor = Color.FromArgb(240, 244, 248);
                tabPage.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
                return;
            }

            if (control is GroupBox groupBox)
            {
                groupBox.BackColor = Color.White;
                groupBox.ForeColor = Color.FromArgb(42, 56, 70);
                groupBox.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
                return;
            }

            if (control is Button button)
            {
                // 主按钮（开始/运行/连接）使用强调色，其余按钮使用次级风格。
                bool emphasized =
                    button.Name.IndexOf("Start", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    button.Name.IndexOf("Run", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    button.Name.IndexOf("Connect", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    button.Name.IndexOf("Analysis", StringComparison.OrdinalIgnoreCase) >= 0;
                ApplyIndustrialButtonStyle(button, emphasized);
                return;
            }

            if (control is Label label)
            {
                label.ForeColor = Color.FromArgb(66, 79, 91);
                label.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
                return;
            }

            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = Color.White;
                textBox.ForeColor = Color.FromArgb(28, 42, 55);
                textBox.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
                return;
            }

            if (control is RichTextBox richTextBox)
            {
                richTextBox.BorderStyle = BorderStyle.FixedSingle;
                richTextBox.BackColor = Color.FromArgb(251, 253, 255);
                richTextBox.ForeColor = Color.FromArgb(24, 39, 52);
                richTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular);
                return;
            }

            if (control is ListBox listBox)
            {
                listBox.BorderStyle = BorderStyle.FixedSingle;
                listBox.BackColor = Color.White;
                listBox.ForeColor = Color.FromArgb(34, 48, 61);
                listBox.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
                return;
            }

            if (control is DataGridView grid)
            {
                grid.EnableHeadersVisualStyles = false;
                grid.BackgroundColor = Color.White;
                grid.BorderStyle = BorderStyle.FixedSingle;
                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                grid.GridColor = Color.FromArgb(218, 226, 234);
                grid.RowHeadersVisible = false;
                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 49, 68);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
                grid.ColumnHeadersHeight = 34;
                grid.DefaultCellStyle.BackColor = Color.White;
                grid.DefaultCellStyle.ForeColor = Color.FromArgb(32, 44, 56);
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(223, 240, 255);
                grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(18, 35, 54);
                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 252);
                grid.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
                return;
            }

            if (control is Panel panel)
            {
                if (panel.Name != "panelDbToolbar")
                {
                    panel.BackColor = Color.FromArgb(240, 244, 248);
                }
                return;
            }

            if (control is PictureBox pictureBox)
            {
                if (pictureBox.BorderStyle == BorderStyle.None)
                {
                    pictureBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }
        }

        private void InitializeAlgorithmTabHandlers()
        {
            radioAlgManualMode.Checked = true;
            ToggleAlgManualMaskControls(true);

            btnAlgBrowseInput.Click += btnAlgBrowseInput_Click;
            btnAlgPreprocess.Click += btnAlgPreprocess_Click;
            treeAlgSamples.AfterSelect += treeAlgSamples_AfterSelect;
            radioAlgManualMode.CheckedChanged += radioAlgMode_CheckedChanged;
            radioAlgAutoMode.CheckedChanged += radioAlgMode_CheckedChanged;
            btnAlgBrowseMaskDir.Click += btnAlgBrowseMaskDir_Click;
            btnAlgLoadMaskList.Click += btnAlgLoadMaskList_Click;
            btnAlgRun.Click += btnAlgRun_Click;
            btnAlgShowDirAverages.Click += btnAlgShowDirAverages_Click;
            btnAlgPreviewBinary.Click += btnAlgPreviewBinary_Click;
            btnAlgPreviewStandard.Click += btnAlgPreviewStandard_Click;
            btnAlgPreviewCropped.Click += btnAlgPreviewCropped_Click;
            btnAlgPreviewOutput.Click += btnAlgPreviewOutput_Click;
            btnAlgPreviewHeatmap.Click += btnAlgPreviewHeatmap_Click;
            btnAlgPreviewOriginal.Click += btnAlgPreviewOriginal_Click;
            UpdateAlgPreviewButtonState(btnAlgPreviewOriginal);
        }

        private void InitializeTrainingTabHandlers()
        {
            btnBrowseTestSet.Click += btnBrowseTestSet_Click;
            // 兼容：优先使用新加的 btnBrowserEngine；若不存在则回退到旧按钮 btnBrowsePt
            var browserEngineBtn = this.Controls.Find("btnBrowserEngine", true).FirstOrDefault() as Button;
            if (browserEngineBtn != null)
            {
                browserEngineBtn.Click += btnBrowseEnginePath_Click;
            }
            else
            {
                btnBrowsePt.Click += btnBrowseEnginePath_Click;
            }
            // 兼容：优先绑定新增 btnStartInfer，否则绑定旧按钮 btnStartTrain
            var startInferBtn = this.Controls.Find("btnStartInfer", true).FirstOrDefault() as Button;
            if (startInferBtn != null)
            {
                startInferBtn.Click += btnStartInfer_Click;
            }
            else
            {
                var legacyStartBtn = this.Controls.Find("btnStartTrain", true).FirstOrDefault() as Button;
                if (legacyStartBtn != null)
                {
                    legacyStartBtn.Click += btnStartInfer_Click;
                }
            }
            btnStopTrain.Click += btnStopTrain_Click;
            btnExportTensorEngine.Click += btnExportPtToEngine_Click;
            btnExportOnnx.Click += btnBrowseBestPt_Click;
        }

        private void btnBrowseBestPt_Click(object sender, EventArgs e)
        {
            _trainingPresenter.BrowseBestPt();
        }

        private async void btnExportPtToEngine_Click(object sender, EventArgs e)
        {
            await _trainingPresenter.ExportPtToEngineAsync();
        }

        private void btnStopTrain_Click(object sender, EventArgs e)
        {
            _ = _trainingPresenter.ToggleTrainingAsync();
        }

        private void UpdateTrainStatus(string status)
        {
            if (labelTrainStatus.InvokeRequired)
            {
                labelTrainStatus.Invoke(new Action<string>(UpdateTrainStatus), status);
            }
            else
            {
                labelTrainStatus.Text = status;
            }
        }

        private void UpdateTrainProgress(int current, int total)
        {
            if (progressBarTrain.InvokeRequired)
            {
                progressBarTrain.Invoke(new Action<int, int>(UpdateTrainProgress), current, total);
                return;
            }

            int safeTotal = Math.Max(total, 1);
            progressBarTrain.Maximum = safeTotal;
            progressBarTrain.Value = Math.Min(Math.Max(current, 0), safeTotal);
        }

        private void LoadLatestTrainingCurveImage()
        {
            try
            {
                string logsDir = Path.Combine(AutoMetalConstants.deepLabProjectPath, "logs");
                if (!Directory.Exists(logsDir))
                {
                    return;
                }

                string latestLossDir = Directory.EnumerateDirectories(logsDir, "loss_*", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(x => x)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(latestLossDir))
                {
                    return;
                }

                string curvePath = Path.Combine(latestLossDir, "epoch_loss.png");
                if (!File.Exists(curvePath))
                {
                    return;
                }

                LoadImageToPictureBox(pictureBoxMergedCurve, curvePath);
                SafeAppendLog($"已加载训练曲线图：{curvePath}");
            }
            catch (Exception ex)
            {
                SafeAppendLog($"训练曲线图加载失败：{ex.Message}");
            }
        }

        private void btnBrowseTestSet_Click(object sender, EventArgs e)
        {
            _trainingPresenter.BrowseTestSet();
        }

        private void btnBrowseEnginePath_Click(object sender, EventArgs e)
        {
            _trainingPresenter.BrowseEnginePath();
        }

        private void btnStartInfer_Click(object sender, EventArgs e)
        {
            _trainingPresenter.StartInference();
        }

        string IAutoMetalTrainingView.BestPtPath
        {
            get => txtBestPtPath.Text;
            set => txtBestPtPath.Text = value;
        }

        string IAutoMetalTrainingView.TensorEnginePath
        {
            get => txtTensorEnginePath.Text;
            set => txtTensorEnginePath.Text = value;
        }

        string IAutoMetalTrainingView.TestSetPath
        {
            get => txtTestSetPath.Text;
            set => txtTestSetPath.Text = value;
        }

        string IAutoMetalTrainingView.EpochText => txtEpoch.Text;
        string IAutoMetalTrainingView.BatchSizeText => txtBatchSize.Text;

        void IAutoMetalTrainingView.ShowWarning(string message) => SafeShowWarning(message);
        void IAutoMetalTrainingView.ShowInfo(string message) => SafeShowInfo(message);
        void IAutoMetalTrainingView.SetConversionStatus(string status)
        {
            if (labelConvertStatus.InvokeRequired)
            {
                labelConvertStatus.Invoke(new Action<string>(((IAutoMetalTrainingView)this).SetConversionStatus), status);
                return;
            }

            labelConvertStatus.Text = status;
        }
        void IAutoMetalTrainingView.SetConversionInProgress(bool inProgress)
        {
            if (btnExportTensorEngine.InvokeRequired)
            {
                btnExportTensorEngine.Invoke(new Action<bool>(((IAutoMetalTrainingView)this).SetConversionInProgress), inProgress);
                return;
            }

            btnExportTensorEngine.Enabled = !inProgress;
            btnExportTensorEngine.Text = inProgress ? "Engine 转换中..." : "Engine 转换";
            btnExportTensorEngine.UseVisualStyleBackColor = !inProgress;
            btnExportTensorEngine.BackColor = inProgress ? Color.LightGreen : SystemColors.Control;
        }
        void IAutoMetalTrainingView.SetTrainButtonText(string text) => btnStopTrain.Text = text;
        void IAutoMetalTrainingView.SetTrainStatus(string status) => UpdateTrainStatus(status);
        void IAutoMetalTrainingView.ResetTrainProgress(int total)
        {
            progressBarTrain.Minimum = 0;
            progressBarTrain.Maximum = Math.Max(total, 1);
            progressBarTrain.Value = 0;
        }
        void IAutoMetalTrainingView.UpdateTrainProgress(int current, int total) => UpdateTrainProgress(current, total);
        void IAutoMetalTrainingView.SetInferStatus(string status) => UpdateInferStatus(status);
        void IAutoMetalTrainingView.UpdateInferProgress(int current, int total) => UpdateInferProgress(current, total);
        void IAutoMetalTrainingView.LoadLatestTrainingCurveImage() => LoadLatestTrainingCurveImage();

        private void UpdateInferStatus(string status)
        {
            if (labelInferStatus.InvokeRequired)
            {
                labelInferStatus.Invoke(new Action<string>(UpdateInferStatus), status);
            }
            else
            {
                labelInferStatus.Text = status;
            }
        }

        private void UpdateInferProgress(int current, int total)
        {
            if (progressBarInfer.InvokeRequired)
            {
                progressBarInfer.Invoke(new Action<int, int>(UpdateInferProgress), current, total);
                return;
            }

            int safeTotal = Math.Max(total, 1);
            progressBarInfer.Minimum = 0;
            progressBarInfer.Maximum = safeTotal;
            progressBarInfer.Value = Math.Min(Math.Max(current, 0), safeTotal);
            Application.DoEvents();
        }

        private void setingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new ParameterSettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
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


        private bool Connect(string strIP, int port)
        {
            if (!m_analysis.connect(strIP.ToCharArray(), port))
            {
                SafeShowWarning("连接失败");
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
            is_free = 1;

            // 连接成功后，回填设备当前参数到UI输入框
            RefreshDeviceParamsToUI();

            return true;
        }

        private void RefreshDeviceParamsToUI()
        {
            if (m_analysis == null)
            {
                return;
            }

            // 读取当前XY
            float currentX, currentY;
            var xyRet = m_analysis.getXY(out currentX, out currentY);
            if (xyRet == Moac_retCode.RC_FINISH)
            {
                SafeUpdateTextBox(textXY1, currentX.ToString("F2"));
                SafeUpdateTextBox(textXY2, currentY.ToString("F2"));
            }
            else
            {
                SafeAppendLog($"读取XY失败: {xyRet}");
            }

            // 读取当前Z
            float currentZ;
            var zRet = m_analysis.getZ(out currentZ);
            if (zRet == Moac_retCode.RC_FINISH)
            {
                SafeUpdateTextBox(textZ, currentZ.ToString("F2"));
            }
            else
            {
                SafeAppendLog($"读取Z失败: {zRet}");
            }

            // 读取当前对比度
            int cMin, cMax, cCur;
            var contrastRet = m_analysis.getContrast(out cMin, out cMax, out cCur);
            if (contrastRet == Moac_retCode.RC_FINISH)
            {
                SafeUpdateTextBox(textContrast, cCur.ToString());
            }
            else
            {
                SafeAppendLog($"读取Contrast失败: {contrastRet}");
            }
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
                SafeShowWarning("无效的IP地址或端口号");
                return;
            }
            if (!Connect(strIP, port))
            {
                SafeShowWarning("连接失败");
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

                if (processedImage == null || processedImage.CroppedImage == null || processedImage.CroppedImage.Empty())
                {
                    SafeAppendLog("自动扫描后图像处理失败：裁剪图为空，跳过保存和后续分析");
                    SafeShowWarning("未检测到镀膜样品");
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(samplePath.Text) && File.Exists(samplePath.Text))
                        {
                            File.Delete(samplePath.Text);
                            SafeAppendLog($"已删除无效样品图像: {samplePath.Text}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SafeAppendLog($"删除无效样品图像失败: {ex.Message}");
                    }
                    SafeUpdateTextBox(expID, "");
                    SafeUpdateTextBox(sampleID, "");
                    SafeUpdateTextBox(samplePath, "");
                    is_free = 1;
                    return;
                }

                if (!TrySaveMatImage(croppedImagePath, processedImage.CroppedImage, "自动扫描裁剪图"))
                {
                    is_free = 1;
                    return;
                }

                Bitmap bitmap = BitmapConverter.ToBitmap(processedImage.CroppedImage);

                Ori_picture.Image = bitmap;

                ////// 识别出来的玻璃编号ID
                //var sampleID_reco = GlassNumberAnalyzer.GetGlassNumber(samplePath.Text);

                //// 均匀性计算
                //Tuple<double, string> nUniformity = ImageUniformityCalculator.CalculateUniformity(croppedImagePath);


                //double nUniformityValue = nUniformity.Item1;


                //SafeUpdateTextBox(textBox6, nUniformity.Item2);


                //// 覆盖率计算
                //double nConverageRate = CoverageAnalyzer.detectImage(croppedImagePath);


                //SafeUpdateTextBox(textBox5, nConverageRate.ToString());


                //var sample = new SampleDBHelper.SampleData
                //{
                //    SampleId = sampleID_reco,
                //    Coverage = nConverageRate,
                //    OriginalImagePath = samplePath.Text,
                //    CroppedImagePath = croppedImagePath,
                //    Uniformity = nUniformityValue,
                //    UniformityAnalysisImagePath = nUniformity.Item2,
                //    CoverageAnalysisImagePath = null
                //};

                //SampleDBHelper.UpsertSample(sample);

                int id_unuse;
                m_analysis.setXY(out id_unuse, AutoMetalConstants.reset_x, AutoMetalConstants.reset_y);

                Thread.Sleep(15000);

                is_free = 1;

                // 仅当 UDP 已启动且目标端点有效时才发送 complete，否则会空引用且机械臂收不到
                if (udpClient != null && isRunning && _targetEndPoint != null)
                {
                    SendMessageAysnc("complete");
                }
                else
                {
                    LogText("UDP 未启动，未向机械臂发送 complete");
                }

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
                SafeShowWarning("无效的坐标值！");
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
                SafeShowWarning("无效的坐标值！");
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
                SafeShowWarning("无效的亮度值！");
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
                SafeShowWarning("无效的对比度值!");
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
                SafeShowWarning("无效的坐标值！");
            }

        }

        private static string GetNextNumericJpgSampleId(string folderPath)
        {
            // 只识别形如 "1.jpg" / "2.jpg" 的文件名（纯数字 + .jpg），忽略其它命名格式
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return "1";
            }

            int maxId = 0;
            foreach (var file in Directory.EnumerateFiles(folderPath, "*.jpg", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(name, out int id) && id > maxId)
                {
                    maxId = id;
                }
            }

            return (maxId + 1).ToString();
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
                SafeShowWarning("无效的坐标值！");
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

        private bool TrySaveMatImage(string filePath, Mat image, string scene)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                SafeAppendLog($"{scene}: 保存失败，文件路径为空");
                return false;
            }

            if (image == null || image.Empty())
            {
                SafeAppendLog($"{scene}: 保存失败，图像为空（可能是全黑图或未检测到有效区域）");
                return false;
            }

            try
            {
                return Cv2.ImWrite(filePath, image);
            }
            catch (Exception ex)
            {
                SafeAppendLog($"{scene}: 保存失败，异常：{ex.Message}");
                return false;
            }
        }

        private void SafeShowWarning(string message)
        {
            _notificationService.ShowWarning(this, message);
        }

        private void SafeShowInfo(string message)
        {
            _notificationService.ShowInfo(this, message);
        }

        private void btnAlgBrowseInput_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(txtAlgInputImage.Text) && Directory.Exists(txtAlgInputImage.Text))
                {
                    folderDialog.SelectedPath = txtAlgInputImage.Text;
                }

                if (folderDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtAlgInputImage.Text = folderDialog.SelectedPath;
                    LoadAlgorithmSampleTree(folderDialog.SelectedPath);
                }
            }
        }

        private void LoadAlgorithmSampleTree(string rootFolder)
        {
            treeAlgSamples.Nodes.Clear();
            ClearAlgorithmProcessingState();

            if (!Directory.Exists(rootFolder))
            {
                SafeShowWarning("输入图像目录不存在");
                return;
            }

            for (int i = 1; i <= 10; i++)
            {
                string batchFolder = Path.Combine(rootFolder, i.ToString());
                if (!Directory.Exists(batchFolder))
                {
                    continue;
                }

                var oriFolder = Directory.EnumerateDirectories(batchFolder, "*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(dir => string.Equals(Path.GetFileName(dir), "ori", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrWhiteSpace(oriFolder) || !Directory.Exists(oriFolder))
                {
                    continue;
                }

                var allImageFiles = Directory.EnumerateFiles(oriFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => AlgImageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .OrderBy(file => Path.GetFileName(file))
                    .ToList();

                var invalidNameFile = allImageFiles
                    .FirstOrDefault(file => !_algSampleNameRegex.IsMatch(Path.GetFileNameWithoutExtension(file)));
                if (!string.IsNullOrWhiteSpace(invalidNameFile))
                {
                    treeAlgSamples.Nodes.Clear();
                    ClearAlgorithmProcessingState();
                    SafeShowWarning($"检测到命名不符合12位规则的图像：{Path.GetFileName(invalidNameFile)}\n路径：{oriFolder}");
                    return;
                }

                var imageFiles = allImageFiles
                    .OrderBy(file => Path.GetFileNameWithoutExtension(file))
                    .ToList();

                var batchNode = new TreeNode($"{i} ({imageFiles.Count})");
                foreach (var imageFile in imageFiles)
                {
                    var imageNode = new TreeNode(Path.GetFileName(imageFile)) { Tag = imageFile };
                    batchNode.Nodes.Add(imageNode);
                }

                if (batchNode.Nodes.Count > 0)
                {
                    treeAlgSamples.Nodes.Add(batchNode);
                }
            }

            treeAlgSamples.ExpandAll();
            if (treeAlgSamples.Nodes.Count == 0)
            {
                SafeShowWarning("未找到符合规则的样品图像（结构：数字目录/ori(或Ori)/12位文件名）");
            }
        }

        private void treeAlgSamples_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string imagePath = e.Node.Tag as string;
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                return;
            }

            _algSelectedImagePath = imagePath;
            _algPreprocessedImagePath = string.Empty;
            _algBinaryImagePath = string.Empty;
            _algStandardImagePath = string.Empty;
            _algCroppedImagePath = string.Empty;
            _algOutputImagePath = string.Empty;
            _algHeatmapImagePath = string.Empty;
            txtAlgCoverageResult.Text = "";
            txtAlgUniformityResult.Text = "";
            labelAlgSelectedSample.Text = $"当前样品：{Path.GetFileName(imagePath)}";
            ClearPictureBoxImage(picAlgPreprocessed);
            LoadImageToPictureBox(picAlgOriginal, imagePath);
            UpdateAlgPreviewButtonState(btnAlgPreviewOriginal);

            RefreshSelectedSamplePreviewFromOutputs();
        }

        private void btnAlgPreprocess_Click(object sender, EventArgs e)
        {
            var allImages = GetAllLoadedImages();
            if (allImages.Count == 0)
            {
                SafeShowWarning("请先加载目录中的样品图像");
                return;
            }

            int successCount = 0;
            int failedCount = 0;
            foreach (var imagePath in allImages)
            {
                if (TryPreprocessAndSaveOutputs(imagePath, updatePreview: false, out _))
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            if (!string.IsNullOrWhiteSpace(_algSelectedImagePath) && File.Exists(_algSelectedImagePath))
            {
                RefreshSelectedSamplePreviewFromOutputs();
            }

            SafeAppendLog($"批量预处理完成：总数={allImages.Count}，成功={successCount}，失败={failedCount}");
        }

        private void RefreshSelectedSamplePreviewFromOutputs()
        {
            if (string.IsNullOrWhiteSpace(_algSelectedImagePath) || !File.Exists(_algSelectedImagePath))
            {
                return;
            }

            var outputPaths = GetOutputImagePathsBySource(_algSelectedImagePath);
            _algBinaryImagePath = outputPaths.binaryPath;
            _algStandardImagePath = outputPaths.standardPath;
            _algCroppedImagePath = outputPaths.croppedPath;
            _algOutputImagePath = outputPaths.outputPath;
            _algHeatmapImagePath = outputPaths.heatmapPath;

            if (File.Exists(_algOutputImagePath))
            {
                _algPreprocessedImagePath = _algOutputImagePath;
                LoadImageToPictureBox(picAlgPreprocessed, _algOutputImagePath);
                labelAlgPreprocessed.Text = "算法输出图（当前样品）";
                return;
            }

            if (File.Exists(_algCroppedImagePath))
            {
                _algPreprocessedImagePath = _algCroppedImagePath;
                LoadImageToPictureBox(picAlgPreprocessed, _algCroppedImagePath);
                labelAlgPreprocessed.Text = "裁剪图（当前样品）";
            }
        }

        private bool TryPreprocessAndSaveOutputs(string imagePath, bool updatePreview, out string croppedPathOut)
        {
            croppedPathOut = string.Empty;
            var processedImage = ImageProcessor.ProcessImage(
                imagePath,
                1000000,
                50,
                rotate90Counterclockwise: false,
                overwriteInputWithResized: false);
            if (processedImage == null || processedImage.CroppedImage == null || processedImage.CroppedImage.Empty())
            {
                SafeShowWarning($"预处理失败：未检测到有效镀膜区域\n样品：{Path.GetFileName(imagePath)}");
                return false;
            }

            string fileName = Path.GetFileName(imagePath);
            string oriFolder = Path.GetDirectoryName(imagePath);
            string parentFolder = Directory.GetParent(oriFolder)?.FullName ?? oriFolder;
            string binaryFolder = Path.Combine(parentFolder, "binary");
            string standardFolder = Path.Combine(parentFolder, "standard");
            string croppedFolder = Path.Combine(parentFolder, "cropped");

            Directory.CreateDirectory(binaryFolder);
            Directory.CreateDirectory(standardFolder);
            Directory.CreateDirectory(croppedFolder);

            string binaryPath = Path.Combine(binaryFolder, fileName);
            string standardPath = Path.Combine(standardFolder, fileName);
            string croppedPath = Path.Combine(croppedFolder, fileName);
            string outputPath = Path.Combine(parentFolder, "output", Path.GetFileNameWithoutExtension(fileName) + "_mask" + Path.GetExtension(fileName));
            string heatmapPath = Path.Combine(parentFolder, "heatmap", Path.GetFileNameWithoutExtension(fileName) + "_heatmap.png");

            if (!TrySaveMatImage(binaryPath, processedImage.BinaryImage, "算法页签二值图保存") ||
                !TrySaveMatImage(standardPath, processedImage.CorrectedImage, "算法页签透视矫正图保存") ||
                !TrySaveMatImage(croppedPath, processedImage.CroppedImage, "算法页签结果图保存"))
            {
                processedImage.Dispose();
                return false;
            }

            croppedPathOut = croppedPath;
            if (updatePreview)
            {
                _algBinaryImagePath = binaryPath;
                _algStandardImagePath = standardPath;
                _algCroppedImagePath = croppedPath;
                _algOutputImagePath = outputPath;
                _algHeatmapImagePath = heatmapPath;
                _algPreprocessedImagePath = croppedPath;

                _imagePreviewService.ReplaceImage(picAlgPreprocessed, BitmapConverter.ToBitmap(processedImage.CroppedImage));
                labelAlgPreprocessed.Text = "裁剪图（当前样品）";
            }

            processedImage.Dispose();
            return true;
        }

        private void btnAlgPreviewBinary_Click(object sender, EventArgs e)
        {
            UpdateAlgPreviewButtonState(btnAlgPreviewBinary);
            ShowAlgProcessedPreview(_algBinaryImagePath, "二值图");
        }

        private void btnAlgPreviewStandard_Click(object sender, EventArgs e)
        {
            UpdateAlgPreviewButtonState(btnAlgPreviewStandard);
            ShowAlgProcessedPreview(_algStandardImagePath, "矫正图");
        }

        private void btnAlgPreviewCropped_Click(object sender, EventArgs e)
        {
            UpdateAlgPreviewButtonState(btnAlgPreviewCropped);
            ShowAlgProcessedPreview(_algCroppedImagePath, "裁剪图");
        }

        private void btnAlgPreviewOutput_Click(object sender, EventArgs e)
        {
            UpdateAlgPreviewButtonState(btnAlgPreviewOutput);
            ShowAlgProcessedPreview(_algOutputImagePath, "算法输出图");
        }

        private void btnAlgPreviewHeatmap_Click(object sender, EventArgs e)
        {
            UpdateAlgPreviewButtonState(btnAlgPreviewHeatmap);
            ShowAlgProcessedPreview(_algHeatmapImagePath, "热力图");
        }

        private void btnAlgPreviewOriginal_Click(object sender, EventArgs e)
        {
            UpdateAlgPreviewButtonState(btnAlgPreviewOriginal);
            if (string.IsNullOrWhiteSpace(_algSelectedImagePath) || !File.Exists(_algSelectedImagePath))
            {
                SafeShowWarning("请先在左侧树中选择样品图像");
                return;
            }

            LoadImageToPictureBox(picAlgOriginal, _algSelectedImagePath);
        }

        private void UpdateAlgPreviewButtonState(Button activeButton)
        {
            var buttons = new[]
            {
                btnAlgPreviewOriginal,
                btnAlgPreviewHeatmap,
                btnAlgPreviewOutput,
                btnAlgPreviewBinary,
                btnAlgPreviewStandard,
                btnAlgPreviewCropped
            };

            foreach (var btn in buttons)
            {
                bool isActive = ReferenceEquals(btn, activeButton);
                btn.FlatStyle = FlatStyle.Standard;
                btn.UseVisualStyleBackColor = !isActive;
                btn.BackColor = isActive ? System.Drawing.Color.LightSteelBlue : System.Drawing.SystemColors.Control;
            }
        }

        private void ShowAlgProcessedPreview(string imagePath, string previewName)
        {
            if (string.IsNullOrWhiteSpace(_algSelectedImagePath) || !File.Exists(_algSelectedImagePath))
            {
                SafeShowWarning("请先在左侧树中选择样品图像");
                return;
            }

            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                var outputPaths = GetOutputImagePathsBySource(_algSelectedImagePath);
                if (previewName.Contains("二值"))
                {
                    imagePath = outputPaths.binaryPath;
                    _algBinaryImagePath = imagePath;
                }
                else if (previewName.Contains("矫正"))
                {
                    imagePath = outputPaths.standardPath;
                    _algStandardImagePath = imagePath;
                }
                else if (previewName.Contains("热力图"))
                {
                    imagePath = outputPaths.heatmapPath;
                    _algHeatmapImagePath = imagePath;
                }
                else
                {
                    if (previewName.Contains("算法输出"))
                    {
                        imagePath = outputPaths.outputPath;
                        _algOutputImagePath = imagePath;
                    }
                    else
                    {
                        imagePath = outputPaths.croppedPath;
                        _algCroppedImagePath = imagePath;
                        if (File.Exists(imagePath))
                        {
                            _algPreprocessedImagePath = imagePath;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                if (previewName.Contains("算法输出"))
                {
                    SafeShowWarning("当前没有可预览的算法输出图，请先执行覆盖率计算");
                }
                else if (previewName.Contains("热力图"))
                {
                    SafeShowWarning("当前没有可预览的热力图，请先执行覆盖率与均匀性计算");
                }
                else
                {
                    SafeShowWarning($"当前没有可预览的{previewName}，请先执行预处理");
                }
                return;
            }

            LoadImageToPictureBox(picAlgPreprocessed, imagePath);
            labelAlgPreprocessed.Text = $"{previewName}（当前样品）";
        }

        private void radioAlgMode_CheckedChanged(object sender, EventArgs e)
        {
            ToggleAlgManualMaskControls(radioAlgManualMode.Checked);
        }

        private void ToggleAlgManualMaskControls(bool manualMode)
        {
            txtAlgMaskDir.Enabled = false;
            btnAlgBrowseMaskDir.Enabled = false;
            btnAlgLoadMaskList.Enabled = manualMode;
            listBoxAlgMasks.Enabled = manualMode;
        }

        private void btnAlgBrowseMaskDir_Click(object sender, EventArgs e)
        {
            SafeShowWarning("手动模式Mask目录已改为自动加载：\n会从输入图像目录下各数字目录的同级 mask 文件夹读取。");
        }

        private void btnAlgLoadMaskList_Click(object sender, EventArgs e)
        {
            listBoxAlgMasks.Items.Clear();
            var allImages = GetAllLoadedImages();
            if (allImages.Count == 0)
            {
                SafeShowWarning("请先加载输入图像目录");
                return;
            }

            txtAlgMaskDir.Text = "自动加载：数字目录同级掩膜文件夹";
            var loadedMaskFiles = new List<string>();
            var missingMaskBatches = new HashSet<string>();

            foreach (var imagePath in allImages)
            {
                string batchName = GetBatchNameFromImagePath(imagePath);
                var outputPaths = GetOutputImagePathsBySource(imagePath);
                string maskDir = Path.GetDirectoryName(outputPaths.maskPath);
                if (string.IsNullOrWhiteSpace(maskDir) || !Directory.Exists(maskDir))
                {
                    missingMaskBatches.Add(batchName);
                    continue;
                }

                string sampleNameNoExt = Path.GetFileNameWithoutExtension(imagePath);
                string matchedMask = Directory.EnumerateFiles(maskDir, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => AlgImageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .FirstOrDefault(file =>
                        string.Equals(Path.GetFileNameWithoutExtension(file), sampleNameNoExt, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrWhiteSpace(matchedMask) || !File.Exists(matchedMask))
                {
                    missingMaskBatches.Add(batchName);
                    continue;
                }

                loadedMaskFiles.Add(matchedMask);
            }

            foreach (var file in loadedMaskFiles.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
            {
                listBoxAlgMasks.Items.Add(file);
            }

            SafeAppendLog($"已自动加载Mask文件数量: {listBoxAlgMasks.Items.Count}");
            if (missingMaskBatches.Count > 0)
            {
                string batchList = string.Join("、", missingMaskBatches.OrderBy(x => x));
                SafeShowWarning($"以下目录未放置完整对应的Mask图：{batchList}");
            }
        }

        private async void btnAlgRun_Click(object sender, EventArgs e)
        {
            // 步骤三固定按已加载的全部目录(1~10)执行，不受当前树选中节点限制
            var runImages = GetAllLoadedImages();
            if (!ValidateAlgorithmRunInputs(runImages))
            {
                return;
            }

            if (AreAllAlgorithmOutputsReady(runImages))
            {
                var confirmOverwrite = MessageBox.Show(
                    "检测到当前目录下所有样品的分析结果已存在。\n是否重新分析并覆盖已有结果？",
                    "结果已存在",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirmOverwrite != DialogResult.Yes)
                {
                    SafeAppendLog("用户取消重新分析，保留已有结果。");
                    return;
                }
            }

            btnAlgRun.Enabled = false;
            btnAlgRun.Text = "处理中...";
            Cursor = Cursors.WaitCursor;

            AlgorithmBatchResponse batchResponse;
            try
            {
                batchResponse = await Task.Run(() => _algorithmBatchService.Execute(new AlgorithmBatchRequest
                {
                    RunImages = runImages,
                    ManualMode = radioAlgManualMode.Checked,
                    ManualMasks = listBoxAlgMasks.Items.Cast<string>().ToList(),
                    GetBatchName = GetBatchNameFromImagePath,
                    GetOutputPaths = GetOutputImagePathsBySource,
                    TryPreprocess = imagePath => TryPreprocessAndSaveOutputs(imagePath, updatePreview: false, out _),
                    GetCoverageFromManualMask = (croppedPath, matchedMaskPath, targetMaskPath, targetOutputPath) =>
                    {
                        double coverage = GenerateCoverageFromManualMask(
                            croppedPath,
                            matchedMaskPath,
                            targetMaskPath,
                            targetOutputPath,
                            out string maskCoveragePath);
                        return (coverage, maskCoveragePath);
                    },
                    GetCoverageAutoMask = (croppedPath, targetMaskPath, targetOutputPath) =>
                    {
                        double coverage = CoverageAnalyzer.detectImage(croppedPath, targetMaskPath, targetOutputPath);
                        return (coverage, targetMaskPath);
                    },
                    CalculateUniformity = (croppedPath, maskCoveragePath) =>
                        ImageUniformityCalculator.CalculateUniformity(
                            croppedPath,
                            maskCoveragePath,
                            gaussianKsize: 101,
                            gaussianSigma: 0.0,
                            applyIlluminationCorrection: false),
                    GenerateHeatmap = (croppedPath, maskCoveragePath, heatmapPath, invertMask) =>
                        TryGenerateUniformityHeatmapByPython(croppedPath, maskCoveragePath, heatmapPath, invertMask)
                }));
            }
            catch (Exception ex)
            {
                SafeShowWarning($"算法处理失败：{ex.Message}");
                return;
            }
            finally
            {
                btnAlgRun.Enabled = true;
                btnAlgRun.Text = "开始处理并计算";
                Cursor = Cursors.Default;
            }

            var batchResults = batchResponse.BatchResults;

            if (radioAlgAutoMode.Checked)
            {
                var selectedResult = batchResults.FirstOrDefault(r =>
                    string.Equals(r.SampleName, Path.GetFileName(_algSelectedImagePath), StringComparison.OrdinalIgnoreCase));
                if (selectedResult != null && !string.IsNullOrWhiteSpace(selectedResult.AlgorithmImagePath) && File.Exists(selectedResult.AlgorithmImagePath))
                {
                    LoadImageToPictureBox(picAlgPreprocessed, selectedResult.AlgorithmImagePath);
                    labelAlgPreprocessed.Text = "算法处理图（当前样品）";
                }
            }

            _algDirectoryAverages = batchResponse.DirectoryAverages;
            UpdateAlgorithmSummaryText();

            ShowBatchResultDialog(batchResults);
            LogAlgorithmBatchSummary(batchResults);
            TryPromptInsertAlgorithmResults(runImages, batchResults);
        }

        private bool AreAllAlgorithmOutputsReady(List<string> runImages)
        {
            if (runImages == null || runImages.Count == 0)
            {
                return false;
            }

            foreach (var imagePath in runImages)
            {
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    return false;
                }

                var paths = GetOutputImagePathsBySource(imagePath);
                if (!File.Exists(paths.binaryPath) ||
                    !File.Exists(paths.standardPath) ||
                    !File.Exists(paths.croppedPath) ||
                    !File.Exists(paths.outputPath) ||
                    !File.Exists(paths.maskPath) ||
                    !File.Exists(paths.heatmapPath))
                {
                    return false;
                }
            }

            return true;
        }

        private void TryPromptInsertAlgorithmResults(List<string> runImages, List<AlgorithmBatchResult> batchResults)
        {
            if (!TryPromptBatchAndSampleType(out var batchNo, out var sampleType))
            {
                SafeAppendLog("已取消算法结果入库。");
                return;
            }

            var candidates = BuildAlgorithmInsertCandidates(runImages, batchResults, batchNo, sampleType);
            if (candidates.Count == 0)
            {
                SafeAppendLog("算法结果未入库：未匹配到可写入数据库的有效计算结果（请检查样品命名与计算结果映射）。");
                return;
            }

            var confirm = MessageBox.Show(
                $"本次共生成 {candidates.Count} 条可入库记录，是否立即插入数据库？",
                "算法结果入库确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) { SafeAppendLog("已取消算法结果入库。"); return; }

            var existingSampleIds = new HashSet<string>(
                SampleDBHelper.GetAllSamples().Select(x => x.SampleId),
                StringComparer.OrdinalIgnoreCase);
            var duplicateCandidates = candidates
                .Where(x => existingSampleIds.Contains(x.SampleId))
                .Select(x => x.SampleId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var candidatesToInsert = candidates;
            if (duplicateCandidates.Count > 0)
            {
                string duplicatePreview = string.Join("、", duplicateCandidates.Take(8));
                if (duplicateCandidates.Count > 8)
                {
                    duplicatePreview += $" 等{duplicateCandidates.Count}个";
                }

                var duplicateDecision = MessageBox.Show(
                    $"检测到重复样品ID：{duplicatePreview}\n是否覆盖数据库中已有记录？\n选择“否”将跳过重复ID，仅插入新记录。",
                    "重复样品ID确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (duplicateDecision != DialogResult.Yes)
                {
                    candidatesToInsert = candidates
                        .Where(x => !existingSampleIds.Contains(x.SampleId))
                        .ToList();
                    SafeAppendLog($"检测到重复ID {duplicateCandidates.Count} 条，按用户选择已跳过重复记录。");
                }
            }

            if (candidatesToInsert.Count == 0)
            {
                SafeAppendLog("没有可插入的新记录（重复记录已全部跳过）。");
                return;
            }

            int successCount = 0;
            foreach (var sample in candidatesToInsert)
            {
                try
                {
                    SampleDBHelper.UpsertSample(sample);
                    successCount++;
                }
                catch (Exception ex)
                {
                    SafeAppendLog($"入库失败：{sample.SampleId}，原因：{ex.Message}");
                }
            }

            SafeAppendLog($"算法结果入库完成：成功 {successCount} / {candidatesToInsert.Count}");
            LoadAllDatabaseRows();
        }

        private List<SampleDBHelper.SampleData> BuildAlgorithmInsertCandidates(
            List<string> runImages,
            List<AlgorithmBatchResult> batchResults,
            int batchNo,
            string sampleType)
        {
            var resultMap = batchResults
                .Where(r => !string.IsNullOrWhiteSpace(r.SampleName))
                .GroupBy(r => $"{r.BatchName}|{r.SampleName}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var resultBySampleName = batchResults
                .Where(r => !string.IsNullOrWhiteSpace(r.SampleName))
                .GroupBy(r => Path.GetFileNameWithoutExtension(r.SampleName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var candidates = new List<SampleDBHelper.SampleData>();
            foreach (var imagePath in runImages)
            {
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    continue;
                }

                var batchName = GetBatchNameFromImagePath(imagePath);
                var sampleName = Path.GetFileName(imagePath);
                var mapKey = $"{batchName}|{sampleName}";
                if (!resultMap.TryGetValue(mapKey, out var result))
                {
                    // 兼容：当批次名或扩展名匹配不上时，退化为按样品名匹配。
                    var sampleId = Path.GetFileNameWithoutExtension(imagePath);
                    if (!resultBySampleName.TryGetValue(sampleId, out result))
                    {
                        continue;
                    }
                }

                // 仅在覆盖率和均匀性都有效时允许入库。
                if (!result.Coverage.HasValue || !result.Uniformity.HasValue || double.IsNaN(result.Uniformity.Value))
                {
                    continue;
                }

                var outputPaths = GetOutputImagePathsBySource(imagePath);
                int iterationNo = 0;
                int.TryParse(batchName, out iterationNo);
                var dbSampleId = Path.GetFileNameWithoutExtension(imagePath);
                var now = DateTime.Now;
                candidates.Add(new SampleDBHelper.SampleData
                {
                    SampleId = dbSampleId,
                    SampleType = sampleType,
                    IterationNo = iterationNo,
                    BatchNo = batchNo,
                    Coverage = result.Coverage.Value,
                    Uniformity = result.Uniformity.Value,
                    CreatedAt = now,
                    UpdatedAt = now,
                    OriImagePath = imagePath,
                    CroppedImagePath = File.Exists(outputPaths.croppedPath) ? outputPaths.croppedPath : string.Empty,
                    HeatmapImagePath = File.Exists(outputPaths.heatmapPath) ? outputPaths.heatmapPath : string.Empty,
                    MaskImagePath = File.Exists(outputPaths.maskPath) ? outputPaths.maskPath : string.Empty,
                    OutputImagePath = File.Exists(outputPaths.outputPath) ? outputPaths.outputPath : string.Empty,
                    StandardImagePath = File.Exists(outputPaths.standardPath) ? outputPaths.standardPath : string.Empty
                });
            }

            return candidates;
        }

        private bool TryPromptBatchAndSampleType(out int batchNo, out string sampleType)
        {
            int selectedBatchNo = 0;
            string selectedSampleType = string.Empty;

            using (var dialog = new Form())
            {
                dialog.Text = "入库参数确认";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new System.Drawing.Size(420, 180);

                var lblBatch = new Label { Text = "第几批次（BatchNo）", Left = 20, Top = 22, Width = 180 };
                var txtBatch = new TextBox { Left = 210, Top = 18, Width = 160, Text = "1" };
                var lblType = new Label { Text = "样品类型（SampleType）", Left = 20, Top = 62, Width = 180 };
                var cmbType = new ComboBox
                {
                    Left = 210,
                    Top = 58,
                    Width = 160,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbType.Items.AddRange(new object[] { "Organic", "Oxide" });
                cmbType.SelectedIndex = 0;

                var btnOk = new Button { Text = "确定", Left = 210, Top = 110, Width = 75 };
                var btnCancel = new Button { Text = "取消", Left = 295, Top = 110, Width = 75 };

                btnOk.Click += (_, __) =>
                {
                    if (!int.TryParse(txtBatch.Text.Trim(), out var parsedBatch) || parsedBatch <= 0)
                    {
                        SafeShowWarning("批次号必须为大于0的整数。");
                        return;
                    }

                    selectedBatchNo = parsedBatch;
                    selectedSampleType = cmbType.SelectedItem?.ToString() ?? "Organic";
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                btnCancel.Click += (_, __) =>
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                };

                dialog.Controls.Add(lblBatch);
                dialog.Controls.Add(txtBatch);
                dialog.Controls.Add(lblType);
                dialog.Controls.Add(cmbType);
                dialog.Controls.Add(btnOk);
                dialog.Controls.Add(btnCancel);

                var ok = dialog.ShowDialog(this) == DialogResult.OK;
                batchNo = ok ? selectedBatchNo : 0;
                sampleType = ok ? selectedSampleType : string.Empty;
                return ok;
            }
        }

        private bool ValidateAlgorithmRunInputs(List<string> runImages)
        {
            if (runImages == null || runImages.Count == 0)
            {
                SafeShowWarning("当前没有可处理图像，请先加载目录中的样品图像");
                return false;
            }

            if (radioAlgManualMode.Checked && listBoxAlgMasks.Items.Count == 0)
            {
                SafeShowWarning("手动模式下请先加载Mask列表");
                return false;
            }

            return true;
        }

        private void UpdateAlgorithmSummaryText()
        {
            if (_algDirectoryAverages.Count == 1)
            {
                txtAlgCoverageResult.Text = _algDirectoryAverages[0].CoverageAvg.ToString("F6");
                txtAlgUniformityResult.Text = _algDirectoryAverages[0].UniformityAvg.ToString("F6");
                return;
            }

            if (_algDirectoryAverages.Count > 1)
            {
                string multiDirText = $"共{_algDirectoryAverages.Count}个目录，点击“查看目录均值”";
                txtAlgCoverageResult.Text = multiDirText;
                txtAlgUniformityResult.Text = multiDirText;
                return;
            }

            txtAlgCoverageResult.Text = string.Empty;
            txtAlgUniformityResult.Text = string.Empty;
        }

        private void LogAlgorithmBatchSummary(List<AlgorithmBatchResult> batchResults)
        {
            int successRowsCount = batchResults.Count(r => r.Coverage.HasValue
                                                           && r.Uniformity.HasValue
                                                           && !double.IsNaN(r.Uniformity.Value)
                                                           && r.Coverage.Value > 0);
            SafeAppendLog($"算法页签批量计算完成：总数={batchResults.Count}, 成功={successRowsCount}, 失败={batchResults.Count - successRowsCount}");
            foreach (var avg in _algDirectoryAverages)
            {
                SafeAppendLog($"目录{avg.BatchName} 平均覆盖率={avg.CoverageAvg:F6}, 平均均匀性={avg.UniformityAvg:F6}");
            }
        }

        private void btnAlgShowDirAverages_Click(object sender, EventArgs e)
        {
            if (_algDirectoryAverages == null || _algDirectoryAverages.Count == 0)
            {
                SafeShowWarning("暂无目录均值数据，请先执行“开始处理并计算”");
                return;
            }

            ShowDirectoryAverageDialog(_algDirectoryAverages);
        }

        private string GetBatchNameFromImagePath(string imagePath)
        {
            try
            {
                string oriFolder = Path.GetDirectoryName(imagePath);
                string batchFolder = Directory.GetParent(oriFolder)?.FullName;
                return string.IsNullOrWhiteSpace(batchFolder) ? "未知目录" : Path.GetFileName(batchFolder);
            }
            catch
            {
                return "未知目录";
            }
        }

        private List<string> GetBatchImagesFromCurrentSelection()
        {
            var selectedNode = treeAlgSamples.SelectedNode;
            if (selectedNode == null)
            {
                return new List<string>();
            }

            TreeNode batchNode = selectedNode;
            while (batchNode.Parent != null)
            {
                batchNode = batchNode.Parent;
            }

            var imagePaths = new List<string>();
            foreach (TreeNode child in batchNode.Nodes)
            {
                if (child.Tag is string path && File.Exists(path))
                {
                    imagePaths.Add(path);
                }
            }

            return imagePaths
                .OrderBy(p => Path.GetFileNameWithoutExtension(p))
                .ToList();
        }

        private List<string> GetAllLoadedImages()
        {
            var all = new List<string>();
            foreach (TreeNode batchNode in treeAlgSamples.Nodes)
            {
                foreach (TreeNode imageNode in batchNode.Nodes)
                {
                    if (imageNode.Tag is string path && File.Exists(path))
                    {
                        all.Add(path);
                    }
                }
            }

            return all.OrderBy(p => p).ToList();
        }

        private (string binaryPath, string standardPath, string croppedPath, string outputPath, string maskPath, string heatmapPath) GetOutputImagePathsBySource(string sourceImagePath)
        {
            string fileName = Path.GetFileName(sourceImagePath);
            string oriFolder = Path.GetDirectoryName(sourceImagePath);
            string parentFolder = Directory.GetParent(oriFolder)?.FullName ?? oriFolder;
            return (
                Path.Combine(parentFolder, "binary", fileName),
                Path.Combine(parentFolder, "standard", fileName),
                Path.Combine(parentFolder, "cropped", fileName),
                Path.Combine(parentFolder, "output", fileName),
                Path.Combine(parentFolder, "mask", fileName),
                Path.Combine(parentFolder, "heatmap", Path.GetFileNameWithoutExtension(fileName) + "_heatmap.png")
            );
        }

        private void TryGenerateUniformityHeatmapByPython(string imagePath, string maskPath, string heatmapPath, bool invertMask)
        {
            try
            {
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "python", "uniformity_heatmap.py");
                if (!File.Exists(scriptPath))
                {
                    SafeAppendLog($"热力图脚本不存在: {scriptPath}");
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(heatmapPath));
                string heatmapMaskPath = maskPath;
                if (invertMask)
                {
                    string invertedMaskPath = CreateInvertedMaskForHeatmap(maskPath, heatmapPath);
                    if (string.IsNullOrWhiteSpace(invertedMaskPath) || !File.Exists(invertedMaskPath))
                    {
                        SafeAppendLog($"热力图生成失败：mask取反失败，原始mask={maskPath}");
                        return;
                    }

                    heatmapMaskPath = invertedMaskPath;
                }

                string args = $"\"{scriptPath}\" --image \"{imagePath}\" --mask \"{heatmapMaskPath}\" --output \"{heatmapPath}\"";
                if (RunPythonProcess("python", args))
                {
                    SafeAppendLog($"均匀性热力图已生成: {heatmapPath}");
                    return;
                }

                // Windows兜底：尝试 py -3
                if (RunPythonProcess("py", $"-3 {args}"))
                {
                    SafeAppendLog($"均匀性热力图已生成: {heatmapPath}");
                    return;
                }

                SafeAppendLog("热力图生成失败：python/py 调用均未成功");
            }
            catch (Exception ex)
            {
                SafeAppendLog($"热力图生成异常: {ex.Message}");
            }
        }

        private string CreateInvertedMaskForHeatmap(string maskPath, string heatmapPath)
        {
            if (string.IsNullOrWhiteSpace(maskPath) || !File.Exists(maskPath))
            {
                return string.Empty;
            }

            try
            {
                string heatmapDir = Path.GetDirectoryName(heatmapPath);
                Directory.CreateDirectory(heatmapDir);
                string invertedMaskPath = Path.Combine(
                    heatmapDir,
                    Path.GetFileNameWithoutExtension(maskPath) + "_inverted_for_heatmap.png");

                using (var mask = Cv2.ImRead(maskPath, ImreadModes.Color))
                using (var invertedMask = new Mat())
                {
                    if (mask.Empty())
                    {
                        return string.Empty;
                    }

                    // 取反规则：黑色 -> 白色，其他颜色 -> 黑色
                    Cv2.InRange(mask, new Scalar(0, 0, 0), new Scalar(0, 0, 0), invertedMask);
                    Cv2.ImWrite(invertedMaskPath, invertedMask);
                }

                return invertedMaskPath;
            }
            catch (Exception ex)
            {
                SafeAppendLog($"热力图mask取反失败: {ex.Message}");
                return string.Empty;
            }
        }

        private bool RunPythonProcess(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    if (p == null)
                    {
                        return false;
                    }

                    string stdOut = p.StandardOutput.ReadToEnd();
                    string stdErr = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        SafeAppendLog($"{fileName} 运行失败: {stdErr}");
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(stdOut))
                    {
                        SafeAppendLog(stdOut.Trim());
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private double GenerateCoverageFromManualMask(string croppedPath, string manualMaskPath, string targetMaskPath, string targetOutputPath, out string savedMaskPath)
        {
            savedMaskPath = string.Empty;
            using (var cropped = Cv2.ImRead(croppedPath, ImreadModes.Color))
            using (var mask = Cv2.ImRead(manualMaskPath, ImreadModes.Color))
            {
                if (cropped.Empty() || mask.Empty())
                {
                    throw new Exception("手动模式图像或mask读取失败");
                }

                if (cropped.Size() != mask.Size())
                {
                    Cv2.Resize(mask, mask, cropped.Size(), 0, 0, InterpolationFlags.Nearest);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(targetMaskPath));
                Directory.CreateDirectory(Path.GetDirectoryName(targetOutputPath));

                // 手动模式统一保存为png，避免同名样品出现jpg/png两份mask
                string maskDir = Path.GetDirectoryName(targetMaskPath);
                string maskNameNoExt = Path.GetFileNameWithoutExtension(targetMaskPath);
                savedMaskPath = Path.Combine(maskDir, maskNameNoExt + ".png");
                CleanupDuplicateManualMasks(maskDir, maskNameNoExt, savedMaskPath);
                Cv2.ImWrite(savedMaskPath, mask);

                // 基于原图+mask叠加生成output可视化图
                // 手动模式下，blend使用取反后的mask：
                // 黑色(0,0,0)->白色(255,255,255)，其他颜色->黑色(0,0,0)
                using (var blackMask = new Mat())
                using (var invertedForBlend = new Mat())
                using (var blended = new Mat())
                {
                    Cv2.InRange(mask, new Scalar(0, 0, 0), new Scalar(0, 0, 0), blackMask);
                    Cv2.CvtColor(blackMask, invertedForBlend, ColorConversionCodes.GRAY2BGR);
                    Cv2.AddWeighted(cropped, 0.6, invertedForBlend, 0.4, 0.0, blended);
                    Cv2.ImWrite(targetOutputPath, blended);
                }

                return CoverageAnalyzer.getRatio(mask);
            }
        }

        private void CleanupDuplicateManualMasks(string maskDir, string maskNameNoExt, string keepPath)
        {
            string[] candidates = { ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff" };
            foreach (var ext in candidates)
            {
                string path = Path.Combine(maskDir, maskNameNoExt + ext);
                if (!string.Equals(path, keepPath, StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private void ShowBatchResultDialog(List<AlgorithmBatchResult> batchResults)
        {
            var dialog = new Form
            {
                Text = "批量处理结果",
                Width = 760,
                Height = 520,
                StartPosition = FormStartPosition.CenterParent
            };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "目录", DataPropertyName = "BatchName", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "样品", DataPropertyName = "SampleName", Width = 160 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "覆盖率", DataPropertyName = "CoverageText", Width = 140 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "均匀性", DataPropertyName = "UniformityText", Width = 140 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "状态", DataPropertyName = "Status", Width = 180 });

            var rows = batchResults
                .OrderBy(r => GetBatchSortKey(r.BatchName))
                .ThenBy(r => r.BatchName)
                .ThenBy(r => r.SampleName)
                .Select(r => new
                {
                    r.BatchName,
                    r.SampleName,
                    CoverageText = r.Coverage.HasValue ? r.Coverage.Value.ToString("F6") : "-",
                    UniformityText = r.Uniformity.HasValue
                        ? (double.IsNaN(r.Uniformity.Value) ? "NaN" : r.Uniformity.Value.ToString("F6"))
                        : "-",
                    r.Status
                }).ToList();

            grid.DataSource = rows;
            dialog.Controls.Add(grid);
            dialog.ShowDialog(this);
        }

        private void ShowDirectoryAverageDialog(List<AlgorithmDirectoryAverage> directoryAverages)
        {
            var dialog = new Form
            {
                Text = "目录均值结果",
                Width = 560,
                Height = 420,
                StartPosition = FormStartPosition.CenterParent
            };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "目录", DataPropertyName = "BatchName", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "样品数", DataPropertyName = "SampleCount", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "平均覆盖率", DataPropertyName = "CoverageAvgText", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "平均均匀性", DataPropertyName = "UniformityAvgText", Width = 150 });

            var rows = directoryAverages
                .OrderBy(x => GetBatchSortKey(x.BatchName))
                .ThenBy(x => x.BatchName)
                .Select(x => new
            {
                x.BatchName,
                x.SampleCount,
                CoverageAvgText = x.CoverageAvg.ToString("F6"),
                UniformityAvgText = x.UniformityAvg.ToString("F6")
            }).ToList();

            grid.DataSource = rows;
            dialog.Controls.Add(grid);
            dialog.ShowDialog(this);
        }

        private static int GetBatchSortKey(string batchName)
        {
            if (int.TryParse(batchName, out var n))
            {
                return n;
            }

            var digits = new string((batchName ?? string.Empty).Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out n))
            {
                return n;
            }

            return int.MaxValue;
        }

        private void LoadImageToPictureBox(PictureBox pictureBox, string imagePath)
        {
            _imagePreviewService.LoadImage(pictureBox, imagePath);
        }

        private void ClearPictureBoxImage(PictureBox pictureBox)
        {
            _imagePreviewService.ClearImage(pictureBox);
        }

        private void ClearAlgorithmProcessingState()
        {
            _algSelectedImagePath = string.Empty;
            _algPreprocessedImagePath = string.Empty;
            _algBinaryImagePath = string.Empty;
            _algStandardImagePath = string.Empty;
            _algCroppedImagePath = string.Empty;
            _algOutputImagePath = string.Empty;
            _algHeatmapImagePath = string.Empty;
            _algDirectoryAverages = new List<AlgorithmDirectoryAverage>();
            labelAlgSelectedSample.Text = "当前样品：未选择任何样品";
            labelAlgPreprocessed.Text = "预处理后（当前样品）";
            UpdateAlgPreviewButtonState(btnAlgPreviewOriginal);
            txtAlgCoverageResult.Text = "";
            txtAlgUniformityResult.Text = "";
            listBoxAlgMasks.Items.Clear();
            ClearPictureBoxImage(picAlgOriginal);
            ClearPictureBoxImage(picAlgPreprocessed);
        }

        private (string x0, string y0, string x1, string y1) GetCurrentScanCoordinates()
        {
            if (radioButton2.Checked)
            {
                return (
                    AutoMetalConstants.test_leftTop_x,
                    AutoMetalConstants.test_leftTop_y,
                    AutoMetalConstants.test_rightBottom_x,
                    AutoMetalConstants.test_rightBottom_y
                );
            }

            return (
                AutoMetalConstants.normal_leftTop_x,
                AutoMetalConstants.normal_leftTop_y,
                AutoMetalConstants.normal_rightBottom_x,
                AutoMetalConstants.normal_rightBottom_y
            );
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
                    //else if (message.StartsWith("place:", StringComparison.OrdinalIgnoreCase))
                    else if (message.StartsWith("place", StringComparison.OrdinalIgnoreCase))
                    {
                        // 放置状态完成后 显微镜处于忙碌状态
                        is_free = 0;

                        //message = message.Substring(6);

                        // 安全更新UI（跨线程调用）
                        SafeAppendLog($"来自 {remoteEP}: {message}");

                        // 实验ID 样品序号
                        //var (expId, sampleId) = AnalysisUtils.ParseExperimentSampleId(message);
                        var expId = "2";
                        // 根据目标文件夹中现有的 1.jpg/2.jpg/... 自动顺序递增，忽略不符合格式的文件
                        string dateString = DateTime.Now.ToString("yyyyMMdd");
                        string folderPath = Path.Combine(AutoMetalConstants.autoFolderPath + "\\" + $"{dateString}", $"{expId}");
                        var sampleId = GetNextNumericJpgSampleId(folderPath);
                        SafeAppendLog($"开始进行检测: 实验ID: {expId}, 样品ID: {sampleId}");

                        // 更新表面组件
                        SafeUpdateTextBox(expID, expId);
                        SafeUpdateTextBox(sampleID, sampleId);

                        // 根据运行模式（正常/测试）加载对应扫描坐标
                        var scanCoords = GetCurrentScanCoordinates();
                        SafeUpdateTextBox(textX0, scanCoords.x0);
                        SafeUpdateTextBox(textY0, scanCoords.y0);
                        SafeUpdateTextBox(textX1, scanCoords.x1);
                        SafeUpdateTextBox(textY1, scanCoords.y1);

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
                _logWriter.Append(listBoxInfo, message);
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

        private void AddColumn(string propertyName, string headerText)
        {
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName, // 绑定到属性名
                HeaderText = headerText,         // 列标题
                SortMode = DataGridViewColumnSortMode.Automatic
            });
        }

        private void BindDatabaseGrid(List<SampleDBHelper.SampleData> samples)
        {
            _bindingList = new BindingList<SampleDBHelper.SampleData>(samples ?? new List<SampleDBHelper.SampleData>());
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _bindingList;
            dataGridView1.Columns.Clear();

            AddColumn("SampleId", "样本ID");
            AddColumn("SampleType", "样本类型");
            AddColumn("IterationNo", "迭代号");
            AddColumn("BatchNo", "批次号");
            AddColumn("Coverage", "覆盖率");
            AddColumn("Uniformity", "均匀度");
            AddColumn("CreatedAt", "创建时间");
            AddColumn("OriImagePath", "原始图像路径(ori)");
            AddColumn("CroppedImagePath", "裁剪图像路径(cropped)");
            AddColumn("HeatmapImagePath", "热力图图像路径");
            AddColumn("MaskImagePath", "mask图像路径");
            AddColumn("OutputImagePath", "output图像路径");
            AddColumn("StandardImagePath", "standard图像路径");
            AddColumn("UpdatedAt", "更新时间");
        }

        private void LoadAllDatabaseRows()
        {
            BindDatabaseGrid(SampleDBHelper.GetAllSamples());
        }

        private void btn_getAllData_Click(object sender, EventArgs e)
        {
            LoadAllDatabaseRows();
        }

        private void btnSearchByDate_Click_1(object sender, EventArgs e)
        {
            DateTime selectedDate = dateTimePicker1.Value.Date; // 获取选择的日期（忽略时间部分）
            BindDatabaseGrid(SampleDBHelper.GetSamplesByDate(selectedDate));
        }

        private void btnSearchCoverage_Click(object sender, EventArgs e)
        {
            double coverage;
            double uniformity;
            bool hasCoverage = double.TryParse(textCoverage.Text, out coverage);
            bool hasUniformity = double.TryParse(textUniformity.Text, out uniformity);

            if (!hasCoverage && !hasUniformity)
            {
                SafeShowWarning("请输入有效的覆盖率或均匀度值。");
                return;
            }

            BindDatabaseGrid(SampleDBHelper.GetSamplesByCoverageAndUniformity(
                hasCoverage ? coverage : (double?)null,
                hasUniformity ? uniformity : (double?)null));
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
            {
                return;
            }

            var rowItem = dataGridView1.Rows[e.RowIndex].DataBoundItem as SampleDBHelper.SampleData;
            if (rowItem == null)
            {
                return;
            }

            ShowSampleEditDialog(rowItem);
        }

        private void ShowSampleEditDialog(SampleDBHelper.SampleData source)
        {
            var editable = new SampleDBHelper.SampleData
            {
                SampleId = source.SampleId,
                SampleType = source.SampleType,
                IterationNo = source.IterationNo,
                BatchNo = source.BatchNo,
                Coverage = source.Coverage,
                Uniformity = source.Uniformity,
                OriImagePath = source.OriImagePath,
                CroppedImagePath = source.CroppedImagePath,
                HeatmapImagePath = source.HeatmapImagePath,
                MaskImagePath = source.MaskImagePath,
                OutputImagePath = source.OutputImagePath,
                StandardImagePath = source.StandardImagePath,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt
            };

            using (var dialog = new Form())
            {
                dialog.Text = "编辑样本记录";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new System.Drawing.Size(760, 380);

                var panel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 13,
                    AutoScroll = true,
                    Padding = new Padding(12)
                };
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                TextBox AddField(string label, string value)
                {
                    panel.Controls.Add(new Label
                    {
                        Text = label,
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Anchor = AnchorStyles.Left
                    });
                    var tb = new TextBox
                    {
                        Text = value ?? string.Empty,
                        Dock = DockStyle.Fill
                    };
                    panel.Controls.Add(tb);
                    return tb;
                }

                var txtSampleId = AddField("样本ID", editable.SampleId);
                var txtSampleType = AddField("样本类型", editable.SampleType);
                var txtIterationNo = AddField("迭代号", editable.IterationNo.ToString());
                var txtBatchNo = AddField("批次号", editable.BatchNo.ToString());
                var txtCoverage = AddField("覆盖率", editable.Coverage.ToString("F6"));
                var txtUniformity = AddField("均匀度", editable.Uniformity.ToString("F6"));
                var txtOriginalPath = AddField("原始图像路径(ori)", editable.OriImagePath);
                var txtCroppedPath = AddField("裁剪图像路径", editable.CroppedImagePath);
                var txtUniImgPath = AddField("热力图图像路径", editable.HeatmapImagePath);
                var txtCoverageImgPath = AddField("mask图像路径", editable.MaskImagePath);
                var txtOutputImgPath = AddField("output图像路径", editable.OutputImagePath);
                var txtStandardImgPath = AddField("standard图像路径", editable.StandardImagePath);

                var txtCreatedAt = AddField("创建时间", editable.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                txtCreatedAt.ReadOnly = true;
                var txtUpdatedAt = AddField("更新时间", editable.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                txtUpdatedAt.ReadOnly = true;

                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 48,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(12, 8, 12, 8)
                };

                var btnSave = new Button { Text = "保存修改", Width = 100, Height = 28 };
                var btnDelete = new Button { Text = "删除记录", Width = 100, Height = 28 };
                var btnCancel = new Button { Text = "取消", Width = 80, Height = 28 };

                btnCancel.Click += (_, __) => dialog.Close();
                btnDelete.Click += (_, __) =>
                {
                    var result = MessageBox.Show(
                        $"确认删除样本 {source.SampleId} 吗？该操作不可撤销。",
                        "确认删除",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    if (SampleDBHelper.DeleteSample(source.SampleId))
                    {
                        SafeAppendLog($"已删除样本: {source.SampleId}");
                        LoadAllDatabaseRows();
                        dialog.Close();
                    }
                    else
                    {
                        SafeShowWarning("删除失败，记录可能不存在。");
                    }
                };

                btnSave.Click += (_, __) =>
                {
                    var newSampleId = txtSampleId.Text.Trim();
                    if (string.IsNullOrWhiteSpace(newSampleId))
                    {
                        SafeShowWarning("样本ID不能为空。");
                        return;
                    }

                    if (!double.TryParse(txtCoverage.Text.Trim(), out var newCoverage))
                    {
                        SafeShowWarning("覆盖率必须为数字。");
                        return;
                    }

                    if (!double.TryParse(txtUniformity.Text.Trim(), out var newUniformity))
                    {
                        SafeShowWarning("均匀度必须为数字。");
                        return;
                    }

                    if (!int.TryParse(txtIterationNo.Text.Trim(), out var newIterationNo))
                    {
                        SafeShowWarning("迭代号必须为整数。");
                        return;
                    }
                    if (!int.TryParse(txtBatchNo.Text.Trim(), out var newBatchNo))
                    {
                        SafeShowWarning("批次号必须为整数。");
                        return;
                    }

                    if (!string.Equals(newSampleId, source.SampleId, StringComparison.Ordinal))
                    {
                        var existing = SampleDBHelper.GetSampleById(newSampleId);
                        if (existing != null)
                        {
                            SafeShowWarning("新的样本ID已存在，请更换。");
                            return;
                        }
                    }

                    editable.SampleId = newSampleId;
                    editable.SampleType = txtSampleType.Text.Trim();
                    editable.IterationNo = newIterationNo;
                    editable.BatchNo = newBatchNo;
                    editable.Coverage = newCoverage;
                    editable.Uniformity = newUniformity;
                    editable.OriImagePath = txtOriginalPath.Text.Trim();
                    editable.CroppedImagePath = txtCroppedPath.Text.Trim();
                    editable.HeatmapImagePath = txtUniImgPath.Text.Trim();
                    editable.MaskImagePath = txtCoverageImgPath.Text.Trim();
                    editable.OutputImagePath = txtOutputImgPath.Text.Trim();
                    editable.StandardImagePath = txtStandardImgPath.Text.Trim();
                    editable.UpdatedAt = DateTime.Now;

                    if (!string.Equals(source.SampleId, editable.SampleId, StringComparison.Ordinal))
                    {
                        SampleDBHelper.DeleteSample(source.SampleId);
                    }
                    SampleDBHelper.UpsertSample(editable);

                    SafeAppendLog($"样本已更新: {source.SampleId} -> {editable.SampleId}");
                    LoadAllDatabaseRows();
                    dialog.Close();
                };

                buttonPanel.Controls.Add(btnSave);
                buttonPanel.Controls.Add(btnDelete);
                buttonPanel.Controls.Add(btnCancel);

                dialog.Controls.Add(panel);
                dialog.Controls.Add(buttonPanel);
                dialog.ShowDialog(this);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSampleSearch_Click(object sender, EventArgs e)
        {
            var sampleID = dbSampleText.Text;

            if (sampleID == "")
            {
                SafeShowWarning("请输入样品ID");
            }
            else
            {
                var sampleData = SampleDBHelper.GetSampleById(sampleID);

                if (sampleData == null)
                {
                    SafeShowWarning("查询的样品不存在");
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
                            SafeShowWarning("文件不存在:" + sampleData.OriginalImagePath);
                        }

                    }

                    // 覆盖率
                    if (sampleData.CoverageAnalysisImagePath != "")
                    {
                        if (File.Exists(sampleData.CoverageAnalysisImagePath))
                        {
                            var image_3 = Image.FromFile(sampleData.CoverageAnalysisImagePath);
                            var abnormalPictureBox = this.Controls.Find("picBoxSampleAbnormal", true).FirstOrDefault() as PictureBox;
                            if (abnormalPictureBox != null)
                            {
                                PictureBoxHelper.EnableImageInteraction(abnormalPictureBox, image_3);
                                // 添加浮动按钮
                                PictureBoxHelper.AddInternalControls(abnormalPictureBox);
                            }
                        }
                        else
                        {
                            SafeShowWarning("文件不存在:" + sampleData.CoverageAnalysisImagePath);
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

                Console.WriteLine("batch sample render");
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
                SafeShowWarning($"计算SSIM时出错: {ex.Message}");
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
            string imageDir = @"C:\Users\SOW111\Desktop\paper";

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
            sb.AppendLine("ImageName\t检测编号\t均匀性\t覆盖率");

            foreach (var imagePath in imageFiles)
            {
                string imageName = Path.GetFileName(imagePath);

                // 裁剪：与主流程一致，先 Process 再保存裁剪图
                ImageProcessor.ProcessResult processedImage = ImageProcessor.ProcessImage(imagePath);
                string croppedImagePath = Path.Combine(Path.GetDirectoryName(imagePath),
                    Path.GetFileNameWithoutExtension(imagePath) + "_cropped.jpg");
                if (processedImage == null || processedImage.CroppedImage == null || processedImage.CroppedImage.Empty())
                {
                    SafeAppendLog($"功能测试: 图像 {imageName} 裁剪结果为空，跳过该图");
                    continue;
                }
                if (!TrySaveMatImage(croppedImagePath, processedImage.CroppedImage, $"功能测试保存裁剪图({imageName})"))
                {
                    continue;
                }

                // 计算 glassnumber（基于原图）
                string glassNumber = GlassNumberAnalyzer.GetGlassNumber(imagePath);

                // ===== 计算均匀性（基于裁剪图） =====
                double resValue = double.NaN;

                // ===== 计算覆盖率（基于裁剪图） =====
                double ratio = CoverageAnalyzer.detectImage(croppedImagePath);

                // 写入一行
                sb.AppendLine($"{imageName}\t{glassNumber}\t{resValue:F6}\t{ratio:F6}");
            }

            // 一次性写入 txt
            File.WriteAllText(saveTxtPath, sb.ToString(), Encoding.UTF8);

            SafeShowInfo("处理完成，结果已保存到 result.txt");
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }


}