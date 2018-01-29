using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace SolderBuddy
{
    static class GerberGraphics
    {
        public static void Circle(Graphics g, Brush b, float x, float y, float radius)
        {
            g.FillEllipse(b, x - radius, y - radius, 2 * radius, 2 * radius);
        }

        public static void Rectangle(Graphics g, Brush b, double x, double y, double w, double h)
        {
            g.FillRectangle(b, (float)(x - w / 2), (float)(y - h / 2), (float)w, (float)h);
        }

        public static void Line(Graphics g, Pen p, float x0, float y0, float x1, float y1)
        {
            g.DrawLine(p, x0, y0, x1, y1);
        }


        /// <summary>
        /// Blends two bitmaps
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Bitmap BlendBitmaps(Bitmap image1, Bitmap image2)
        {
            Bitmap result = new Bitmap(image1);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.DrawImage(image2, 0, 0);
                   
            }

            return result;
        }

        public static void FlipY(Bitmap image)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
    }
}
