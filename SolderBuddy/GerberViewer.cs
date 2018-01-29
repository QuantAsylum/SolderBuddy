using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace SolderBuddy
{
    class GerberViewer
    {
        public class Aperture
        {
            public int Index;

            public override string ToString()
            {
                return "Aperture Class   Index = " + Index.ToString();
            }
        }

        public class ApertureCircle : Aperture
        {
            public float R;

            public override string ToString()
            {
                return string.Format("ApertureCircle Class   Index = {0}  Radius = {1}", Index, R);
            }
        }

        public class ApertureRectangle : Aperture
        {
            public float Width, Height;

            public override string ToString()
            {
                return string.Format("ApertureCircle Class   Index = {0}  Width = {1} Height = {2}", Index, Width, Height);
            }
        }

        List<Aperture> Apertures = new List<Aperture>();

        public class Op
        {
            public int Code;
            public Aperture Aperture;
            public float X, Y;

            public override string ToString()
            {
                return string.Format("Op Class   Code = {0}  Aperture = {1} X = {2} Y = {3}", Code, Aperture, X, Y);
            }
        }

        public class OpInterp : Op
        {
            public float X0, Y0;

            public override string ToString()
            {
                return string.Format("OpInterp Class   Code = {0}  Aperture = {1} X = {2} Y = {3} X0 = {4} Y0 = {5}", Code, Aperture, X, Y, X0, Y0);
            }
        }

        List<Op> Ops = new List<Op>();

        Aperture CurrentAperature;
        float OldX, OldY;
        float CurrX, CurrY;
        string FormatString = "";
        bool IsMetric = true;

        public GerberViewer()
        {

        }

        public void LoadLayer(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);



            for (int i = 0; i < lines.Length; i++)
            {
                //Debug.WriteLine("Parsing Line: " + i);
                string s = lines[i].ToUpper();

                if (s.Contains("%ADD"))
                {
                    string index = s.SubstringToNonDigit(4);
                    int commaIndex = lines[i].IndexOf(',');
                    char code = s[commaIndex - 1];
                    string size = lines[i].Substring(commaIndex + 1, s.Length - commaIndex - 3);

                    if (code == 'C')
                    {
                        //ConvertToMM(toks[1], FormatString, IsMetric);
                        //float sizeMM = Convert.ToSingle(size) * 25.4f;
                        float sizeMM = ConvertToMM(size, IsMetric);
                        ApertureCircle ac = new ApertureCircle { Index = Convert.ToInt32(index), R = sizeMM / 2 };
                        Apertures.Add(ac);
                        //Debug.WriteLine(string.Format("Added Circle Aperture {0}", ac));
                    }
                    else if (code == 'R')
                    {
                        string[] sizes = size.Split('X');
                        //float sizeXMM = Convert.ToSingle(sizes[0]) * 25.4f;
                        //float sizeYMM = Convert.ToSingle(sizes[1]) * 25.4f;
                        float sizeXMM = ConvertToMM(sizes[0], IsMetric);
                        float sizeYMM = ConvertToMM(sizes[1], IsMetric);
                        ApertureRectangle ar = new ApertureRectangle { Index = Convert.ToInt32(index), Width = sizeXMM, Height = sizeYMM };
                        Apertures.Add(ar);
                        //Debug.WriteLine(string.Format("Added Rect App {0}", ar));
                    }
                }
                else if (s.Contains("%FS"))
                {
                    FormatString = s;
                }
                else if (s.Contains("%MOIN"))
                {
                    IsMetric = false;
                }
                else if (s[0] == 'D')
                {
                    // Process aperature change
                    //int index = Convert.ToInt32( s.Substring(1, 2) );
                    int index = Convert.ToInt32(s.SubstringToNonDigit(1));
                    try
                    {
                        CurrentAperature = Apertures.First(o => o.Index == index);
                    }
                    catch
                    {

                    }
                }
                else if (s[0] == 'X' || s[0] == 'Y')
                {
                    OldX = CurrX;
                    OldY = CurrY;

                    string[] toks = s.Split(new char[] { 'X', 'Y', 'D' });
                    string operation = s.Substring(s.Length - 3, 2);

                    if (s[0] == 'X')
                    {
                        CurrX = ConvertToMM(toks[1], FormatString, IsMetric);

                        if (toks.Length == 4)
                        {
                            //CurrY = Convert.ToSingle(toks[2]) / 1000;
                            CurrY = ConvertToMM(toks[2], FormatString, IsMetric);
                        }
                    }
                    else if (s[0] == 'Y')
                    {
                        CurrY = ConvertToMM(toks[1], FormatString, IsMetric);
                    }

                    if (operation == "D1")
                    {
                        // Linear interp
                        OpInterp oi = new OpInterp { Code = 1, Aperture = CurrentAperature, X = CurrX, Y = CurrY, X0 = OldX, Y0 = OldY };
                        Ops.Add(oi);
                        //Debug.WriteLine("Added OpLin X:{0} Y:{1} X:{2} Y:{3} Aperture:{4}", CurrX, CurrY, OldX, OldY, CurrentAperature);
                    }
                    else if (operation == "D2")
                    {
                        // Move only. Nothing created
                    }
                    else if (operation == "D3")
                    {
                        // Flash operation
                        Op op = new Op { Code = 3, Aperture = CurrentAperature, X = CurrX, Y = CurrY };
                        Ops.Add(op);
                        //Debug.WriteLine("Added OpFlash @ {0}, {1} Aperture: {2}", CurrX, CurrY, CurrentAperature);
                    }
                }
            }
        }

        private float ConvertToMM(string s, bool isMetric)
        {
            float val = Convert.ToSingle(s);

            if (isMetric)
                return val;
            else
                return val * 25.4f;
        }

        // private float ConvertToMM(string s, int wholeDigits, int fractDigs, bool isInches)
        /// <summary>
        /// Converts a string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="formatString"></param>
        /// <param name="isMetric"></param>
        /// <returns></returns>
        private float ConvertToMM(string s, string formatString, bool isMetric)
        {
            bool isNeg = false;
            bool omitLeadingZeros = false;
            int intLen, fracLen;

            if (formatString.Contains("L"))
            {
                // Omit leading zeros
                omitLeadingZeros = true;
            }
            else if (formatString.Contains("T"))
            {
                // Omit trailing zeros
                throw new NotImplementedException("QA Gerber contains Format Specifier T which is not supported");
            }

            string[] formatToks = formatString.Split(new char[] { 'X', 'Y' });
            string format = formatToks[1];
            intLen = (int)Char.GetNumericValue(format[0]);
            fracLen = (int)Char.GetNumericValue(format[1]);

            // Check if negative
            if (s[0] == '-')
            {
                isNeg = true;
                s = s.Substring(1);
            }

            // Pad if needed
            if (omitLeadingZeros)
            {
                s = s.PadLeft(intLen + fracLen, '0');
            }

            float whole = Convert.ToSingle(s.Substring(0, intLen));
            float frac = Convert.ToSingle(s.Substring(intLen, fracLen)) / (float)Math.Pow(10, fracLen);

            float val = (whole + frac);

            if (isMetric == false)
                val = val * 25.4f;

            if (isNeg)
                return -val;
            else
                return val;
        }

        public Bitmap Draw(Color c, bool drawInterpCircles, Bitmap bmp, PointF centerMM, float widthMM)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Brush for all our drawing. The pen will be created based on line width
                Brush b = new SolidBrush(c);

                float dotsXPerMM = g.DpiX / 25.4f; // Compute native dots per mm resolution
                float dotsYPerMM = g.DpiY / 25.4f;
                float heightMM = ((float)bmp.Height/(float)bmp.Width) * widthMM;
                float zoom = bmp.Width / widthMM;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform((-centerMM.X + widthMM/2)  * zoom, (-centerMM.Y + heightMM/2) * zoom);
                g.ScaleTransform(zoom, zoom);

                //Horz and vert cross hair
                g.DrawLine(Pens.Red, centerMM.X - widthMM/10, centerMM.Y, centerMM.X + widthMM/10, centerMM.Y);
                g.DrawLine(Pens.Red, centerMM.X, centerMM.Y - widthMM / 10, centerMM.X, centerMM.Y + widthMM / 10);

                for (int i = 0; i < Ops.Count; i++)
                {
                    if ((Ops[i] is OpInterp) && (Ops[i].Aperture is ApertureRectangle))
                    {
                        // Interpolated rectangular aperture. These aren't generally used at all
                        if (false)
                        {
                            OpInterp obj = (OpInterp)Ops[i];
                            ApertureRectangle rectApp = (ApertureRectangle)Ops[i].Aperture;

                            if (obj.X0 == obj.X)
                            {
                                // Vert interp

                            }
                            else if (obj.Y0 == obj.Y)
                            {
                                // Horz interp
                                for (float x = obj.X; x <= obj.X0; x += (rectApp.Width - rectApp.Width * 0.1f))
                                {
                                    GerberGraphics.Rectangle(g, b, x, obj.Y, rectApp.Width, rectApp.Height);
                                }
                            }
                        }
                    }
                    else if ((Ops[i] is OpInterp) && (Ops[i].Aperture is ApertureCircle))
                    {
                        // Interpolated circular aperture. These are typically used for drawing traces and silk
                        if (drawInterpCircles)
                        {
                            OpInterp obj = (OpInterp)Ops[i];
                            ApertureCircle ac = (ApertureCircle)Ops[i].Aperture;

                            Pen p = new Pen(c, ac.R * 2);
                            GerberGraphics.Line(g, p, obj.X, obj.Y, obj.X0, obj.Y0);
                            p.Dispose();

                            if (obj.X0 == obj.X)
                            {
                                // Vert interp
                                
                            }
                            else if (obj.Y0 == obj.Y)
                            {
                                // Horz interp
                            }
                        }
                    }
                    else if (Ops[i].Code == 3)
                    {
                        // Flash operation
                        if (Ops[i].Aperture is ApertureRectangle)
                        {
                            GerberGraphics.Rectangle(g, b, Ops[i].X, Ops[i].Y, (Ops[i].Aperture as ApertureRectangle).Width, (Ops[i].Aperture as ApertureRectangle).Height);
                        }
                        else if (Ops[i].Aperture is ApertureCircle)
                        {
                            // Only draw circles above a certain size as the smaller ones are likely vias
                            if ((Ops[i].Aperture as ApertureCircle).R > 0.5)
                            {
                                GerberGraphics.Circle(g, b, Ops[i].X, Ops[i].Y, (Ops[i].Aperture as ApertureCircle).R);
                            }
                        }

                    }
                }
            }

            //bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }


        



        public static bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
                   (r.Contains(p1) || r.Contains(p2));
        }

        private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

    }
}
