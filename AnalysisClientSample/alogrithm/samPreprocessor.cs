using OpenCvSharp;
using SAMViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Runtime.InteropServices;
using OxyPlot;

namespace ImageAnalysis
{
    public  class samProcessor
    {


        public static void getImgageRes(string imagePath)
        {

            RectAnnotation mCurRectAnno = new RectAnnotation();
            Stack<Promotion> mUndoStack = new Stack<Promotion>();
            Stack<Promotion> mRedoStack = new Stack<Promotion>();
            List<Promotion> mPromotionList = new List<Promotion>();


            SAM mSam = SAM.Instance();

            mSam.LoadONNXModel();

            float[] mImgEmbedding;

            OpenCvSharp.Mat image = OpenCvSharp.Cv2.ImRead(imagePath, OpenCvSharp.ImreadModes.Color);


            mImgEmbedding = mSam.Encode(image, image.Width, image.Height);

            SAMAutoMask mAutoMask = new SAMAutoMask();

            mAutoMask.mImgEmbedding = mImgEmbedding;

            mAutoMask.mSAM = mSam;

            //image.Dispose();
            

            // 前面需要停止
            BoxPromotion prompt = new BoxPromotion();
            (prompt as BoxPromotion).mLeftUp.X = 0;
            (prompt as BoxPromotion).mLeftUp.Y = 0;
            (prompt as BoxPromotion).mRightBottom.X = 1292;
            (prompt as BoxPromotion).mRightBottom.Y = 649;


            Transforms ts = new Transforms(1024);

            var pb = ts.ApplyBox(prompt, image.Width, image.Height);

            pb.mAnation = mCurRectAnno;

            mUndoStack.Push(pb);
            mPromotionList.Add(pb);

            //Thread thread = new Thread(() =>
            //{
            //    try
            //    {
            MaskData mask_ = mSam.Decode(mPromotionList, mImgEmbedding, image.Width, image.Height);

            float[] mask = mask_.mMask.ToArray();

            WriteableBitmap bp = new WriteableBitmap(image.Width, image.Height, 96, 96, PixelFormats.Pbgra32, null);

            // 设置像素数据，将所有像素的透明度设置为半透明
            byte[] pixelData = new byte[image.Width * image.Height * 4];

            Array.Clear(pixelData, 0, pixelData.Length);

            Color color = Color.FromArgb((byte)100, (byte)255, (byte)0, (byte)0);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int ind = y * image.Width + x;
                    if (mask[ind] > mSam.mask_threshold)
                    {
                        pixelData[4 * ind] = color.B;  // Blue
                        pixelData[4 * ind + 1] = color.G;  // Green
                        pixelData[4 * ind + 2] = color.R;  // Red
                        pixelData[4 * ind + 3] = 50;  // Alpha
                    }
                }
            }

            bp.WritePixels(new Int32Rect(0, 0, image.Width, image.Height), pixelData, image.Width * 4, 0);
            // 创建一个BitmapImage对象，将WriteableBitmap作为源
            //mask_.Source = bp;
            SaveBitmapToFile(bp, @"C:\Users\SOW111\Desktop\test_mask.png");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Error in mask processing: {ex.Message}");
                //    }
                //});

                //thread.Start();

            

        }

        public static void SaveBitmapToFile(WriteableBitmap bitmap, string filename)
        {
            try
            {
                // 创建编码器
                PngBitmapEncoder encoder = new PngBitmapEncoder();


                // 将 WriteableBitmap 转换为 BitmapFrame 并添加到编码器
                BitmapFrame frame = BitmapFrame.Create(bitmap);
                encoder.Frames.Add(frame);

                // 创建文件流并保存
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                Console.WriteLine($"Mask saved to: {Path.GetFullPath(filename)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving mask: {ex.Message}");
            }
        }


    }

}
