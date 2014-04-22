using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraphTool
{
    class Axis
    {
        static int ParseRange(String str)
        {
            if (str == "min") {
                return int.MinValue;
            } else if (str == "max") {
                return int.MaxValue;
            } else {
                return int.Parse(str);
            }
        }

        public static Axis Parse(String str)
        {
            var split = str.Split(',');
            var axis = new Axis();

            if (split.Length > 0) {
                axis.Label = split[0].Replace("[mu]", "μ");
            }

            if (split.Length > 1) {
                var range = split[1].Split('-');
                if (range.Length == 1) {
                    axis.Maximum = ParseRange(range[0]);
                } else if (range.Length == 2) {
                    axis.Minimum = ParseRange(range[0]);
                    axis.Maximum = ParseRange(range[1]);
                }
            }

            if (split.Length > 2) {
                axis.MajorInterval = int.Parse(split[2]);
            }

            if (split.Length > 3) {
                axis.MinorInterval = int.Parse(split[3]);
            }

            return axis;
        }

        public String Label { get; set; }

        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public int MajorInterval { get; set; }
        public int MinorInterval { get; set; }

        public Axis()
        {
            Label = "";

            Minimum = 0;
            Maximum = 100;
            MajorInterval = 10;
            MinorInterval = 0;
        }
    }

    class DataSet
    {
        static Func<float[], float> ParseColumnExpression(String str)
        {
            str = str.TrimStart();

            Func<float[], float> head = x => 0;

            if (str.Length == 0) return head;

            if (char.IsDigit(str[0])) {
                String number = "";

                while (str.Length > 0 && char.IsDigit(str[0])) {
                    number += str[0];
                    str = str.Substring(1);
                }

                int val = int.Parse(number);

                head = x => val;
            } else if (char.IsLetter(str[0])) {
                int digit = str[0] - 'a';

                head = x => x[digit];
                str = str.Substring(1).TrimStart();
            }

            if (str.Length == 0) return head;
            
            switch (str[0]) {
                case '+':
                    return x => head(x) + ParseColumnExpression(str.Substring(1))(x);
                case '-':
                    return x => head(x) - ParseColumnExpression(str.Substring(1))(x);
                case '*':
                    return x => head(x) * ParseColumnExpression(str.Substring(1))(x);
                case '/':
                    return x => head(x) / ParseColumnExpression(str.Substring(1))(x);
            }

            return head;
        }

        public static DataSet Parse(String str)
        {
            var split = str.Split(',');
            var dataSet = new DataSet();

            dataSet.FilePath = split[0];

            if (split.Length > 1) {
                dataSet.XColumn = ParseColumnExpression(split[1]);
            }

            if (split.Length > 2) {
                dataSet.YColumn = ParseColumnExpression(split[2]);
            }

            if (split.Length > 3) {
                dataSet.Label = split[3];
            }

            if (split.Length > 4) {
                dataSet.Pen = new Pen(Color.FromArgb(int.Parse(split[4], NumberStyles.HexNumber)));
            }

            if (split.Length > 5) {
                dataSet.Pen.Width = float.Parse(split[5]);
            }

            return dataSet;
        }

        public String FilePath { get; set; }
        public String Label { get; set; }

        public Func<float[], float> XColumn { get; set; }
        public Func<float[], float> YColumn { get; set; }

        public Pen Pen { get; set; }

        public PointF[] GetPoints()
        {
            return File.ReadAllLines(FilePath)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0 && !x.StartsWith("#"))
                .Select(x => x.Split(' '))
                .Select(x => x.Select(y => float.Parse(y)).ToArray())
                .Select(x => new PointF(XColumn(x), YColumn(x)))
                .ToArray();
        }
    }

    static class Program
    {
        static void DrawStringCentred(this Graphics ctx, String str, Font font, Brush brush, float x, float y)
        {
            var size = ctx.MeasureString(str, font);

            ctx.DrawString(str, font, brush, x - size.Width / 2f, y - size.Height / 2f);
        }
        static void DrawStringLeftAligned(this Graphics ctx, String str, Font font, Brush brush, float x, float y)
        {
            var size = ctx.MeasureString(str, font);

            ctx.DrawString(str, font, brush, x, y - size.Height / 2f);
        }

        static void DrawStringRightAligned(this Graphics ctx, String str, Font font, Brush brush, float x, float y)
        {
            var size = ctx.MeasureString(str, font);

            ctx.DrawString(str, font, brush, x - size.Width, y - size.Height / 2f);
        }

        static void Main(string[] args)
        {
            string outputPath = "graph.png";

            int imgWidth = 768;
            int imgHeight = 384;

            int margin = 16;

            var xAxis = new Axis();
            var yAxis = new Axis();

            var dataSets = new List<DataSet>();

            for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
                    case "-o":
                    case "--output":
                        outputPath = args[++i];
                        break;
                    case "-w":
                    case "--width":
                        imgWidth = int.Parse(args[++i]);
                        break;
                    case "-h":
                    case "--height":
                        imgHeight = int.Parse(args[++i]);
                        break;
                    case "-x":
                    case "--xaxis":
                        xAxis = Axis.Parse(args[++i]);
                        break;
                    case "-y":
                    case "--yaxis":
                        yAxis = Axis.Parse(args[++i]);
                        break;
                    default:
                        dataSets.Add(DataSet.Parse(args[i]));
                        break;
                }
            }

            if (xAxis.Minimum == int.MinValue) {
                xAxis.Minimum = (int) Math.Floor(dataSets.Min(x => x.GetPoints().Min(y => y.X)));
            }

            if (xAxis.Maximum == int.MaxValue) {
                xAxis.Maximum = (int) Math.Ceiling(dataSets.Max(x => x.GetPoints().Max(y => y.X)));
            }

            if (yAxis.Minimum == int.MinValue) {
                yAxis.Minimum = (int) Math.Floor(dataSets.Min(x => x.GetPoints().Min(y => y.Y)));
            }

            if (yAxis.Maximum == int.MaxValue) {
                yAxis.Maximum = (int) Math.Ceiling(dataSets.Max(x => x.GetPoints().Max(y => y.Y)));
            }

            using (var bmp = new Bitmap(imgWidth, imgHeight)) {
                using (var ctx = Graphics.FromImage(bmp)) {
                    ctx.Clear(Color.White);

                    var labelFont = new Font(FontFamily.GenericSansSerif, 20f);
                    var labelBrush = new SolidBrush(Color.Gray);

                    var intervalFont = new Font(FontFamily.GenericSansSerif, 14f);
                    var intervalBrush = new SolidBrush(Color.Gray);

                    int left = margin + 64;
                    int right = imgWidth - margin;
                    int top = margin;
                    int bottom = imgHeight - margin - 64;

                    int width = right - left;
                    int height = bottom - top;

                    ctx.SmoothingMode = SmoothingMode.AntiAlias;

                    var axisPen = new Pen(Color.DarkGray, 2f);
                    var majorGridPen = new Pen(Color.LightGray, 2f);
                    var minorGridPen = new Pen(Color.LightGray, 1f);

                    ctx.DrawStringCentred(xAxis.Label, labelFont, labelBrush, left + width / 2f, bottom + 56f);

                    ctx.TranslateTransform(left - 56f, top + height / 2f);
                    ctx.RotateTransform(-90f);
                    ctx.DrawStringCentred(yAxis.Label, labelFont, labelBrush, 0f, 0f);
                    ctx.ResetTransform();

                    for (int i = 0; i * xAxis.MajorInterval <= xAxis.Maximum; ++i) {
                        int x = (i * xAxis.MajorInterval * width) / xAxis.Maximum;
                        
                        ctx.DrawLine(majorGridPen, left + x, top, left + x, bottom + 4);
                        ctx.DrawStringCentred((i * xAxis.MajorInterval).ToString(), intervalFont, intervalBrush, left + x, bottom + 16);
                    }

                    for (int i = 1; i * xAxis.MinorInterval <= xAxis.Maximum; ++i) {
                        int x = (i * xAxis.MinorInterval * width) / xAxis.Maximum;
                        ctx.DrawLine(minorGridPen, left + x, top, left + x, bottom);
                    }

                    for (int i = 0; i * yAxis.MajorInterval <= yAxis.Maximum; ++i) {
                        int y = (i * yAxis.MajorInterval * height) / yAxis.Maximum;
                        ctx.DrawLine(majorGridPen, left - 4, bottom - y, right, bottom - y);
                        ctx.DrawStringRightAligned((i * yAxis.MajorInterval).ToString(), intervalFont, intervalBrush, left - 6, bottom - y);
                    }

                    for (int i = 1; i * yAxis.MinorInterval <= yAxis.Maximum; ++i) {
                        int y = (i * yAxis.MinorInterval * height) / yAxis.Maximum;
                        ctx.DrawLine(minorGridPen, left, bottom - y, right, bottom - y);
                    }

                    foreach (var dataSet in dataSets) {
                        var points = dataSet.GetPoints()
                            .Select(p => new PointF(
                                left + ((p.X - xAxis.Minimum) * width) / xAxis.Maximum,
                                top + ((yAxis.Maximum - p.Y + yAxis.Minimum) * height) / yAxis.Maximum))
                            .ToArray();

                        ctx.DrawLines(dataSet.Pen, points);
                    }

                    ctx.DrawLine(axisPen, left, top, left, bottom + 4);
                    ctx.DrawLine(axisPen, left - 4, bottom, right, bottom);

                    int legendTop = top + margin;
                    int legendRight = right - margin;
                    int legendWidth = 56 + (int) Math.Ceiling(dataSets.Max(x => ctx.MeasureString(x.Label, intervalFont).Width));
                    int legendHeight = dataSets.Count * 32 + 16;
                    int legendLeft = legendRight - legendWidth;
                    int legendBottom = legendTop + legendHeight;

                    ctx.FillRectangle(new SolidBrush(Color.FromArgb(191, Color.White)), legendLeft, legendTop, legendWidth, legendHeight);
                    ctx.DrawRectangle(minorGridPen, legendLeft, legendTop, legendWidth, legendHeight);

                    int yPos = legendTop + 8;
                    foreach (var dataSet in dataSets) {
                        ctx.DrawRectangle(minorGridPen, legendLeft + 12f, yPos + 4f, 24f, 24f);
                        ctx.FillRectangle(new SolidBrush(dataSet.Pen.Color), legendLeft + 14f, yPos + 6f, 20f, 20f);
                        ctx.DrawStringLeftAligned(dataSet.Label, intervalFont, labelBrush, legendLeft + 48f, yPos + 16f);
                        yPos += 32;
                    }
                }

                bmp.Save(outputPath);
            }
        }
    }
}
