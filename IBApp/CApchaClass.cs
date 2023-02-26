using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media.Imaging;
using System.IO;

namespace IBApp
{
    internal class CApchaClass
    {
        internal CApchaClass()
        {
            text = RandomString(6);
        }

        private string text;

        public BitmapImage Capcha { get { return Convert(MakeCaptchaImge(text)); } }

        private Random Rand = new Random();

        private BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        // Создаем изображение для текста.
        private Bitmap MakeCaptchaImge(string txt,
            int min_size = 50, int max_size = 100, int wid = 400, int hgt = 100)
        {
            // Создаем растровое изображение и связанный с ним объект Graphics.
            Bitmap bm = new Bitmap(wid, hgt);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.Clear(Color.White);

                // Посмотрите, сколько места доступно для каждого персонажа.
                int ch_wid = (int)(wid / txt.Length);

                // Рисуем каждый символ.
                for (int i = 0; i < txt.Length; i++)
                {
                    float font_size = Rand.Next(min_size, max_size);
                    using (Font the_font = new Font("Times New Roman",
                        font_size, FontStyle.Bold))
                    {
                        DrawCharacter(txt.Substring(i, 1), gr,
                            the_font, i * ch_wid, ch_wid, wid, hgt);
                    }
                }
            }

            return bm;
        }

        private int PreviousAngle = 0;
        private void DrawCharacter(string txt, Graphics gr,
            Font the_font, int X, int ch_wid, int wid, int hgt)
        {
            // Центрируем текст.
            using (StringFormat string_format = new StringFormat())
            {
                string_format.Alignment = StringAlignment.Center;
                string_format.LineAlignment = StringAlignment.Center;
                RectangleF rectf = new RectangleF(X, 0, ch_wid, hgt);

                // Преобразование текста в путь.
                using (GraphicsPath graphics_path = new GraphicsPath())
                {
                    graphics_path.AddString(txt,
                        the_font.FontFamily, (int)(the_font.Style),
                        the_font.Size, rectf, string_format);

                    // Произвольные случайные параметры деформации.
                    float x1 = (float)(X + Rand.Next(ch_wid) / 2);
                    float y1 = (float)(Rand.Next(hgt) / 2);
                    float x2 = (float)(X + ch_wid / 2 +
                        Rand.Next(ch_wid) / 2);
                    float y2 = (float)(hgt / 2 + Rand.Next(hgt) / 2);
                    PointF[] pts = {
            new PointF(
                (float)(X + Rand.Next(ch_wid) / 4),
                (float)(Rand.Next(hgt) / 4)),
            new PointF(
                (float)(X + ch_wid - Rand.Next(ch_wid) / 4),
                (float)(Rand.Next(hgt) / 4)),
            new PointF(
                (float)(X + Rand.Next(ch_wid) / 4),
                (float)(hgt - Rand.Next(hgt) / 4)),
            new PointF(
                (float)(X + ch_wid - Rand.Next(ch_wid) / 4),
                (float)(hgt - Rand.Next(hgt) / 4))
        };
                    Matrix mat = new Matrix();
                    graphics_path.Warp(pts, rectf, mat,
                        WarpMode.Perspective, 0);

                    // Поворачиваем бит случайным образом.
                    float dx = (float)(X + ch_wid / 2);
                    float dy = (float)(hgt / 2);
                    gr.TranslateTransform(-dx, -dy, MatrixOrder.Append);
                    int angle = PreviousAngle;
                    do
                    {
                        angle = Rand.Next(-30, 30);
                    } while (Math.Abs(angle - PreviousAngle) < 20);
                    PreviousAngle = angle;
                    gr.RotateTransform(angle, MatrixOrder.Append);
                    gr.TranslateTransform(dx, dy, MatrixOrder.Append);

                    gr.FillPath(Brushes.Purple, graphics_path);
                    gr.ResetTransform();
                }
            }
        }

        internal bool isCorrect(string text_)
        {
            return text_ == text;
        }

        private string RandomString(int length)
        {
            const string chars = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }
    }
}
