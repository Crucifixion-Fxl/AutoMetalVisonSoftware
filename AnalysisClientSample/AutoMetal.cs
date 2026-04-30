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
using System.Diagnostics;

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
        private List<AlgDirectoryAverage> _algDirectoryAverages = new List<AlgDirectoryAverage>();
        private bool _isTrainingRunning = false;
        private CancellationTokenSource _trainingCancellationTokenSource;
        private Process _trainingProcess;
        private int _trainingTotalEpoch = 0;
        private readonly Regex _trainingEpochRegex = new Regex(@"Epoch\s+(\d+)\s*/\s*(\d+)", RegexOptions.IgnoreCase);
        private class AlgBatchResult
        {
            public string BatchName { get; set; }
            public string SampleName { get; set; }
            public double? Coverage { get; set; }
            public double? Uniformity { get; set; }
            public string Status { get; set; }
            public string AlgorithmImagePath { get; set; }
        }
        private class AlgDirectoryAverage
        {
            public string BatchName { get; set; }
            public double CoverageAvg { get; set; }
            public double UniformityAvg { get; set; }
            public int SampleCount { get; set; }
        }

        public AutoMetal()
        {
            // 初始化组件及窗口固定
            InitializeComponent();

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

            // 初始化键盘控制CheckBox监听
            InitializeKeyboardControlCheckBox();

            // 默认选择正常模式
            radioButton1.Checked = true;

            // 打开参数设置界面
            setingsToolStripMenuItem.Click += setingsToolStripMenuItem_Click;

            // 初始化算法处理页签交互
            InitializeAlgorithmTabHandlers();
            InitializeTrainingTabHandlers();

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
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "PyTorch权重 (*.pt;*.pth)|*.pt;*.pth|All files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (!string.IsNullOrWhiteSpace(txtBestPtPath.Text))
                {
                    try
                    {
                        var baseDir = Path.GetDirectoryName(txtBestPtPath.Text);
                        if (!string.IsNullOrWhiteSpace(baseDir) && Directory.Exists(baseDir))
                        {
                            dialog.InitialDirectory = baseDir;
                        }
                    }
                    catch
                    {
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtBestPtPath.Text = dialog.FileName;
                }
            }
        }

        private async void btnExportPtToEngine_Click(object sender, EventArgs e)
        {
            try
            {
                string ptPath = txtBestPtPath.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(ptPath) || !File.Exists(ptPath))
                {
                    SafeShowWarning("请先选择有效的最优权重(.pt/.pth)路径");
                    return;
                }

                string onnxPath = Path.Combine(
                    Path.GetDirectoryName(ptPath),
                    Path.GetFileNameWithoutExtension(ptPath) + ".onnx");

                Directory.CreateDirectory(Path.GetDirectoryName(onnxPath));
                string projectPath = AutoMetalConstants.deepLabProjectPath;
                if (!Directory.Exists(projectPath))
                {
                    SafeShowWarning($"DeepLab工程目录不存在：{projectPath}");
                    return;
                }

                string helperScriptPath = EnsureOnnxExportHelperScript(projectPath);
                string args = $"\"{helperScriptPath}\" --pt \"{ptPath}\" --onnx \"{onnxPath}\" --num-classes 2 --backbone mobilenet --downsample 16";

                SafeAppendLog($"开始导出ONNX：{ptPath} -> {onnxPath}");
                int exitCode = await RunPythonWithOptionalVenvAsync(projectPath, args);
                if (exitCode != 0 || !File.Exists(onnxPath))
                {
                    SafeShowWarning("ONNX导出失败，请检查日志输出");
                    return;
                }

                SafeAppendLog($"ONNX导出完成：{onnxPath}");

                string enginePath = txtTensorEnginePath.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(enginePath))
                {
                    enginePath = Path.Combine(Path.GetDirectoryName(onnxPath), Path.GetFileNameWithoutExtension(onnxPath) + ".engine");
                    txtTensorEnginePath.Text = enginePath;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(enginePath));

                string trtWorkingDir = AutoMetalConstants.deepLabProjectPath;
                if (!Directory.Exists(trtWorkingDir))
                {
                    trtWorkingDir = AppDomain.CurrentDomain.BaseDirectory;
                }

                string trtArgs = $"--onnx=\"{onnxPath}\" --saveEngine=\"{enginePath}\"";
                SafeAppendLog($"开始TensorRT转换：{onnxPath} -> {enginePath}");
                int trtExitCode = await RunCommandAsync(
                    "cmd.exe",
                    $"/c trtexec {trtArgs}",
                    trtWorkingDir);
                if (trtExitCode != 0 || !File.Exists(enginePath))
                {
                    SafeShowWarning("Engine转换失败，请确认trtexec已安装并在PATH中");
                    return;
                }

                SafeAppendLog($"Engine转换完成：{enginePath}");
            }
            catch (Exception ex)
            {
                SafeShowWarning($"pt转engine异常：{ex.Message}");
            }
        }

        private string EnsureOnnxExportHelperScript(string projectPath)
        {
            string scriptPath = Path.Combine(projectPath, "export_onnx_helper.py");
            string scriptContent = @"
import argparse
from deeplab import DeeplabV3

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Export DeepLab weights to ONNX')
    parser.add_argument('--pt', required=True)
    parser.add_argument('--onnx', required=True)
    parser.add_argument('--num-classes', type=int, default=2)
    parser.add_argument('--backbone', default='mobilenet')
    parser.add_argument('--downsample', type=int, default=16)
    args = parser.parse_args()

    deeplab = DeeplabV3(
        model_path=args.pt,
        num_classes=args.num_classes,
        backbone=args.backbone,
        downsample_factor=args.downsample,
        cuda=False
    )
    deeplab.convert_to_onnx(True, args.onnx)
";
            File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);
            return scriptPath;
        }

        private async Task<int> RunPythonWithOptionalVenvAsync(string projectPath, string pythonScriptArgs)
        {
            string activateBat = Path.Combine(projectPath, ".venv", "Scripts", "activate.bat");
            string cmdArguments;
            if (File.Exists(activateBat))
            {
                cmdArguments = $"/c \"call \"\"{activateBat}\"\" && python {pythonScriptArgs}\"";
            }
            else
            {
                string pythonExe = File.Exists(AutoMetalConstants.deepLabPythonExe)
                    ? AutoMetalConstants.deepLabPythonExe
                    : "python";
                cmdArguments = $"/c \"\"{pythonExe}\"\" {pythonScriptArgs}";
            }

            return await RunCommandAsync("cmd.exe", cmdArguments, projectPath);
        }

        private async Task<int> RunCommandAsync(string fileName, string arguments, string workingDirectory)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                p.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        SafeAppendLog(e.Data);
                    }
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        SafeAppendLog(e.Data);
                    }
                };

                if (!p.Start())
                {
                    return -1;
                }
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                await Task.Run(() => p.WaitForExit());
                return p.ExitCode;
            }
        }

        private void btnStopTrain_Click(object sender, EventArgs e)
        {
            if (!_isTrainingRunning)
            {
                _ = StartTrainingAsync();
                return;
            }

            StopTraining();
        }

        private async Task StartTrainingAsync()
        {
            if (_isTrainingRunning)
            {
                return;
            }

            if (!int.TryParse(txtEpoch.Text?.Trim(), out int epoch) || epoch <= 0)
            {
                SafeShowWarning("请先输入有效的Epoch");
                return;
            }

            if (!int.TryParse(txtBatchSize.Text?.Trim(), out int batchSize) || batchSize <= 0)
            {
                SafeShowWarning("请先输入有效的BatchSize");
                return;
            }

            string projectPath = AutoMetalConstants.deepLabProjectPath;
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
            {
                SafeShowWarning($"训练项目目录不存在：{projectPath}");
                return;
            }

            string trainScript = Path.Combine(projectPath, "train.py");
            if (!File.Exists(trainScript))
            {
                SafeShowWarning($"未找到训练脚本：{trainScript}");
                return;
            }

            _trainingTotalEpoch = epoch;
            progressBarTrain.Minimum = 0;
            progressBarTrain.Maximum = epoch;
            progressBarTrain.Value = 0;

            string quotedTrain = $"\"{trainScript}\"";
            string quotedVocPath = $"\"{AutoMetalConstants.deepLabVocPath}\"";
            string trainingSaveDir = Path.Combine(projectPath, "logs");
            string quotedSaveDir = $"\"{trainingSaveDir}\"";
            string activateBat = Path.Combine(projectPath, ".venv", "Scripts", "activate.bat");
            string trainingArgs = $"{quotedTrain} --epochs {epoch} --batch-size {batchSize} --voc-path {quotedVocPath} --save-dir {quotedSaveDir}";
            string cmdArguments;
            if (File.Exists(activateBat))
            {
                cmdArguments = $"/c \"call \"\"{activateBat}\"\" && python {trainingArgs}\"";
            }
            else
            {
                string pythonExe = File.Exists(AutoMetalConstants.deepLabPythonExe)
                    ? AutoMetalConstants.deepLabPythonExe
                    : "python";
                cmdArguments = $"/c \"\"{pythonExe}\"\" {trainingArgs}";
            }

            _trainingCancellationTokenSource?.Dispose();
            _trainingCancellationTokenSource = new CancellationTokenSource();
            _isTrainingRunning = true;
            btnStopTrain.Text = "停止";
            labelTrainStatus.Text = $"训练状态：启动中(1/{epoch})";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = cmdArguments,
                    WorkingDirectory = projectPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _trainingProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                _trainingProcess.OutputDataReceived += TrainingProcess_OutputDataReceived;
                _trainingProcess.ErrorDataReceived += TrainingProcess_OutputDataReceived;

                bool started = _trainingProcess.Start();
                if (!started)
                {
                    throw new Exception("训练进程未能启动");
                }

                _trainingProcess.BeginOutputReadLine();
                _trainingProcess.BeginErrorReadLine();

                using (_trainingCancellationTokenSource.Token.Register(() =>
                {
                    KillTrainingProcessTree();
                }))
                {
                    await Task.Run(() => _trainingProcess.WaitForExit());
                }

                if (_trainingCancellationTokenSource.IsCancellationRequested)
                {
                    labelTrainStatus.Text = "训练状态：已停止";
                    SafeAppendLog("训练已停止");
                }
                else if (_trainingProcess.ExitCode == 0)
                {
                    labelTrainStatus.Text = "训练状态：已完成";
                    progressBarTrain.Value = progressBarTrain.Maximum;
                    LoadLatestTrainingCurveImage();
                    SafeAppendLog("训练已完成");
                }
                else
                {
                    labelTrainStatus.Text = $"训练状态：失败(ExitCode={_trainingProcess.ExitCode})";
                    SafeAppendLog($"训练失败，退出码：{_trainingProcess.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                labelTrainStatus.Text = "训练状态：异常";
                SafeAppendLog($"训练异常：{ex.Message}");
            }
            finally
            {
                _isTrainingRunning = false;
                btnStopTrain.Text = "开始训练";
                if (_trainingProcess != null)
                {
                    _trainingProcess.OutputDataReceived -= TrainingProcess_OutputDataReceived;
                    _trainingProcess.ErrorDataReceived -= TrainingProcess_OutputDataReceived;
                    _trainingProcess.Dispose();
                    _trainingProcess = null;
                }
            }
        }

        private void StopTraining()
        {
            if (!_isTrainingRunning)
            {
                return;
            }

            try
            {
                _trainingCancellationTokenSource?.Cancel();
                KillTrainingProcessTree();
                labelTrainStatus.Text = "训练状态：停止中...";
            }
            catch
            {
            }
        }

        private void KillTrainingProcessTree()
        {
            try
            {
                if (_trainingProcess == null || _trainingProcess.HasExited)
                {
                    return;
                }

                int pid = _trainingProcess.Id;
                try
                {
                    using (var killer = Process.Start(new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/PID {pid} /T /F",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }))
                    {
                        killer?.WaitForExit(3000);
                    }
                }
                catch
                {
                    try
                    {
                        _trainingProcess.Kill();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private void TrainingProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            string line = e.Data.Trim();
            SafeAppendLog($"[train] {line}");

            var match = _trainingEpochRegex.Match(line);
            if (!match.Success)
            {
                return;
            }

            if (!int.TryParse(match.Groups[1].Value, out int currentEpoch))
            {
                return;
            }

            if (!int.TryParse(match.Groups[2].Value, out int totalEpochFromLog))
            {
                totalEpochFromLog = _trainingTotalEpoch;
            }

            int total = totalEpochFromLog > 0 ? totalEpochFromLog : Math.Max(_trainingTotalEpoch, 1);
            int value = Math.Min(Math.Max(currentEpoch, 0), total);

            if (progressBarTrain.InvokeRequired)
            {
                progressBarTrain.Invoke(new Action(() =>
                {
                    progressBarTrain.Maximum = total;
                    progressBarTrain.Value = Math.Min(value, progressBarTrain.Maximum);
                    labelTrainStatus.Text = $"训练状态：训练中({value}/{total})";
                }));
            }
            else
            {
                progressBarTrain.Maximum = total;
                progressBarTrain.Value = Math.Min(value, progressBarTrain.Maximum);
                labelTrainStatus.Text = $"训练状态：训练中({value}/{total})";
            }
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
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(txtTestSetPath.Text) && Directory.Exists(txtTestSetPath.Text))
                {
                    folderDialog.SelectedPath = txtTestSetPath.Text;
                }

                if (folderDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtTestSetPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnBrowseEnginePath_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "TensorRT Engine (*.engine)|*.engine|All files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (!string.IsNullOrWhiteSpace(txtTensorEnginePath.Text))
                {
                    try
                    {
                        var baseDir = Path.GetDirectoryName(txtTensorEnginePath.Text);
                        if (!string.IsNullOrWhiteSpace(baseDir) && Directory.Exists(baseDir))
                        {
                            dialog.InitialDirectory = baseDir;
                        }
                    }
                    catch
                    {
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtTensorEnginePath.Text = dialog.FileName;
                }
            }
        }

        private void btnStartInfer_Click(object sender, EventArgs e)
        {
            string testSetDir = txtTestSetPath.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(testSetDir) || !Directory.Exists(testSetDir))
            {
                SafeShowWarning("请先选择有效的测试集路径");
                return;
            }

            string enginePath = txtTensorEnginePath.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(enginePath) || !File.Exists(enginePath))
            {
                SafeShowWarning("请先选择有效的Engine文件路径");
                return;
            }

            try
            {
                CoverageAnalyzer.SetModelPath(enginePath);
                SafeAppendLog($"已切换Coverage模型Engine：{enginePath}");
            }
            catch (Exception ex)
            {
                SafeShowWarning($"Engine加载失败：{ex.Message}");
                return;
            }

            var supportedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff"
            };
            var images = Directory.EnumerateFiles(testSetDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => supportedExt.Contains(Path.GetExtension(p)))
                .OrderBy(p => p)
                .ToList();

            if (images.Count == 0)
            {
                SafeShowWarning("测试集目录中没有可推理图像");
                return;
            }

            progressBarInfer.Minimum = 0;
            progressBarInfer.Maximum = images.Count;
            progressBarInfer.Value = 0;

            int successCount = 0;
            int failedCount = 0;

            for (int i = 0; i < images.Count; i++)
            {
                string imagePath = images[i];
                string fileName = Path.GetFileName(imagePath);
                try
                {
                    using (var image = Cv2.ImRead(imagePath))
                    {
                        if (image.Empty())
                        {
                            failedCount++;
                            SafeAppendLog($"推理失败(图像读取为空): {fileName}");
                        }
                        else
                        {
                            string outputPath = Path.Combine(
                                Path.GetDirectoryName(imagePath),
                                Path.GetFileNameWithoutExtension(imagePath) + "_mask" + Path.GetExtension(imagePath));
                            double coverage = CoverageAnalyzer.detectImage(imagePath, null, outputPath);
                            successCount++;
                            SafeAppendLog($"推理完成: {fileName}, 覆盖率={coverage:F6}, 输出={outputPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    SafeAppendLog($"推理异常: {fileName}, 错误={ex.Message}");
                }

                progressBarInfer.Value = i + 1;
                labelInferStatus.Text = $"推理状态：进行中 {i + 1}/{images.Count}";
                Application.DoEvents();
            }

            labelInferStatus.Text = $"推理状态：完成 成功{successCount} 失败{failedCount}";
            SafeAppendLog($"测试集批量推理结束，总数={images.Count}，成功={successCount}，失败={failedCount}");
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
                //var sampleID_reco = glassNumberAnalyzer.GetGlassNumber(samplePath.Text);

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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SafeShowWarning), message);
            }
            else
            {
                MessageBox.Show(this, message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            }
            else if (File.Exists(_algCroppedImagePath))
            {
                _algPreprocessedImagePath = _algCroppedImagePath;
                LoadImageToPictureBox(picAlgPreprocessed, _algCroppedImagePath);
                labelAlgPreprocessed.Text = "裁剪图（当前样品）";
            }
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
                }
                else if (File.Exists(_algCroppedImagePath))
                {
                    _algPreprocessedImagePath = _algCroppedImagePath;
                    LoadImageToPictureBox(picAlgPreprocessed, _algCroppedImagePath);
                    labelAlgPreprocessed.Text = "裁剪图（当前样品）";
                }
            }

            SafeAppendLog($"批量预处理完成：总数={allImages.Count}，成功={successCount}，失败={failedCount}");
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

                if (picAlgPreprocessed.Image != null)
                {
                    picAlgPreprocessed.Image.Dispose();
                }
                picAlgPreprocessed.Image = BitmapConverter.ToBitmap(processedImage.CroppedImage);
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

            txtAlgMaskDir.Text = "自动加载：数字目录同级 mask 文件夹";
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

        private void btnAlgRun_Click(object sender, EventArgs e)
        {
            // 步骤三固定按已加载的全部目录(1~10)执行，不受当前树选中节点限制
            var runImages = GetAllLoadedImages();
            if (runImages.Count == 0)
            {
                SafeShowWarning("当前没有可处理图像，请先加载目录中的样品图像");
                return;
            }

            if (radioAlgManualMode.Checked)
            {
                if (listBoxAlgMasks.Items.Count == 0)
                {
                    SafeShowWarning("手动模式下请先加载Mask列表");
                    return;
                }
            }

            var batchResults = new List<AlgBatchResult>();
            foreach (var imagePath in runImages)
            {
                var result = new AlgBatchResult
                {
                    BatchName = GetBatchNameFromImagePath(imagePath),
                    SampleName = Path.GetFileName(imagePath),
                    Status = "成功"
                };

                if (radioAlgManualMode.Checked)
                {
                    string sampleNameNoExt = Path.GetFileNameWithoutExtension(imagePath);
                    string matchedMaskPath = listBoxAlgMasks.Items.Cast<string>()
                        .FirstOrDefault(path => string.Equals(Path.GetFileNameWithoutExtension(path), sampleNameNoExt, StringComparison.OrdinalIgnoreCase));
                    if (string.IsNullOrWhiteSpace(matchedMaskPath) || !File.Exists(matchedMaskPath))
                    {
                        result.Status = "失败：缺少同名Mask";
                        batchResults.Add(result);
                        continue;
                    }
                }

                try
                {
                    if (!TryPreprocessAndSaveOutputs(imagePath, updatePreview: false, out string croppedPath))
                    {
                        result.Status = "失败：预处理失败";
                        batchResults.Add(result);
                        continue;
                    }

                    var outputPaths = GetOutputImagePathsBySource(imagePath);
                    double coverage;
                    string maskCoveragePath;
                    if (radioAlgManualMode.Checked)
                    {
                        string sampleNameNoExt = Path.GetFileNameWithoutExtension(imagePath);
                        string matchedMaskPath = listBoxAlgMasks.Items.Cast<string>()
                            .First(path => string.Equals(Path.GetFileNameWithoutExtension(path), sampleNameNoExt, StringComparison.OrdinalIgnoreCase));
                        coverage = GenerateCoverageFromManualMask(
                            croppedPath,
                            matchedMaskPath,
                            outputPaths.maskPath,
                            outputPaths.outputPath,
                            out maskCoveragePath);
                    }
                    else
                    {
                        coverage = CoverageAnalyzer.detectImage(croppedPath, outputPaths.maskPath, outputPaths.outputPath);
                        maskCoveragePath = outputPaths.maskPath;
                    }
                    result.Coverage = coverage;
                    string outputCoveragePath = outputPaths.outputPath;
                    result.AlgorithmImagePath = File.Exists(outputCoveragePath) ? outputCoveragePath : croppedPath;
                    if (Math.Abs(coverage) < 1e-12)
                    {
                        result.Uniformity = double.NaN;
                        result.Status = "覆盖率为0，均匀性记为NaN（不参与平均）";
                    }
                    else
                    {
                        if (!File.Exists(outputCoveragePath))
                        {
                            result.Uniformity = double.NaN;
                            result.Status = "失败：缺少用于均匀性计算的mask";
                        }
                        else
                        {
                            result.Uniformity = ImageUniformityCalculator.CalculateUniformity(
                                croppedPath,
                                maskCoveragePath,
                                gaussianKsize: 101,
                                gaussianSigma: 0.0,
                                applyIlluminationCorrection: false);
                            TryGenerateUniformityHeatmapByPython(
                                croppedPath,
                                maskCoveragePath,
                                outputPaths.heatmapPath,
                                invertMask: radioAlgManualMode.Checked);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Status = $"失败：{ex.Message}";
                }

                batchResults.Add(result);
            }

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

            var successRows = batchResults
                .Where(r => r.Coverage.HasValue
                            && r.Uniformity.HasValue
                            && !double.IsNaN(r.Uniformity.Value)
                            && r.Coverage.Value > 0)
                .ToList();

            _algDirectoryAverages = successRows
                .GroupBy(r => string.IsNullOrWhiteSpace(r.BatchName) ? "未知目录" : r.BatchName)
                .OrderBy(g => g.Key)
                .Select(g => new AlgDirectoryAverage
                {
                    BatchName = g.Key,
                    CoverageAvg = g.Average(x => x.Coverage.Value),
                    UniformityAvg = g.Average(x => x.Uniformity.Value),
                    SampleCount = g.Count()
                })
                .ToList();

            if (_algDirectoryAverages.Count == 1)
            {
                txtAlgCoverageResult.Text = _algDirectoryAverages[0].CoverageAvg.ToString("F6");
                txtAlgUniformityResult.Text = _algDirectoryAverages[0].UniformityAvg.ToString("F6");
            }
            else if (_algDirectoryAverages.Count > 1)
            {
                txtAlgCoverageResult.Text = $"共{_algDirectoryAverages.Count}个目录，点击“查看目录均值”";
                txtAlgUniformityResult.Text = $"共{_algDirectoryAverages.Count}个目录，点击“查看目录均值”";
            }
            else
            {
                txtAlgCoverageResult.Text = "";
                txtAlgUniformityResult.Text = "";
            }

            ShowBatchResultDialog(batchResults);
            SafeAppendLog($"算法页签批量计算完成：总数={batchResults.Count}, 成功={successRows.Count}, 失败={batchResults.Count - successRows.Count}");
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
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "uniformity_heatmap.py");
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

        private void ShowBatchResultDialog(List<AlgBatchResult> batchResults)
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

            var rows = batchResults.Select(r => new
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

        private void ShowDirectoryAverageDialog(List<AlgDirectoryAverage> directoryAverages)
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

            var rows = directoryAverages.Select(x => new
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

        private void LoadImageToPictureBox(PictureBox pictureBox, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return;
            }

            try
            {
                using (var image = Image.FromFile(imagePath))
                {
                    ClearPictureBoxImage(pictureBox);
                    pictureBox.Image = new Bitmap(image);
                }
            }
            catch (Exception ex)
            {
                SafeShowWarning($"图像加载失败: {ex.Message}");
            }
        }

        private static void ClearPictureBoxImage(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }
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
            _algDirectoryAverages = new List<AlgDirectoryAverage>();
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
                // 直接操作UI控件。ListBox不支持自动换行，这里手动折行避免单行过长。
                var wrappedLines = WrapLogMessage(message, 80);
                string timePrefix = $"{DateTime.Now:HH:mm:ss} - ";
                for (int i = 0; i < wrappedLines.Count; i++)
                {
                    string prefix = i == 0 ? timePrefix : new string(' ', timePrefix.Length);
                    listBoxInfo.Items.Add(prefix + wrappedLines[i]);
                }
                listBoxInfo.TopIndex = listBoxInfo.Items.Count - 1; // 自动滚动到最后一行
            }
        }

        private List<string> WrapLogMessage(string message, int maxLineLength)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(message))
            {
                lines.Add(string.Empty);
                return lines;
            }

            var rawLines = message.Replace("\r\n", "\n").Split('\n');
            foreach (var raw in rawLines)
            {
                string remaining = raw;
                while (remaining.Length > maxLineLength)
                {
                    int breakPos = remaining.LastIndexOf(' ', maxLineLength);
                    if (breakPos <= 0)
                    {
                        breakPos = maxLineLength;
                    }

                    lines.Add(remaining.Substring(0, breakPos).TrimEnd());
                    remaining = remaining.Substring(breakPos).TrimStart();
                }

                lines.Add(remaining);
            }

            return lines;
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
                string glassNumber = glassNumberAnalyzer.GetGlassNumber(imagePath);

                // ===== 计算均匀性（基于裁剪图） =====
                double resValue = double.NaN;

                // ===== 计算覆盖率（基于裁剪图） =====
                double ratio = CoverageAnalyzer.detectImage(croppedImagePath);

                // 写入一行
                sb.AppendLine($"{imageName}\t{glassNumber}\t{resValue:F6}\t{ratio:F6}");
            }

            // 一次性写入 txt
            File.WriteAllText(saveTxtPath, sb.ToString(), Encoding.UTF8);

            MessageBox.Show("处理完成，结果已保存到 result.txt");
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }


}