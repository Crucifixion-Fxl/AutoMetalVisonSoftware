using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace AutoMetal {

    public static class PictureBoxHelper
    {
        /// <summary>
        /// 启用图片交互功能（缩放、平移、标注）
        /// </summary>
        public static void EnableImageInteraction(PictureBox pictureBox, Image image,
            bool enableZoom = true, bool enablePan = true, bool enableAnnotations = true)
        {
            var state = new PictureBoxState
            {
                OriginalImage = image,
                DisplayImage = (Image)image.Clone(),
                ZoomFactor = 1.0f,
                ImageOffset = Point.Empty,
                Annotations = new List<Annotation>(),
                AnnotationPen = new Pen(Color.Red, 2),
                IsDrawing = false
            };

            pictureBox.Tag = state;
            pictureBox.Image = state.DisplayImage;
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;

            // 事件处理
            pictureBox.MouseWheel += (s, e) => { if (enableZoom) HandleMouseWheel(pictureBox, e); };
            pictureBox.MouseDown += (s, e) =>
            {
                if (enablePan && e.Button == MouseButtons.Left) HandleMouseDown(pictureBox, e);
                if (enableAnnotations && e.Button == MouseButtons.Right) StartAnnotation(pictureBox, e);
            };
            pictureBox.MouseMove += (s, e) =>
            {
                if (enablePan && e.Button == MouseButtons.Left) HandleMouseMove(pictureBox, e);
                if (enableAnnotations && state.IsDrawing) PreviewAnnotation(pictureBox, e);
            };
            pictureBox.MouseUp += (s, e) =>
            {
                if (enablePan) HandleMouseUp(pictureBox, e);
                if (enableAnnotations && e.Button == MouseButtons.Right) CompleteAnnotation(pictureBox, e);
            };
            pictureBox.Paint += (s, e) =>
            {
                if (enableAnnotations) DrawAnnotations(pictureBox, e);
            };
        }

        /// <summary>
        /// 清除所有标注
        /// </summary>
        public static void ClearAnnotations(PictureBox pictureBox)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                state.Annotations.Clear();
                RedrawImage(pictureBox);
            }
        }

        /// <summary>
        /// 重置视图
        /// </summary>
        public static void ResetView(PictureBox pictureBox)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                state.ZoomFactor = 1.0f;
                state.ImageOffset = Point.Empty;
                RedrawImage(pictureBox);
            }
        }

        #region 私有实现

        private class PictureBoxState
        {
            public Image OriginalImage { get; set; }
            public Image DisplayImage { get; set; }
            public float ZoomFactor { get; set; }
            public Point ImageOffset { get; set; }
            public List<Annotation> Annotations { get; set; }
            public Pen AnnotationPen { get; set; }
            public bool IsDrawing { get; set; }
            public Point StartPoint { get; set; }
            public Annotation CurrentAnnotation { get; set; }
        }

        private class Annotation
        {
            public RectangleF OriginalRect { get; set; }  // 在原始图像坐标系中的位置
            public Pen Pen { get; set; }
        }

        private static void HandleMouseWheel(PictureBox pictureBox, MouseEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                float zoomDelta = e.Delta > 0 ? 0.2f : -0.2f;
                state.ZoomFactor = Math.Max(0.1f, Math.Min(5f, state.ZoomFactor + zoomDelta));
                RedrawImage(pictureBox);
            }
        }

        private static void HandleMouseDown(PictureBox pictureBox, MouseEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                state.StartPoint = e.Location;
                pictureBox.Cursor = Cursors.Hand;
            }
        }

        private static void HandleMouseMove(PictureBox pictureBox, MouseEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state && e.Button == MouseButtons.Left)
            {
                state.ImageOffset = new Point(
                    state.ImageOffset.X + (e.X - state.StartPoint.X),
                    state.ImageOffset.Y + (e.Y - state.StartPoint.Y));
                state.StartPoint = e.Location;
                RedrawImage(pictureBox);
            }
        }

        private static void HandleMouseUp(PictureBox pictureBox, MouseEventArgs e)
        {
            pictureBox.Cursor = Cursors.Default;
        }

        private static void StartAnnotation(PictureBox pictureBox, MouseEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                state.IsDrawing = true;

                // 转换为原始图像坐标
                var originalPoint = DisplayToOriginal(pictureBox, e.Location);

                state.CurrentAnnotation = new Annotation
                {
                    OriginalRect = new RectangleF(originalPoint.X, originalPoint.Y, 0, 0),
                    Pen = (Pen)state.AnnotationPen.Clone()
                };
            }
        }

        private static void PreviewAnnotation(PictureBox pictureBox, MouseEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state && state.IsDrawing)
            {
                // 转换为原始图像坐标
                var currentPoint = DisplayToOriginal(pictureBox, e.Location);
                var startPoint = state.CurrentAnnotation.OriginalRect.Location;

                // 更新当前标注的矩形
                state.CurrentAnnotation.OriginalRect = new RectangleF(
                    Math.Min(startPoint.X, currentPoint.X),
                    Math.Min(startPoint.Y, currentPoint.Y),
                    Math.Abs(currentPoint.X - startPoint.X),
                    Math.Abs(currentPoint.Y - startPoint.Y));

                pictureBox.Invalidate();
            }
        }

        private static void CompleteAnnotation(PictureBox pictureBox, MouseEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state && state.IsDrawing)
            {
                if (state.CurrentAnnotation.OriginalRect.Width > 5 &&
                    state.CurrentAnnotation.OriginalRect.Height > 5)
                {
                    state.Annotations.Add(state.CurrentAnnotation);
                }
                state.IsDrawing = false;
                state.CurrentAnnotation = null;
                RedrawImage(pictureBox);
            }
        }

        // ... 保留之前的所有其他方法 ...

        /// <summary>
        /// 在PictureBox内部右上角添加控制按钮
        /// </summary>
        public static void AddInternalControls(PictureBox pictureBox)
        {
            // 确保PictureBox可以包含其他控件
            pictureBox.Controls.Clear();

            // 创建重置按钮
            var btnReset = new Button
            {
                Text = "reset",
                Size = new Size(60, 25),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
                Font = new Font("Microsoft Sans Serif", 8.25f) // 设置字体和大小
            };
            btnReset.FlatAppearance.BorderSize = 1;
            btnReset.Click += (s, e) => ResetView(pictureBox);

            // 创建清除按钮
            var btnClear = new Button
            {
                Text = "clear",
                Size = new Size(60, 25),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
                Font = new Font("Microsoft Sans Serif", 8.25f) // 设置字体和大小
            };
            btnClear.FlatAppearance.BorderSize = 1;
            btnClear.Click += (s, e) => ClearAnnotations(pictureBox);

            // 使用TableLayoutPanel来布局按钮
            var panel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // 设置列样式
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

            // 添加按钮
            panel.Controls.Add(btnClear, 0, 0);
            panel.Controls.Add(btnReset, 1, 0);

            // 将面板添加到PictureBox
            pictureBox.Controls.Add(panel);

            // 设置面板位置（右上角）
            panel.Location = new Point(
                pictureBox.Width - panel.Width - 5,
                5);

            // 确保按钮在最前面
            panel.BringToFront();

            // 当PictureBox大小改变时重新定位面板
            pictureBox.SizeChanged += (s, e) =>
            {
                panel.Location = new Point(
                    pictureBox.Width - panel.Width - 5,
                    5);
            };
        }

        // 更新按钮位置的方法
        private static void UpdateButtonPosition(PictureBox pictureBox, Button btnReset, Button btnClear)
        {
            // 计算右上角位置
            int right = pictureBox.Right - 10;
            int top = pictureBox.Top + 10;

            // 设置按钮位置
            btnReset.Location = new Point(right - btnReset.Width, top);
            btnClear.Location = new Point(right - btnReset.Width - btnClear.Width - 5, top);
        }

        private static void DrawAnnotations(PictureBox pictureBox, PaintEventArgs e)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                // 绘制已完成的标注
                foreach (var annotation in state.Annotations)
                {
                    var rect = OriginalToDisplay(pictureBox, annotation.OriginalRect);
                    e.Graphics.DrawRectangle(annotation.Pen, rect);
                }

                // 绘制当前正在创建的标注
                if (state.IsDrawing && state.CurrentAnnotation != null)
                {
                    var rect = OriginalToDisplay(pictureBox, state.CurrentAnnotation.OriginalRect);
                    using (var dashPen = (Pen)state.CurrentAnnotation.Pen.Clone())
                    {
                        dashPen.DashStyle = DashStyle.Dash;
                        e.Graphics.DrawRectangle(dashPen, rect);
                    }
                }
            }
        }

        private static void RedrawImage(PictureBox pictureBox)
        {
            if (pictureBox.Tag is PictureBoxState state)
            {
                // 释放之前的显示图像
                state.DisplayImage?.Dispose();

                // 创建缩放后的图像
                int newWidth = (int)(state.OriginalImage.Width * state.ZoomFactor);
                int newHeight = (int)(state.OriginalImage.Height * state.ZoomFactor);

                var zoomedImage = new Bitmap(newWidth, newHeight);
                using (var g = Graphics.FromImage(zoomedImage))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(state.OriginalImage, 0, 0, newWidth, newHeight);
                }

                // 创建最终显示图像
                var displayImage = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (var g = Graphics.FromImage(displayImage))
                {
                    g.Clear(Color.White);
                    g.DrawImage(zoomedImage, state.ImageOffset.X, state.ImageOffset.Y);
                }

                state.DisplayImage = displayImage;
                pictureBox.Image = displayImage;
                zoomedImage.Dispose();

                pictureBox.Invalidate();
            }
        }

        // 坐标转换：显示坐标 -> 原始图像坐标
        private static PointF DisplayToOriginal(PictureBox pictureBox, Point displayPoint)
        {
            if (!(pictureBox.Tag is PictureBoxState state)) return PointF.Empty;

            // 减去偏移量，然后除以缩放因子
            return new PointF(
                (displayPoint.X - state.ImageOffset.X) / state.ZoomFactor,
                (displayPoint.Y - state.ImageOffset.Y) / state.ZoomFactor);
        }

        // 坐标转换：原始图像坐标 -> 显示坐标
        private static Rectangle OriginalToDisplay(PictureBox pictureBox, RectangleF originalRect)
        {
            if (!(pictureBox.Tag is PictureBoxState state)) return Rectangle.Empty;

            // 乘以缩放因子，然后加上偏移量
            return new Rectangle(
                (int)(originalRect.X * state.ZoomFactor) + state.ImageOffset.X,
                (int)(originalRect.Y * state.ZoomFactor) + state.ImageOffset.Y,
                (int)(originalRect.Width * state.ZoomFactor),
                (int)(originalRect.Height * state.ZoomFactor));
        }

        #endregion
    }
}