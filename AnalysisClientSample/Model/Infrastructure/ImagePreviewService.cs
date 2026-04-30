using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal class ImagePreviewService : IImagePreviewService
    {
        private readonly Action<string> _warning;

        public ImagePreviewService(Action<string> warningCallback)
        {
            _warning = warningCallback ?? (_ => { });
        }

        public bool LoadImage(PictureBox pictureBox, string imagePath)
        {
            if (pictureBox == null || string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                return false;
            }

            try
            {
                using (var image = Image.FromFile(imagePath))
                {
                    ReplaceImage(pictureBox, new Bitmap(image));
                }
                return true;
            }
            catch (Exception ex)
            {
                _warning($"图像加载失败: {ex.Message}");
                return false;
            }
        }

        public void ReplaceImage(PictureBox pictureBox, Image image)
        {
            if (pictureBox == null)
            {
                image?.Dispose();
                return;
            }

            ClearImage(pictureBox);
            pictureBox.Image = image;
        }

        public void ClearImage(PictureBox pictureBox)
        {
            if (pictureBox?.Image == null)
            {
                return;
            }

            pictureBox.Image.Dispose();
            pictureBox.Image = null;
        }
    }
}
