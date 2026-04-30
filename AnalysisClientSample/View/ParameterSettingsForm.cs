using System;
using System.Drawing;
using System.Windows.Forms;
using AutoMetal.Shared;

namespace AutoMetal
{
    public class ParameterSettingsForm : Form
    {
        private readonly TextBox txtModelCls = new TextBox();
        private readonly TextBox txtModelSeg = new TextBox();
        private readonly TextBox txtManualFolder = new TextBox();
        private readonly TextBox txtAutoFolder = new TextBox();
        private readonly TextBox txtScaleWidth = new TextBox();
        private readonly TextBox txtScaleHeight = new TextBox();
        private readonly TextBox txtClipTop = new TextBox();
        private readonly TextBox txtClipBottom = new TextBox();
        private readonly TextBox txtClipLeft = new TextBox();
        private readonly TextBox txtClipRight = new TextBox();

        public ParameterSettingsForm()
        {
            Text = "参数设置";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 760;
            Height = 500;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 11,
                Padding = new Padding(12),
                AutoScroll = true
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            AddPathFileRow(panel, 0, "分类模型路径", txtModelCls);
            AddPathFileRow(panel, 1, "分割模型路径", txtModelSeg);
            AddPathFolderRow(panel, 2, "手动图像目录", txtManualFolder);
            AddPathFolderRow(panel, 3, "自动图像目录", txtAutoFolder);
            AddRow(panel, 4, "scale_width", txtScaleWidth);
            AddRow(panel, 5, "scale_height", txtScaleHeight);
            AddRow(panel, 6, "clipTop", txtClipTop);
            AddRow(panel, 7, "clipBottom", txtClipBottom);
            AddRow(panel, 8, "clipLeft", txtClipLeft);
            AddRow(panel, 9, "clipRight", txtClipRight);

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            var btnSave = new Button { Text = "保存", Width = 100 };
            var btnCancel = new Button { Text = "取消", Width = 100 };
            btnSave.Click += (_, __) => SaveSettings();
            btnCancel.Click += (_, __) => Close();
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnCancel);
            panel.Controls.Add(btnPanel, 0, 10);
            panel.SetColumnSpan(btnPanel, 3);

            Controls.Add(panel);
            LoadCurrentSettings();
        }

        private static void AddRow(TableLayoutPanel panel, int row, string label, TextBox textBox)
        {
            var lbl = new Label
            {
                Text = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            textBox.Dock = DockStyle.Fill;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(textBox, 1, row);
            panel.Controls.Add(new Label { Dock = DockStyle.Fill }, 2, row);
        }

        private static void AddPathFileRow(TableLayoutPanel panel, int row, string label, TextBox textBox)
        {
            AddRow(panel, row, label, textBox);
            var btnBrowse = new Button
            {
                Text = "浏览...",
                Dock = DockStyle.Fill
            };
            btnBrowse.Click += (_, __) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Engine files (*.engine)|*.engine|All files (*.*)|*.*";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox.Text = dialog.FileName;
                    }
                }
            };
            var placeholder = panel.GetControlFromPosition(2, row);
            if (placeholder != null)
            {
                panel.Controls.Remove(placeholder);
                placeholder.Dispose();
            }
            panel.Controls.Add(btnBrowse, 2, row);
        }

        private static void AddPathFolderRow(TableLayoutPanel panel, int row, string label, TextBox textBox)
        {
            AddRow(panel, row, label, textBox);
            var btnBrowse = new Button
            {
                Text = "浏览...",
                Dock = DockStyle.Fill
            };
            btnBrowse.Click += (_, __) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        dialog.SelectedPath = textBox.Text;
                    }
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox.Text = dialog.SelectedPath;
                    }
                }
            };
            var placeholder = panel.GetControlFromPosition(2, row);
            if (placeholder != null)
            {
                panel.Controls.Remove(placeholder);
                placeholder.Dispose();
            }
            panel.Controls.Add(btnBrowse, 2, row);
        }

        private void LoadCurrentSettings()
        {
            var cfg = AutoMetalConstants.GetCurrentParameterSettings();
            txtModelCls.Text = cfg.ModelPathCls;
            txtModelSeg.Text = cfg.ModelPathSeg;
            txtManualFolder.Text = cfg.ManualFolderPath;
            txtAutoFolder.Text = cfg.AutoFolderPath;
            txtScaleWidth.Text = cfg.ScaleWidth.ToString();
            txtScaleHeight.Text = cfg.ScaleHeight.ToString();
            txtClipTop.Text = cfg.ClipTop.ToString();
            txtClipBottom.Text = cfg.ClipBottom.ToString();
            txtClipLeft.Text = cfg.ClipLeft.ToString();
            txtClipRight.Text = cfg.ClipRight.ToString();
        }

        private void SaveSettings()
        {
            if (!TryGetNonNegativeInt(txtScaleWidth.Text, out int scaleWidth) || scaleWidth <= 0 ||
                !TryGetNonNegativeInt(txtScaleHeight.Text, out int scaleHeight) || scaleHeight <= 0 ||
                !TryGetNonNegativeInt(txtClipTop.Text, out int clipTop) ||
                !TryGetNonNegativeInt(txtClipBottom.Text, out int clipBottom) ||
                !TryGetNonNegativeInt(txtClipLeft.Text, out int clipLeft) ||
                !TryGetNonNegativeInt(txtClipRight.Text, out int clipRight))
            {
                MessageBox.Show(this, "请检查数值参数，必须是有效的非负整数（宽高需大于0）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AutoMetalConstants.SaveUserSettings(new AutoMetalConstants.ParameterSettings
            {
                ModelPathCls = txtModelCls.Text.Trim(),
                ModelPathSeg = txtModelSeg.Text.Trim(),
                ManualFolderPath = txtManualFolder.Text.Trim(),
                AutoFolderPath = txtAutoFolder.Text.Trim(),
                ScaleWidth = scaleWidth,
                ScaleHeight = scaleHeight,
                ClipTop = clipTop,
                ClipBottom = clipBottom,
                ClipLeft = clipLeft,
                ClipRight = clipRight
            });

            MessageBox.Show(this, "参数已保存并生效", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private static bool TryGetNonNegativeInt(string input, out int value)
        {
            return int.TryParse(input, out value) && value >= 0;
        }
    }
}
