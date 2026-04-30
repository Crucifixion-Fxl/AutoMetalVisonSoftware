using System.Drawing;
using System.Windows.Forms;

namespace AutoMetal.Infrastructure
{
    internal interface IImagePreviewService
    {
        bool LoadImage(PictureBox pictureBox, string imagePath);
        void ReplaceImage(PictureBox pictureBox, Image image);
        void ClearImage(PictureBox pictureBox);
    }
}
