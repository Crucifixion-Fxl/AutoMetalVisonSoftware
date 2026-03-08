using System.Windows.Media.Animation;

namespace AutoMetal
{
    partial class AutoMetal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoMetal));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.选择ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uahToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.编辑ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.参数设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.水平ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关系ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.相关ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.等等ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.自动化 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.runtimeLabel = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btnCloseServer = new System.Windows.Forms.Button();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.label_communcationPort = new System.Windows.Forms.Label();
            this.udpPort = new System.Windows.Forms.TextBox();
            this.samplePath = new System.Windows.Forms.TextBox();
            this.label_sample_path = new System.Windows.Forms.Label();
            this.sampleID = new System.Windows.Forms.TextBox();
            this.label_sample_id = new System.Windows.Forms.Label();
            this.expID = new System.Windows.Forms.TextBox();
            this.label_exp_id = new System.Windows.Forms.Label();
            this.btnAnalysis = new System.Windows.Forms.Button();
            this.ResCls_RichTextbox = new System.Windows.Forms.RichTextBox();
            this.label_pass = new System.Windows.Forms.Label();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label_uniformity = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.abnormalArea = new System.Windows.Forms.TextBox();
            this.normalArea = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label_coverageRate = new System.Windows.Forms.Label();
            this.textEdfStep = new System.Windows.Forms.TextBox();
            this.textEdfRange = new System.Windows.Forms.TextBox();
            this.label_edf = new System.Windows.Forms.Label();
            this.checkBoxScan = new System.Windows.Forms.CheckBox();
            this.comboBoxAutoScan = new System.Windows.Forms.ComboBox();
            this.snap = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.outputPicBox = new System.Windows.Forms.PictureBox();
            this.Ori_picture = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxInfo = new System.Windows.Forms.ListBox();
            this.textY1 = new System.Windows.Forms.TextBox();
            this.textX1 = new System.Windows.Forms.TextBox();
            this.textY0 = new System.Windows.Forms.TextBox();
            this.textX0 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.labelx_x1_y1 = new System.Windows.Forms.Label();
            this.label_x0_y0 = new System.Windows.Forms.Label();
            this.btnAutoScan = new System.Windows.Forms.Button();
            this.textFocus = new System.Windows.Forms.TextBox();
            this.btnAutoFocus = new System.Windows.Forms.Button();
            this.label_focus = new System.Windows.Forms.Label();
            this.textZ = new System.Windows.Forms.TextBox();
            this.textXY2 = new System.Windows.Forms.TextBox();
            this.textXY1 = new System.Windows.Forms.TextBox();
            this.label7_z = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label_xy = new System.Windows.Forms.Label();
            this.btnSetZ = new System.Windows.Forms.Button();
            this.btnSetXY = new System.Windows.Forms.Button();
            this.textContrast = new System.Windows.Forms.TextBox();
            this.textBrightness = new System.Windows.Forms.TextBox();
            this.label_contrast = new System.Windows.Forms.Label();
            this.label_brightness = new System.Windows.Forms.Label();
            this.btnSetContrast = new System.Windows.Forms.Button();
            this.btnSetBrightness = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label_ip = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.dBGridView = new System.Windows.Forms.TabPage();
            this.label37 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.textUniformity = new System.Windows.Forms.TextBox();
            this.btnSearchCoverageAndUniformity = new System.Windows.Forms.Button();
            this.textCoverage = new System.Windows.Forms.TextBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.btnSearchByDate = new System.Windows.Forms.Button();
            this.btn_getAllData = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnCalculateSimilarity = new System.Windows.Forms.Button();
            this.btnSearchByDateAndBatchId = new System.Windows.Forms.Button();
            this.BatchId = new System.Windows.Forms.TextBox();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label33 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.batchUniLabel_3 = new System.Windows.Forms.Label();
            this.batchCoverLabel_3 = new System.Windows.Forms.Label();
            this.batchUniLabel_5 = new System.Windows.Forms.Label();
            this.batchCoverLabel_5 = new System.Windows.Forms.Label();
            this.batchUniLabel_2 = new System.Windows.Forms.Label();
            this.batchCoverLabel_2 = new System.Windows.Forms.Label();
            this.batchUniLabel_4 = new System.Windows.Forms.Label();
            this.batchCoverLabel_4 = new System.Windows.Forms.Label();
            this.batchUniLabel_1 = new System.Windows.Forms.Label();
            this.batchCoverLabel_1 = new System.Windows.Forms.Label();
            this.textConsistency = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.batchPictureBox3 = new System.Windows.Forms.PictureBox();
            this.batchPictureBox5 = new System.Windows.Forms.PictureBox();
            this.batchPictureBox2 = new System.Windows.Forms.PictureBox();
            this.batchPictureBox4 = new System.Windows.Forms.PictureBox();
            this.batchPictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnSampleSearch = new System.Windows.Forms.Button();
            this.dbSampleText = new System.Windows.Forms.TextBox();
            this.label38 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.textBoxAbnormal_4 = new System.Windows.Forms.TextBox();
            this.textBoxAbnormal_3 = new System.Windows.Forms.TextBox();
            this.textBoxAbnormal_2 = new System.Windows.Forms.TextBox();
            this.textBoxAbnormal_1 = new System.Windows.Forms.TextBox();
            this.labelAbnormal_4 = new System.Windows.Forms.Label();
            this.labelAbnormal_3 = new System.Windows.Forms.Label();
            this.labelAbnormal_2 = new System.Windows.Forms.Label();
            this.labelAbnormal_1 = new System.Windows.Forms.Label();
            this.picBoxSampleAbnormal = new System.Windows.Forms.PictureBox();
            this.picBoxSample = new System.Windows.Forms.PictureBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.btn_FuncTest = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.自动化.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.outputPicBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Ori_picture)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.dBGridView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox1)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            this.tabPage7.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSampleAbnormal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSample)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.编辑ToolStripMenuItem,
            this.参数设置ToolStripMenuItem,
            this.水平ToolStripMenuItem,
            this.关系ToolStripMenuItem,
            this.相关ToolStripMenuItem,
            this.等等ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(1282, 24);
            this.menuStrip1.TabIndex = 76;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.选择ToolStripMenuItem,
            this.uahToolStripMenuItem});
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.文件ToolStripMenuItem.Text = "文件";
            // 
            // 选择ToolStripMenuItem
            // 
            this.选择ToolStripMenuItem.Name = "选择ToolStripMenuItem";
            this.选择ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.选择ToolStripMenuItem.Text = "选择";
            // 
            // uahToolStripMenuItem
            // 
            this.uahToolStripMenuItem.Name = "uahToolStripMenuItem";
            this.uahToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.uahToolStripMenuItem.Text = "uah";
            // 
            // 编辑ToolStripMenuItem
            // 
            this.编辑ToolStripMenuItem.Name = "编辑ToolStripMenuItem";
            this.编辑ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.编辑ToolStripMenuItem.Text = "编辑";
            // 
            // 参数设置ToolStripMenuItem
            // 
            this.参数设置ToolStripMenuItem.Name = "参数设置ToolStripMenuItem";
            this.参数设置ToolStripMenuItem.Size = new System.Drawing.Size(68, 22);
            this.参数设置ToolStripMenuItem.Text = "参数设置";
            // 
            // 水平ToolStripMenuItem
            // 
            this.水平ToolStripMenuItem.Name = "水平ToolStripMenuItem";
            this.水平ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.水平ToolStripMenuItem.Text = "水平";
            // 
            // 关系ToolStripMenuItem
            // 
            this.关系ToolStripMenuItem.Name = "关系ToolStripMenuItem";
            this.关系ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.关系ToolStripMenuItem.Text = "关系";
            // 
            // 相关ToolStripMenuItem
            // 
            this.相关ToolStripMenuItem.Name = "相关ToolStripMenuItem";
            this.相关ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.相关ToolStripMenuItem.Text = "相关";
            // 
            // 等等ToolStripMenuItem
            // 
            this.等等ToolStripMenuItem.Name = "等等ToolStripMenuItem";
            this.等等ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.等等ToolStripMenuItem.Text = "等等";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // 自动化
            // 
            this.自动化.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.自动化.Controls.Add(this.tabPage1);
            this.自动化.Controls.Add(this.tabPage2);
            this.自动化.Controls.Add(this.tabPage3);
            this.自动化.Controls.Add(this.tabPage4);
            this.自动化.Controls.Add(this.tabPage5);
            this.自动化.Font = new System.Drawing.Font("宋体", 9F);
            this.自动化.Location = new System.Drawing.Point(6, 21);
            this.自动化.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.自动化.Multiline = true;
            this.自动化.Name = "自动化";
            this.自动化.SelectedIndex = 0;
            this.自动化.Size = new System.Drawing.Size(1295, 552);
            this.自动化.TabIndex = 77;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btn_FuncTest);
            this.tabPage1.Controls.Add(this.runtimeLabel);
            this.tabPage1.Controls.Add(this.checkBox1);
            this.tabPage1.Controls.Add(this.btnCloseServer);
            this.tabPage1.Controls.Add(this.btnStartServer);
            this.tabPage1.Controls.Add(this.label25);
            this.tabPage1.Controls.Add(this.label_communcationPort);
            this.tabPage1.Controls.Add(this.udpPort);
            this.tabPage1.Controls.Add(this.samplePath);
            this.tabPage1.Controls.Add(this.label_sample_path);
            this.tabPage1.Controls.Add(this.sampleID);
            this.tabPage1.Controls.Add(this.label_sample_id);
            this.tabPage1.Controls.Add(this.expID);
            this.tabPage1.Controls.Add(this.label_exp_id);
            this.tabPage1.Controls.Add(this.btnAnalysis);
            this.tabPage1.Controls.Add(this.ResCls_RichTextbox);
            this.tabPage1.Controls.Add(this.label_pass);
            this.tabPage1.Controls.Add(this.textBox6);
            this.tabPage1.Controls.Add(this.label_uniformity);
            this.tabPage1.Controls.Add(this.textBox5);
            this.tabPage1.Controls.Add(this.abnormalArea);
            this.tabPage1.Controls.Add(this.normalArea);
            this.tabPage1.Controls.Add(this.label18);
            this.tabPage1.Controls.Add(this.label16);
            this.tabPage1.Controls.Add(this.label_coverageRate);
            this.tabPage1.Controls.Add(this.textEdfStep);
            this.tabPage1.Controls.Add(this.textEdfRange);
            this.tabPage1.Controls.Add(this.label_edf);
            this.tabPage1.Controls.Add(this.checkBoxScan);
            this.tabPage1.Controls.Add(this.comboBoxAutoScan);
            this.tabPage1.Controls.Add(this.snap);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.outputPicBox);
            this.tabPage1.Controls.Add(this.Ori_picture);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.listBoxInfo);
            this.tabPage1.Controls.Add(this.textY1);
            this.tabPage1.Controls.Add(this.textX1);
            this.tabPage1.Controls.Add(this.textY0);
            this.tabPage1.Controls.Add(this.textX0);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.labelx_x1_y1);
            this.tabPage1.Controls.Add(this.label_x0_y0);
            this.tabPage1.Controls.Add(this.btnAutoScan);
            this.tabPage1.Controls.Add(this.textFocus);
            this.tabPage1.Controls.Add(this.btnAutoFocus);
            this.tabPage1.Controls.Add(this.label_focus);
            this.tabPage1.Controls.Add(this.textZ);
            this.tabPage1.Controls.Add(this.textXY2);
            this.tabPage1.Controls.Add(this.textXY1);
            this.tabPage1.Controls.Add(this.label7_z);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.label_xy);
            this.tabPage1.Controls.Add(this.btnSetZ);
            this.tabPage1.Controls.Add(this.btnSetXY);
            this.tabPage1.Controls.Add(this.textContrast);
            this.tabPage1.Controls.Add(this.textBrightness);
            this.tabPage1.Controls.Add(this.label_contrast);
            this.tabPage1.Controls.Add(this.label_brightness);
            this.tabPage1.Controls.Add(this.btnSetContrast);
            this.tabPage1.Controls.Add(this.btnSetBrightness);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label_ip);
            this.tabPage1.Controls.Add(this.textBoxPort);
            this.tabPage1.Controls.Add(this.textBoxIP);
            this.tabPage1.Controls.Add(this.btnConnect);
            this.tabPage1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage1.Location = new System.Drawing.Point(22, 4);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage1.Size = new System.Drawing.Size(1269, 544);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "自动化";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // runtimeLabel
            // 
            this.runtimeLabel.AutoSize = true;
            this.runtimeLabel.Location = new System.Drawing.Point(24, 478);
            this.runtimeLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.runtimeLabel.Name = "runtimeLabel";
            this.runtimeLabel.Size = new System.Drawing.Size(0, 14);
            this.runtimeLabel.TabIndex = 142;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(931, 57);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(82, 18);
            this.checkBox1.TabIndex = 141;
            this.checkBox1.Text = "键盘控制";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // btnCloseServer
            // 
            this.btnCloseServer.Location = new System.Drawing.Point(1038, 22);
            this.btnCloseServer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCloseServer.Name = "btnCloseServer";
            this.btnCloseServer.Size = new System.Drawing.Size(48, 21);
            this.btnCloseServer.TabIndex = 140;
            this.btnCloseServer.Text = "关闭";
            this.btnCloseServer.UseVisualStyleBackColor = true;
            this.btnCloseServer.Click += new System.EventHandler(this.btnCloseServer_Click);
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(973, 22);
            this.btnStartServer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(53, 21);
            this.btnStartServer.TabIndex = 139;
            this.btnStartServer.Text = "开启";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(929, 25);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(0, 14);
            this.label25.TabIndex = 138;
            // 
            // label_communcationPort
            // 
            this.label_communcationPort.AutoSize = true;
            this.label_communcationPort.Location = new System.Drawing.Point(753, 25);
            this.label_communcationPort.Name = "label_communcationPort";
            this.label_communcationPort.Size = new System.Drawing.Size(77, 14);
            this.label_communcationPort.TabIndex = 137;
            this.label_communcationPort.Text = "通信端口：";
            // 
            // udpPort
            // 
            this.udpPort.Location = new System.Drawing.Point(827, 22);
            this.udpPort.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.udpPort.Name = "udpPort";
            this.udpPort.ReadOnly = true;
            this.udpPort.Size = new System.Drawing.Size(143, 23);
            this.udpPort.TabIndex = 136;
            // 
            // samplePath
            // 
            this.samplePath.Location = new System.Drawing.Point(827, 199);
            this.samplePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.samplePath.Name = "samplePath";
            this.samplePath.ReadOnly = true;
            this.samplePath.Size = new System.Drawing.Size(156, 23);
            this.samplePath.TabIndex = 135;
            // 
            // label_sample_path
            // 
            this.label_sample_path.AutoSize = true;
            this.label_sample_path.Location = new System.Drawing.Point(755, 203);
            this.label_sample_path.Name = "label_sample_path";
            this.label_sample_path.Size = new System.Drawing.Size(77, 14);
            this.label_sample_path.TabIndex = 134;
            this.label_sample_path.Text = "样本路径：";
            // 
            // sampleID
            // 
            this.sampleID.Location = new System.Drawing.Point(827, 167);
            this.sampleID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sampleID.Name = "sampleID";
            this.sampleID.ReadOnly = true;
            this.sampleID.Size = new System.Drawing.Size(156, 23);
            this.sampleID.TabIndex = 133;
            // 
            // label_sample_id
            // 
            this.label_sample_id.AutoSize = true;
            this.label_sample_id.Location = new System.Drawing.Point(755, 169);
            this.label_sample_id.Name = "label_sample_id";
            this.label_sample_id.Size = new System.Drawing.Size(77, 14);
            this.label_sample_id.TabIndex = 132;
            this.label_sample_id.Text = "样本序号：";
            // 
            // expID
            // 
            this.expID.Location = new System.Drawing.Point(827, 131);
            this.expID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.expID.Name = "expID";
            this.expID.ReadOnly = true;
            this.expID.Size = new System.Drawing.Size(156, 23);
            this.expID.TabIndex = 131;
            // 
            // label_exp_id
            // 
            this.label_exp_id.AutoSize = true;
            this.label_exp_id.Location = new System.Drawing.Point(755, 135);
            this.label_exp_id.Name = "label_exp_id";
            this.label_exp_id.Size = new System.Drawing.Size(63, 14);
            this.label_exp_id.TabIndex = 130;
            this.label_exp_id.Text = "实验ID：";
            // 
            // btnAnalysis
            // 
            this.btnAnalysis.Location = new System.Drawing.Point(756, 247);
            this.btnAnalysis.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAnalysis.Name = "btnAnalysis";
            this.btnAnalysis.Size = new System.Drawing.Size(247, 35);
            this.btnAnalysis.TabIndex = 129;
            this.btnAnalysis.Text = "成分分析";
            this.btnAnalysis.UseVisualStyleBackColor = true;
            this.btnAnalysis.Click += new System.EventHandler(this.button1_Click);
            // 
            // ResCls_RichTextbox
            // 
            this.ResCls_RichTextbox.Location = new System.Drawing.Point(848, 439);
            this.ResCls_RichTextbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ResCls_RichTextbox.Name = "ResCls_RichTextbox";
            this.ResCls_RichTextbox.ReadOnly = true;
            this.ResCls_RichTextbox.Size = new System.Drawing.Size(87, 25);
            this.ResCls_RichTextbox.TabIndex = 128;
            this.ResCls_RichTextbox.Text = "";
            // 
            // label_pass
            // 
            this.label_pass.AutoSize = true;
            this.label_pass.Location = new System.Drawing.Point(754, 443);
            this.label_pass.Name = "label_pass";
            this.label_pass.Size = new System.Drawing.Size(77, 14);
            this.label_pass.TabIndex = 127;
            this.label_pass.Text = "是否通过：";
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(848, 401);
            this.textBox6.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.Size = new System.Drawing.Size(156, 23);
            this.textBox6.TabIndex = 126;
            // 
            // label_uniformity
            // 
            this.label_uniformity.AutoSize = true;
            this.label_uniformity.Location = new System.Drawing.Point(754, 407);
            this.label_uniformity.Name = "label_uniformity";
            this.label_uniformity.Size = new System.Drawing.Size(91, 14);
            this.label_uniformity.TabIndex = 125;
            this.label_uniformity.Text = "镀膜均匀性：";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(848, 363);
            this.textBox5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(156, 23);
            this.textBox5.TabIndex = 124;
            // 
            // abnormalArea
            // 
            this.abnormalArea.Location = new System.Drawing.Point(848, 331);
            this.abnormalArea.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.abnormalArea.Name = "abnormalArea";
            this.abnormalArea.ReadOnly = true;
            this.abnormalArea.Size = new System.Drawing.Size(156, 23);
            this.abnormalArea.TabIndex = 123;
            // 
            // normalArea
            // 
            this.normalArea.Location = new System.Drawing.Point(848, 297);
            this.normalArea.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.normalArea.Name = "normalArea";
            this.normalArea.ReadOnly = true;
            this.normalArea.Size = new System.Drawing.Size(156, 23);
            this.normalArea.TabIndex = 122;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(754, 333);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(105, 14);
            this.label18.TabIndex = 121;
            this.label18.Text = "异常区域占比：";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(754, 299);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(105, 14);
            this.label16.TabIndex = 120;
            this.label16.Text = "正常区域占比：";
            // 
            // label_coverageRate
            // 
            this.label_coverageRate.AutoSize = true;
            this.label_coverageRate.Location = new System.Drawing.Point(754, 369);
            this.label_coverageRate.Name = "label_coverageRate";
            this.label_coverageRate.Size = new System.Drawing.Size(91, 14);
            this.label_coverageRate.TabIndex = 119;
            this.label_coverageRate.Text = "镀膜覆盖率：";
            // 
            // textEdfStep
            // 
            this.textEdfStep.Location = new System.Drawing.Point(151, 187);
            this.textEdfStep.Name = "textEdfStep";
            this.textEdfStep.Size = new System.Drawing.Size(37, 23);
            this.textEdfStep.TabIndex = 118;
            this.textEdfStep.Text = "10";
            this.textEdfStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textEdfRange
            // 
            this.textEdfRange.Location = new System.Drawing.Point(110, 187);
            this.textEdfRange.Name = "textEdfRange";
            this.textEdfRange.Size = new System.Drawing.Size(37, 23);
            this.textEdfRange.TabIndex = 117;
            this.textEdfRange.Text = "20";
            this.textEdfRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_edf
            // 
            this.label_edf.AutoSize = true;
            this.label_edf.Location = new System.Drawing.Point(17, 189);
            this.label_edf.Name = "label_edf";
            this.label_edf.Size = new System.Drawing.Size(105, 14);
            this.label_edf.TabIndex = 116;
            this.label_edf.Text = "EDF Range/Step";
            // 
            // checkBoxScan
            // 
            this.checkBoxScan.AutoSize = true;
            this.checkBoxScan.Location = new System.Drawing.Point(194, 187);
            this.checkBoxScan.Name = "checkBoxScan";
            this.checkBoxScan.Size = new System.Drawing.Size(82, 18);
            this.checkBoxScan.TabIndex = 115;
            this.checkBoxScan.Text = "ScanOnly";
            this.checkBoxScan.UseVisualStyleBackColor = true;
            // 
            // comboBoxAutoScan
            // 
            this.comboBoxAutoScan.FormattingEnabled = true;
            this.comboBoxAutoScan.Location = new System.Drawing.Point(267, 187);
            this.comboBoxAutoScan.Name = "comboBoxAutoScan";
            this.comboBoxAutoScan.Size = new System.Drawing.Size(97, 22);
            this.comboBoxAutoScan.TabIndex = 114;
            // 
            // snap
            // 
            this.snap.Location = new System.Drawing.Point(17, 261);
            this.snap.Name = "snap";
            this.snap.Size = new System.Drawing.Size(345, 22);
            this.snap.TabIndex = 113;
            this.snap.Text = "获取单张图像";
            this.snap.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(529, 468);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 14);
            this.label4.TabIndex = 112;
            this.label4.Text = "处理后图像";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(549, 227);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 14);
            this.label3.TabIndex = 111;
            this.label3.Text = "原始图像";
            // 
            // outputPicBox
            // 
            this.outputPicBox.BackColor = System.Drawing.SystemColors.ControlDark;
            this.outputPicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.outputPicBox.Location = new System.Drawing.Point(397, 247);
            this.outputPicBox.Name = "outputPicBox";
            this.outputPicBox.Size = new System.Drawing.Size(346, 216);
            this.outputPicBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.outputPicBox.TabIndex = 110;
            this.outputPicBox.TabStop = false;
            // 
            // Ori_picture
            // 
            this.Ori_picture.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Ori_picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Ori_picture.Location = new System.Drawing.Point(397, 9);
            this.Ori_picture.Name = "Ori_picture";
            this.Ori_picture.Size = new System.Drawing.Size(346, 216);
            this.Ori_picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Ori_picture.TabIndex = 109;
            this.Ori_picture.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 463);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 14);
            this.label2.TabIndex = 108;
            this.label2.Text = "日志区";
            // 
            // listBoxInfo
            // 
            this.listBoxInfo.FormattingEnabled = true;
            this.listBoxInfo.ItemHeight = 14;
            this.listBoxInfo.Location = new System.Drawing.Point(17, 289);
            this.listBoxInfo.Name = "listBoxInfo";
            this.listBoxInfo.Size = new System.Drawing.Size(346, 172);
            this.listBoxInfo.TabIndex = 107;
            // 
            // textY1
            // 
            this.textY1.Location = new System.Drawing.Point(192, 237);
            this.textY1.Name = "textY1";
            this.textY1.Size = new System.Drawing.Size(66, 23);
            this.textY1.TabIndex = 103;
            // 
            // textX1
            // 
            this.textX1.Location = new System.Drawing.Point(103, 237);
            this.textX1.Name = "textX1";
            this.textX1.Size = new System.Drawing.Size(66, 23);
            this.textX1.TabIndex = 104;
            // 
            // textY0
            // 
            this.textY0.Location = new System.Drawing.Point(192, 211);
            this.textY0.Name = "textY0";
            this.textY0.Size = new System.Drawing.Size(66, 23);
            this.textY0.TabIndex = 105;
            // 
            // textX0
            // 
            this.textX0.Location = new System.Drawing.Point(103, 211);
            this.textX0.Name = "textX0";
            this.textX0.Size = new System.Drawing.Size(66, 23);
            this.textX0.TabIndex = 106;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(173, 241);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(14, 14);
            this.label11.TabIndex = 99;
            this.label11.Text = "-";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(173, 215);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(14, 14);
            this.label10.TabIndex = 100;
            this.label10.Text = "-";
            // 
            // labelx_x1_y1
            // 
            this.labelx_x1_y1.AutoSize = true;
            this.labelx_x1_y1.Location = new System.Drawing.Point(24, 241);
            this.labelx_x1_y1.Name = "labelx_x1_y1";
            this.labelx_x1_y1.Size = new System.Drawing.Size(42, 14);
            this.labelx_x1_y1.TabIndex = 101;
            this.labelx_x1_y1.Text = "X1-Y1";
            // 
            // label_x0_y0
            // 
            this.label_x0_y0.AutoSize = true;
            this.label_x0_y0.Location = new System.Drawing.Point(24, 215);
            this.label_x0_y0.Name = "label_x0_y0";
            this.label_x0_y0.Size = new System.Drawing.Size(42, 14);
            this.label_x0_y0.TabIndex = 102;
            this.label_x0_y0.Text = "X0-Y0";
            // 
            // btnAutoScan
            // 
            this.btnAutoScan.Location = new System.Drawing.Point(264, 211);
            this.btnAutoScan.Name = "btnAutoScan";
            this.btnAutoScan.Size = new System.Drawing.Size(98, 44);
            this.btnAutoScan.TabIndex = 98;
            this.btnAutoScan.Text = "autoScan";
            this.btnAutoScan.UseVisualStyleBackColor = true;
            this.btnAutoScan.Click += new System.EventHandler(this.btnAutoScan_Click);
            // 
            // textFocus
            // 
            this.textFocus.Location = new System.Drawing.Point(103, 153);
            this.textFocus.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textFocus.Name = "textFocus";
            this.textFocus.Size = new System.Drawing.Size(155, 23);
            this.textFocus.TabIndex = 97;
            this.textFocus.Text = "100";
            // 
            // btnAutoFocus
            // 
            this.btnAutoFocus.Location = new System.Drawing.Point(264, 153);
            this.btnAutoFocus.Name = "btnAutoFocus";
            this.btnAutoFocus.Size = new System.Drawing.Size(98, 22);
            this.btnAutoFocus.TabIndex = 96;
            this.btnAutoFocus.Text = "Auto Focus";
            this.btnAutoFocus.UseVisualStyleBackColor = true;
            this.btnAutoFocus.Click += new System.EventHandler(this.btnAutoFocus_Click);
            // 
            // label_focus
            // 
            this.label_focus.AutoSize = true;
            this.label_focus.Location = new System.Drawing.Point(24, 153);
            this.label_focus.Name = "label_focus";
            this.label_focus.Size = new System.Drawing.Size(91, 14);
            this.label_focus.TabIndex = 95;
            this.label_focus.Text = "Focus Range:";
            // 
            // textZ
            // 
            this.textZ.Location = new System.Drawing.Point(103, 127);
            this.textZ.Name = "textZ";
            this.textZ.Size = new System.Drawing.Size(155, 23);
            this.textZ.TabIndex = 92;
            // 
            // textXY2
            // 
            this.textXY2.Location = new System.Drawing.Point(192, 95);
            this.textXY2.Name = "textXY2";
            this.textXY2.Size = new System.Drawing.Size(66, 23);
            this.textXY2.TabIndex = 93;
            // 
            // textXY1
            // 
            this.textXY1.Location = new System.Drawing.Point(103, 95);
            this.textXY1.Name = "textXY1";
            this.textXY1.Size = new System.Drawing.Size(66, 23);
            this.textXY1.TabIndex = 94;
            // 
            // label7_z
            // 
            this.label7_z.AutoSize = true;
            this.label7_z.Location = new System.Drawing.Point(24, 131);
            this.label7_z.Name = "label7_z";
            this.label7_z.Size = new System.Drawing.Size(21, 14);
            this.label7_z.TabIndex = 89;
            this.label7_z.Text = "Z:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(173, 101);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 14);
            this.label9.TabIndex = 90;
            this.label9.Text = "-";
            // 
            // label_xy
            // 
            this.label_xy.AutoSize = true;
            this.label_xy.Location = new System.Drawing.Point(24, 103);
            this.label_xy.Name = "label_xy";
            this.label_xy.Size = new System.Drawing.Size(28, 14);
            this.label_xy.TabIndex = 91;
            this.label_xy.Text = "XY:";
            // 
            // btnSetZ
            // 
            this.btnSetZ.Location = new System.Drawing.Point(264, 125);
            this.btnSetZ.Name = "btnSetZ";
            this.btnSetZ.Size = new System.Drawing.Size(98, 22);
            this.btnSetZ.TabIndex = 87;
            this.btnSetZ.Text = "SetZ";
            this.btnSetZ.UseVisualStyleBackColor = true;
            this.btnSetZ.Click += new System.EventHandler(this.btnSetZ_Click);
            // 
            // btnSetXY
            // 
            this.btnSetXY.Location = new System.Drawing.Point(264, 95);
            this.btnSetXY.Name = "btnSetXY";
            this.btnSetXY.Size = new System.Drawing.Size(98, 22);
            this.btnSetXY.TabIndex = 88;
            this.btnSetXY.Text = "SetXY";
            this.btnSetXY.UseVisualStyleBackColor = true;
            this.btnSetXY.Click += new System.EventHandler(this.btnSetXY_Click);
            // 
            // textContrast
            // 
            this.textContrast.Location = new System.Drawing.Point(103, 71);
            this.textContrast.Name = "textContrast";
            this.textContrast.Size = new System.Drawing.Size(155, 23);
            this.textContrast.TabIndex = 85;
            // 
            // textBrightness
            // 
            this.textBrightness.Location = new System.Drawing.Point(103, 41);
            this.textBrightness.Name = "textBrightness";
            this.textBrightness.Size = new System.Drawing.Size(155, 23);
            this.textBrightness.TabIndex = 86;
            // 
            // label_contrast
            // 
            this.label_contrast.AutoSize = true;
            this.label_contrast.Location = new System.Drawing.Point(24, 77);
            this.label_contrast.Name = "label_contrast";
            this.label_contrast.Size = new System.Drawing.Size(70, 14);
            this.label_contrast.TabIndex = 83;
            this.label_contrast.Text = "Contrast:";
            // 
            // label_brightness
            // 
            this.label_brightness.AutoSize = true;
            this.label_brightness.Location = new System.Drawing.Point(24, 47);
            this.label_brightness.Name = "label_brightness";
            this.label_brightness.Size = new System.Drawing.Size(84, 14);
            this.label_brightness.TabIndex = 84;
            this.label_brightness.Text = "Brightness:";
            // 
            // btnSetContrast
            // 
            this.btnSetContrast.Location = new System.Drawing.Point(264, 71);
            this.btnSetContrast.Name = "btnSetContrast";
            this.btnSetContrast.Size = new System.Drawing.Size(98, 22);
            this.btnSetContrast.TabIndex = 81;
            this.btnSetContrast.Text = "SetContrast";
            this.btnSetContrast.UseVisualStyleBackColor = true;
            this.btnSetContrast.Click += new System.EventHandler(this.btnSetContrast_Click);
            // 
            // btnSetBrightness
            // 
            this.btnSetBrightness.Location = new System.Drawing.Point(264, 41);
            this.btnSetBrightness.Name = "btnSetBrightness";
            this.btnSetBrightness.Size = new System.Drawing.Size(98, 22);
            this.btnSetBrightness.TabIndex = 82;
            this.btnSetBrightness.Text = "SetBrightness";
            this.btnSetBrightness.UseVisualStyleBackColor = true;
            this.btnSetBrightness.Click += new System.EventHandler(this.btnSetBrightness_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(204, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 14);
            this.label1.TabIndex = 79;
            this.label1.Text = ":";
            // 
            // label_ip
            // 
            this.label_ip.AutoSize = true;
            this.label_ip.Location = new System.Drawing.Point(24, 13);
            this.label_ip.Name = "label_ip";
            this.label_ip.Size = new System.Drawing.Size(77, 14);
            this.label_ip.TabIndex = 80;
            this.label_ip.Text = "IPAddress:";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(217, 9);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.ReadOnly = true;
            this.textBoxPort.Size = new System.Drawing.Size(41, 23);
            this.textBoxPort.TabIndex = 77;
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(103, 9);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.ReadOnly = true;
            this.textBoxIP.Size = new System.Drawing.Size(99, 23);
            this.textBoxIP.TabIndex = 78;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(264, 7);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(98, 23);
            this.btnConnect.TabIndex = 76;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tabControl2);
            this.tabPage2.Font = new System.Drawing.Font("宋体", 10.5F);
            this.tabPage2.Location = new System.Drawing.Point(22, 4);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage2.Size = new System.Drawing.Size(1269, 544);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "数据库";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.dBGridView);
            this.tabControl2.Location = new System.Drawing.Point(2, 12);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1247, 509);
            this.tabControl2.TabIndex = 0;
            // 
            // dBGridView
            // 
            this.dBGridView.Controls.Add(this.label37);
            this.dBGridView.Controls.Add(this.label36);
            this.dBGridView.Controls.Add(this.textUniformity);
            this.dBGridView.Controls.Add(this.btnSearchCoverageAndUniformity);
            this.dBGridView.Controls.Add(this.textCoverage);
            this.dBGridView.Controls.Add(this.dateTimePicker1);
            this.dBGridView.Controls.Add(this.btnSearchByDate);
            this.dBGridView.Controls.Add(this.btn_getAllData);
            this.dBGridView.Controls.Add(this.dataGridView1);
            this.dBGridView.Location = new System.Drawing.Point(4, 24);
            this.dBGridView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dBGridView.Name = "dBGridView";
            this.dBGridView.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dBGridView.Size = new System.Drawing.Size(1239, 481);
            this.dBGridView.TabIndex = 0;
            this.dBGridView.Text = "数据库";
            this.dBGridView.UseVisualStyleBackColor = true;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(639, 13);
            this.label37.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(63, 14);
            this.label37.TabIndex = 10;
            this.label37.Text = "均匀性：";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(495, 13);
            this.label36.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(63, 14);
            this.label36.TabIndex = 9;
            this.label36.Text = "覆盖率：";
            // 
            // textUniformity
            // 
            this.textUniformity.Location = new System.Drawing.Point(715, 11);
            this.textUniformity.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textUniformity.Name = "textUniformity";
            this.textUniformity.Size = new System.Drawing.Size(94, 23);
            this.textUniformity.TabIndex = 7;
            this.textUniformity.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // btnSearchCoverageAndUniformity
            // 
            this.btnSearchCoverageAndUniformity.Location = new System.Drawing.Point(812, 11);
            this.btnSearchCoverageAndUniformity.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSearchCoverageAndUniformity.Name = "btnSearchCoverageAndUniformity";
            this.btnSearchCoverageAndUniformity.Size = new System.Drawing.Size(75, 21);
            this.btnSearchCoverageAndUniformity.TabIndex = 6;
            this.btnSearchCoverageAndUniformity.Text = "条件查询";
            this.btnSearchCoverageAndUniformity.UseVisualStyleBackColor = true;
            this.btnSearchCoverageAndUniformity.Click += new System.EventHandler(this.btnSearchCoverage_Click);
            // 
            // textCoverage
            // 
            this.textCoverage.Location = new System.Drawing.Point(569, 9);
            this.textCoverage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textCoverage.Name = "textCoverage";
            this.textCoverage.Size = new System.Drawing.Size(68, 23);
            this.textCoverage.TabIndex = 5;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(157, 11);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(135, 23);
            this.dateTimePicker1.TabIndex = 4;
            // 
            // btnSearchByDate
            // 
            this.btnSearchByDate.Location = new System.Drawing.Point(306, 9);
            this.btnSearchByDate.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSearchByDate.Name = "btnSearchByDate";
            this.btnSearchByDate.Size = new System.Drawing.Size(111, 22);
            this.btnSearchByDate.TabIndex = 2;
            this.btnSearchByDate.Text = "按照日期查询";
            this.btnSearchByDate.UseVisualStyleBackColor = true;
            this.btnSearchByDate.Click += new System.EventHandler(this.btnSearchByDate_Click_1);
            // 
            // btn_getAllData
            // 
            this.btn_getAllData.Location = new System.Drawing.Point(10, 9);
            this.btn_getAllData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btn_getAllData.Name = "btn_getAllData";
            this.btn_getAllData.Size = new System.Drawing.Size(124, 22);
            this.btn_getAllData.TabIndex = 1;
            this.btn_getAllData.Text = "获取全部数据";
            this.btn_getAllData.UseVisualStyleBackColor = true;
            this.btn_getAllData.Click += new System.EventHandler(this.btn_getAllData_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(-3, 35);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 30;
            this.dataGridView1.Size = new System.Drawing.Size(1028, 373);
            this.dataGridView1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnCalculateSimilarity);
            this.tabPage3.Controls.Add(this.btnSearchByDateAndBatchId);
            this.tabPage3.Controls.Add(this.BatchId);
            this.tabPage3.Controls.Add(this.dateTimePicker2);
            this.tabPage3.Controls.Add(this.label33);
            this.tabPage3.Controls.Add(this.label32);
            this.tabPage3.Controls.Add(this.batchUniLabel_3);
            this.tabPage3.Controls.Add(this.batchCoverLabel_3);
            this.tabPage3.Controls.Add(this.batchUniLabel_5);
            this.tabPage3.Controls.Add(this.batchCoverLabel_5);
            this.tabPage3.Controls.Add(this.batchUniLabel_2);
            this.tabPage3.Controls.Add(this.batchCoverLabel_2);
            this.tabPage3.Controls.Add(this.batchUniLabel_4);
            this.tabPage3.Controls.Add(this.batchCoverLabel_4);
            this.tabPage3.Controls.Add(this.batchUniLabel_1);
            this.tabPage3.Controls.Add(this.batchCoverLabel_1);
            this.tabPage3.Controls.Add(this.textConsistency);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.label12);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.batchPictureBox3);
            this.tabPage3.Controls.Add(this.batchPictureBox5);
            this.tabPage3.Controls.Add(this.batchPictureBox2);
            this.tabPage3.Controls.Add(this.batchPictureBox4);
            this.tabPage3.Controls.Add(this.batchPictureBox1);
            this.tabPage3.Font = new System.Drawing.Font("宋体", 10.5F);
            this.tabPage3.Location = new System.Drawing.Point(22, 4);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1269, 544);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "批次对比";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnCalculateSimilarity
            // 
            this.btnCalculateSimilarity.Location = new System.Drawing.Point(1035, 290);
            this.btnCalculateSimilarity.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCalculateSimilarity.Name = "btnCalculateSimilarity";
            this.btnCalculateSimilarity.Size = new System.Drawing.Size(93, 27);
            this.btnCalculateSimilarity.TabIndex = 32;
            this.btnCalculateSimilarity.Text = "计算相似度";
            this.btnCalculateSimilarity.UseVisualStyleBackColor = true;
            this.btnCalculateSimilarity.Click += new System.EventHandler(this.btnCalculateSimilarity_Click);
            // 
            // btnSearchByDateAndBatchId
            // 
            this.btnSearchByDateAndBatchId.Location = new System.Drawing.Point(976, 389);
            this.btnSearchByDateAndBatchId.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSearchByDateAndBatchId.Name = "btnSearchByDateAndBatchId";
            this.btnSearchByDateAndBatchId.Size = new System.Drawing.Size(73, 21);
            this.btnSearchByDateAndBatchId.TabIndex = 31;
            this.btnSearchByDateAndBatchId.Text = "查询批次";
            this.btnSearchByDateAndBatchId.UseVisualStyleBackColor = true;
            this.btnSearchByDateAndBatchId.Click += new System.EventHandler(this.btnSearchByDateAndBatchId_Click);
            // 
            // BatchId
            // 
            this.BatchId.Location = new System.Drawing.Point(893, 389);
            this.BatchId.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BatchId.Name = "BatchId";
            this.BatchId.Size = new System.Drawing.Size(68, 23);
            this.BatchId.TabIndex = 30;
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Location = new System.Drawing.Point(893, 339);
            this.dateTimePicker2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(135, 23);
            this.dateTimePicker2.TabIndex = 29;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(825, 391);
            this.label33.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(49, 14);
            this.label33.TabIndex = 28;
            this.label33.Text = "批次号";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(825, 344);
            this.label32.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(35, 14);
            this.label32.TabIndex = 27;
            this.label32.Text = "日期";
            // 
            // batchUniLabel_3
            // 
            this.batchUniLabel_3.AutoSize = true;
            this.batchUniLabel_3.Location = new System.Drawing.Point(1147, 241);
            this.batchUniLabel_3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchUniLabel_3.Name = "batchUniLabel_3";
            this.batchUniLabel_3.Size = new System.Drawing.Size(49, 14);
            this.batchUniLabel_3.TabIndex = 25;
            this.batchUniLabel_3.Text = "均匀性";
            // 
            // batchCoverLabel_3
            // 
            this.batchCoverLabel_3.AutoSize = true;
            this.batchCoverLabel_3.Location = new System.Drawing.Point(1147, 213);
            this.batchCoverLabel_3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchCoverLabel_3.Name = "batchCoverLabel_3";
            this.batchCoverLabel_3.Size = new System.Drawing.Size(49, 14);
            this.batchCoverLabel_3.TabIndex = 24;
            this.batchCoverLabel_3.Text = "覆盖率";
            // 
            // batchUniLabel_5
            // 
            this.batchUniLabel_5.AutoSize = true;
            this.batchUniLabel_5.Location = new System.Drawing.Point(735, 506);
            this.batchUniLabel_5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchUniLabel_5.Name = "batchUniLabel_5";
            this.batchUniLabel_5.Size = new System.Drawing.Size(49, 14);
            this.batchUniLabel_5.TabIndex = 22;
            this.batchUniLabel_5.Text = "均匀性";
            // 
            // batchCoverLabel_5
            // 
            this.batchCoverLabel_5.AutoSize = true;
            this.batchCoverLabel_5.Location = new System.Drawing.Point(735, 465);
            this.batchCoverLabel_5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchCoverLabel_5.Name = "batchCoverLabel_5";
            this.batchCoverLabel_5.Size = new System.Drawing.Size(49, 14);
            this.batchCoverLabel_5.TabIndex = 21;
            this.batchCoverLabel_5.Text = "覆盖率";
            // 
            // batchUniLabel_2
            // 
            this.batchUniLabel_2.AutoSize = true;
            this.batchUniLabel_2.Location = new System.Drawing.Point(735, 236);
            this.batchUniLabel_2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchUniLabel_2.Name = "batchUniLabel_2";
            this.batchUniLabel_2.Size = new System.Drawing.Size(49, 14);
            this.batchUniLabel_2.TabIndex = 19;
            this.batchUniLabel_2.Text = "均匀性";
            // 
            // batchCoverLabel_2
            // 
            this.batchCoverLabel_2.AutoSize = true;
            this.batchCoverLabel_2.Location = new System.Drawing.Point(735, 203);
            this.batchCoverLabel_2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchCoverLabel_2.Name = "batchCoverLabel_2";
            this.batchCoverLabel_2.Size = new System.Drawing.Size(49, 14);
            this.batchCoverLabel_2.TabIndex = 18;
            this.batchCoverLabel_2.Text = "覆盖率";
            // 
            // batchUniLabel_4
            // 
            this.batchUniLabel_4.AutoSize = true;
            this.batchUniLabel_4.Location = new System.Drawing.Point(333, 499);
            this.batchUniLabel_4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchUniLabel_4.Name = "batchUniLabel_4";
            this.batchUniLabel_4.Size = new System.Drawing.Size(49, 14);
            this.batchUniLabel_4.TabIndex = 16;
            this.batchUniLabel_4.Text = "均匀性";
            // 
            // batchCoverLabel_4
            // 
            this.batchCoverLabel_4.AutoSize = true;
            this.batchCoverLabel_4.Location = new System.Drawing.Point(333, 465);
            this.batchCoverLabel_4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchCoverLabel_4.Name = "batchCoverLabel_4";
            this.batchCoverLabel_4.Size = new System.Drawing.Size(49, 14);
            this.batchCoverLabel_4.TabIndex = 15;
            this.batchCoverLabel_4.Text = "覆盖率";
            // 
            // batchUniLabel_1
            // 
            this.batchUniLabel_1.AutoSize = true;
            this.batchUniLabel_1.Location = new System.Drawing.Point(333, 241);
            this.batchUniLabel_1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchUniLabel_1.Name = "batchUniLabel_1";
            this.batchUniLabel_1.Size = new System.Drawing.Size(49, 14);
            this.batchUniLabel_1.TabIndex = 13;
            this.batchUniLabel_1.Text = "均匀性";
            // 
            // batchCoverLabel_1
            // 
            this.batchCoverLabel_1.AutoSize = true;
            this.batchCoverLabel_1.Location = new System.Drawing.Point(333, 213);
            this.batchCoverLabel_1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.batchCoverLabel_1.Name = "batchCoverLabel_1";
            this.batchCoverLabel_1.Size = new System.Drawing.Size(49, 14);
            this.batchCoverLabel_1.TabIndex = 12;
            this.batchCoverLabel_1.Text = "覆盖率";
            // 
            // textConsistency
            // 
            this.textConsistency.Location = new System.Drawing.Point(893, 293);
            this.textConsistency.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.textConsistency.Name = "textConsistency";
            this.textConsistency.Size = new System.Drawing.Size(125, 23);
            this.textConsistency.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(825, 294);
            this.label13.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(49, 14);
            this.label13.TabIndex = 10;
            this.label13.Text = "一致性";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(518, 267);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(98, 14);
            this.label12.TabIndex = 9;
            this.label12.Text = "批次号-顺序号";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(117, 267);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(98, 14);
            this.label8.TabIndex = 8;
            this.label8.Text = "批次号-顺序号";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(931, 11);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(98, 14);
            this.label7.TabIndex = 7;
            this.label7.Text = "批次号-顺序号";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(518, 11);
            this.label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 14);
            this.label6.TabIndex = 6;
            this.label6.Text = "批次号-顺序号";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(117, 11);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 14);
            this.label5.TabIndex = 5;
            this.label5.Text = "批次号-顺序号";
            // 
            // batchPictureBox3
            // 
            this.batchPictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.batchPictureBox3.Location = new System.Drawing.Point(827, 26);
            this.batchPictureBox3.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.batchPictureBox3.Name = "batchPictureBox3";
            this.batchPictureBox3.Size = new System.Drawing.Size(311, 231);
            this.batchPictureBox3.TabIndex = 4;
            this.batchPictureBox3.TabStop = false;
            // 
            // batchPictureBox5
            // 
            this.batchPictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.batchPictureBox5.Location = new System.Drawing.Point(419, 290);
            this.batchPictureBox5.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.batchPictureBox5.Name = "batchPictureBox5";
            this.batchPictureBox5.Size = new System.Drawing.Size(311, 231);
            this.batchPictureBox5.TabIndex = 3;
            this.batchPictureBox5.TabStop = false;
            // 
            // batchPictureBox2
            // 
            this.batchPictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.batchPictureBox2.Location = new System.Drawing.Point(419, 26);
            this.batchPictureBox2.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.batchPictureBox2.Name = "batchPictureBox2";
            this.batchPictureBox2.Size = new System.Drawing.Size(312, 225);
            this.batchPictureBox2.TabIndex = 2;
            this.batchPictureBox2.TabStop = false;
            // 
            // batchPictureBox4
            // 
            this.batchPictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.batchPictureBox4.Location = new System.Drawing.Point(27, 283);
            this.batchPictureBox4.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.batchPictureBox4.Name = "batchPictureBox4";
            this.batchPictureBox4.Size = new System.Drawing.Size(303, 231);
            this.batchPictureBox4.TabIndex = 1;
            this.batchPictureBox4.TabStop = false;
            // 
            // batchPictureBox1
            // 
            this.batchPictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.batchPictureBox1.Location = new System.Drawing.Point(27, 26);
            this.batchPictureBox1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.batchPictureBox1.Name = "batchPictureBox1";
            this.batchPictureBox1.Size = new System.Drawing.Size(303, 231);
            this.batchPictureBox1.TabIndex = 0;
            this.batchPictureBox1.TabStop = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnSampleSearch);
            this.tabPage4.Controls.Add(this.dbSampleText);
            this.tabPage4.Controls.Add(this.label38);
            this.tabPage4.Controls.Add(this.tabControl1);
            this.tabPage4.Controls.Add(this.picBoxSample);
            this.tabPage4.Font = new System.Drawing.Font("宋体", 10.5F);
            this.tabPage4.Location = new System.Drawing.Point(22, 4);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1269, 544);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "指标分析";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnSampleSearch
            // 
            this.btnSampleSearch.Location = new System.Drawing.Point(182, 14);
            this.btnSampleSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSampleSearch.Name = "btnSampleSearch";
            this.btnSampleSearch.Size = new System.Drawing.Size(68, 21);
            this.btnSampleSearch.TabIndex = 4;
            this.btnSampleSearch.Text = "查看";
            this.btnSampleSearch.UseVisualStyleBackColor = true;
            this.btnSampleSearch.Click += new System.EventHandler(this.btnSampleSearch_Click);
            // 
            // dbSampleText
            // 
            this.dbSampleText.Location = new System.Drawing.Point(105, 14);
            this.dbSampleText.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dbSampleText.Name = "dbSampleText";
            this.dbSampleText.Size = new System.Drawing.Size(68, 23);
            this.dbSampleText.TabIndex = 3;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(25, 17);
            this.label38.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(77, 14);
            this.label38.TabIndex = 2;
            this.label38.Text = "样品编号：";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage8);
            this.tabControl1.Location = new System.Drawing.Point(607, 17);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(639, 503);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.pictureBox7);
            this.tabPage6.Location = new System.Drawing.Point(4, 24);
            this.tabPage6.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage6.Size = new System.Drawing.Size(631, 475);
            this.tabPage6.TabIndex = 0;
            this.tabPage6.Text = "覆盖率";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // pictureBox7
            // 
            this.pictureBox7.Location = new System.Drawing.Point(15, 13);
            this.pictureBox7.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(604, 453);
            this.pictureBox7.TabIndex = 0;
            this.pictureBox7.TabStop = false;
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.panel1);
            this.tabPage7.Location = new System.Drawing.Point(4, 24);
            this.tabPage7.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage7.Size = new System.Drawing.Size(631, 475);
            this.tabPage7.TabIndex = 1;
            this.tabPage7.Text = "均匀性";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(617, 451);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(597, 437);
            this.tableLayoutPanel1.TabIndex = 1;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.textBoxAbnormal_4);
            this.tabPage8.Controls.Add(this.textBoxAbnormal_3);
            this.tabPage8.Controls.Add(this.textBoxAbnormal_2);
            this.tabPage8.Controls.Add(this.textBoxAbnormal_1);
            this.tabPage8.Controls.Add(this.labelAbnormal_4);
            this.tabPage8.Controls.Add(this.labelAbnormal_3);
            this.tabPage8.Controls.Add(this.labelAbnormal_2);
            this.tabPage8.Controls.Add(this.labelAbnormal_1);
            this.tabPage8.Controls.Add(this.picBoxSampleAbnormal);
            this.tabPage8.Location = new System.Drawing.Point(4, 24);
            this.tabPage8.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Size = new System.Drawing.Size(631, 475);
            this.tabPage8.TabIndex = 2;
            this.tabPage8.Text = "异常区";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // textBoxAbnormal_4
            // 
            this.textBoxAbnormal_4.Location = new System.Drawing.Point(557, 350);
            this.textBoxAbnormal_4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxAbnormal_4.Name = "textBoxAbnormal_4";
            this.textBoxAbnormal_4.ReadOnly = true;
            this.textBoxAbnormal_4.Size = new System.Drawing.Size(68, 23);
            this.textBoxAbnormal_4.TabIndex = 8;
            // 
            // textBoxAbnormal_3
            // 
            this.textBoxAbnormal_3.Location = new System.Drawing.Point(557, 268);
            this.textBoxAbnormal_3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxAbnormal_3.Name = "textBoxAbnormal_3";
            this.textBoxAbnormal_3.ReadOnly = true;
            this.textBoxAbnormal_3.Size = new System.Drawing.Size(68, 23);
            this.textBoxAbnormal_3.TabIndex = 7;
            // 
            // textBoxAbnormal_2
            // 
            this.textBoxAbnormal_2.Location = new System.Drawing.Point(557, 187);
            this.textBoxAbnormal_2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxAbnormal_2.Name = "textBoxAbnormal_2";
            this.textBoxAbnormal_2.ReadOnly = true;
            this.textBoxAbnormal_2.Size = new System.Drawing.Size(68, 23);
            this.textBoxAbnormal_2.TabIndex = 6;
            // 
            // textBoxAbnormal_1
            // 
            this.textBoxAbnormal_1.Location = new System.Drawing.Point(558, 105);
            this.textBoxAbnormal_1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxAbnormal_1.Name = "textBoxAbnormal_1";
            this.textBoxAbnormal_1.ReadOnly = true;
            this.textBoxAbnormal_1.Size = new System.Drawing.Size(68, 23);
            this.textBoxAbnormal_1.TabIndex = 5;
            // 
            // labelAbnormal_4
            // 
            this.labelAbnormal_4.AutoSize = true;
            this.labelAbnormal_4.Location = new System.Drawing.Point(483, 352);
            this.labelAbnormal_4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAbnormal_4.Name = "labelAbnormal_4";
            this.labelAbnormal_4.Size = new System.Drawing.Size(70, 14);
            this.labelAbnormal_4.TabIndex = 4;
            this.labelAbnormal_4.Text = "异常类别4";
            // 
            // labelAbnormal_3
            // 
            this.labelAbnormal_3.AutoSize = true;
            this.labelAbnormal_3.Location = new System.Drawing.Point(483, 268);
            this.labelAbnormal_3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAbnormal_3.Name = "labelAbnormal_3";
            this.labelAbnormal_3.Size = new System.Drawing.Size(70, 14);
            this.labelAbnormal_3.TabIndex = 3;
            this.labelAbnormal_3.Text = "异常类别3";
            // 
            // labelAbnormal_2
            // 
            this.labelAbnormal_2.AutoSize = true;
            this.labelAbnormal_2.Location = new System.Drawing.Point(483, 189);
            this.labelAbnormal_2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAbnormal_2.Name = "labelAbnormal_2";
            this.labelAbnormal_2.Size = new System.Drawing.Size(70, 14);
            this.labelAbnormal_2.TabIndex = 2;
            this.labelAbnormal_2.Text = "异常类别2";
            // 
            // labelAbnormal_1
            // 
            this.labelAbnormal_1.AutoSize = true;
            this.labelAbnormal_1.Location = new System.Drawing.Point(483, 110);
            this.labelAbnormal_1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAbnormal_1.Name = "labelAbnormal_1";
            this.labelAbnormal_1.Size = new System.Drawing.Size(70, 14);
            this.labelAbnormal_1.TabIndex = 1;
            this.labelAbnormal_1.Text = "异常类别1";
            // 
            // picBoxSampleAbnormal
            // 
            this.picBoxSampleAbnormal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxSampleAbnormal.Location = new System.Drawing.Point(10, 18);
            this.picBoxSampleAbnormal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.picBoxSampleAbnormal.Name = "picBoxSampleAbnormal";
            this.picBoxSampleAbnormal.Size = new System.Drawing.Size(470, 451);
            this.picBoxSampleAbnormal.TabIndex = 0;
            this.picBoxSampleAbnormal.TabStop = false;
            // 
            // picBoxSample
            // 
            this.picBoxSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxSample.Location = new System.Drawing.Point(27, 39);
            this.picBoxSample.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.picBoxSample.Name = "picBoxSample";
            this.picBoxSample.Size = new System.Drawing.Size(568, 479);
            this.picBoxSample.TabIndex = 0;
            this.picBoxSample.TabStop = false;
            // 
            // tabPage5
            // 
            this.tabPage5.Font = new System.Drawing.Font("宋体", 10.5F);
            this.tabPage5.Location = new System.Drawing.Point(22, 4);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(1269, 544);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "显微镜控制区";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // btn_FuncTest
            // 
            this.btn_FuncTest.Location = new System.Drawing.Point(1055, 130);
            this.btn_FuncTest.Name = "btn_FuncTest";
            this.btn_FuncTest.Size = new System.Drawing.Size(165, 46);
            this.btn_FuncTest.TabIndex = 143;
            this.btn_FuncTest.Text = "btn_FuncTest";
            this.btn_FuncTest.UseVisualStyleBackColor = true;
            this.btn_FuncTest.Click += new System.EventHandler(this.btn_FuncTest_Click);
            // 
            // AutoMetal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1282, 657);
            this.Controls.Add(this.自动化);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "AutoMetal";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.自动化.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.outputPicBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Ori_picture)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.dBGridView.ResumeLayout(false);
            this.dBGridView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.batchPictureBox1)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            this.tabPage7.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabPage8.ResumeLayout(false);
            this.tabPage8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSampleAbnormal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSample)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 选择ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uahToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 编辑ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 参数设置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 水平ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关系ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 相关ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 等等ToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.TabControl 自动化;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button btnCloseServer;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label_communcationPort;
        private System.Windows.Forms.TextBox udpPort;
        private System.Windows.Forms.TextBox samplePath;
        private System.Windows.Forms.Label label_sample_path;
        private System.Windows.Forms.TextBox sampleID;
        private System.Windows.Forms.Label label_sample_id;
        private System.Windows.Forms.TextBox expID;
        private System.Windows.Forms.Label label_exp_id;
        private System.Windows.Forms.Button btnAnalysis;
        private System.Windows.Forms.RichTextBox ResCls_RichTextbox;
        private System.Windows.Forms.Label label_pass;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label_uniformity;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox abnormalArea;
        private System.Windows.Forms.TextBox normalArea;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label_coverageRate;
        private System.Windows.Forms.TextBox textEdfStep;
        private System.Windows.Forms.TextBox textEdfRange;
        private System.Windows.Forms.Label label_edf;
        private System.Windows.Forms.CheckBox checkBoxScan;
        private System.Windows.Forms.ComboBox comboBoxAutoScan;
        private System.Windows.Forms.Button snap;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox outputPicBox;
        private System.Windows.Forms.PictureBox Ori_picture;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxInfo;
        private System.Windows.Forms.TextBox textY1;
        private System.Windows.Forms.TextBox textX1;
        private System.Windows.Forms.TextBox textY0;
        private System.Windows.Forms.TextBox textX0;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelx_x1_y1;
        private System.Windows.Forms.Label label_x0_y0;
        private System.Windows.Forms.Button btnAutoScan;
        private System.Windows.Forms.TextBox textFocus;
        private System.Windows.Forms.Button btnAutoFocus;
        private System.Windows.Forms.Label label_focus;
        private System.Windows.Forms.TextBox textZ;
        private System.Windows.Forms.TextBox textXY2;
        private System.Windows.Forms.TextBox textXY1;
        private System.Windows.Forms.Label label7_z;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label_xy;
        private System.Windows.Forms.Button btnSetZ;
        private System.Windows.Forms.Button btnSetXY;
        private System.Windows.Forms.TextBox textContrast;
        private System.Windows.Forms.TextBox textBrightness;
        private System.Windows.Forms.Label label_contrast;
        private System.Windows.Forms.Label label_brightness;
        private System.Windows.Forms.Button btnSetContrast;
        private System.Windows.Forms.Button btnSetBrightness;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_ip;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Label runtimeLabel;
        private System.Windows.Forms.PictureBox batchPictureBox3;
        private System.Windows.Forms.PictureBox batchPictureBox5;
        private System.Windows.Forms.PictureBox batchPictureBox2;
        private System.Windows.Forms.PictureBox batchPictureBox4;
        private System.Windows.Forms.PictureBox batchPictureBox1;
        private System.Windows.Forms.Label batchUniLabel_1;
        private System.Windows.Forms.Label batchCoverLabel_1;
        private System.Windows.Forms.TextBox textConsistency;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label batchUniLabel_3;
        private System.Windows.Forms.Label batchCoverLabel_3;
        private System.Windows.Forms.Label batchUniLabel_5;
        private System.Windows.Forms.Label batchCoverLabel_5;
        private System.Windows.Forms.Label batchUniLabel_2;
        private System.Windows.Forms.Label batchCoverLabel_2;
        private System.Windows.Forms.Label batchUniLabel_4;
        private System.Windows.Forms.Label batchCoverLabel_4;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.PictureBox picBoxSample;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.PictureBox picBoxSampleAbnormal;
        private System.Windows.Forms.Label labelAbnormal_4;
        private System.Windows.Forms.Label labelAbnormal_3;
        private System.Windows.Forms.Label labelAbnormal_2;
        private System.Windows.Forms.Label labelAbnormal_1;
        private System.Windows.Forms.TextBox textBoxAbnormal_4;
        private System.Windows.Forms.TextBox textBoxAbnormal_3;
        private System.Windows.Forms.TextBox textBoxAbnormal_2;
        private System.Windows.Forms.TextBox textBoxAbnormal_1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage dBGridView;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btn_getAllData;
        private System.Windows.Forms.Button btnSearchByDate;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.TextBox textCoverage;
        private System.Windows.Forms.Button btnSearchCoverageAndUniformity;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox textUniformity;
        private System.Windows.Forms.TextBox dbSampleText;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Button btnSampleSearch;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox BatchId;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Button btnSearchByDateAndBatchId;
        private System.Windows.Forms.Button btnCalculateSimilarity;
        private System.Windows.Forms.Button btn_FuncTest;
    }
}