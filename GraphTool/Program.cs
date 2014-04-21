using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTool
{
    class Axis
    {
        public static Axis Parse(String str)
        {
            var split = str.Split(',');
            var axis = new Axis();

            if (split.Length > 0) {
                axis.Label = split[0];
            }

            if (split.Length > 1) {
                var range = split[1].Split('-');
                if (range.Length == 1) {
                    axis.Maximum = int.Parse(range[0]);
                } else if (range.Length == 2) {
                    axis.Minimum = int.Parse(range[0]);
                    axis.Maximum = int.Parse(range[1]);
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
        public static DataSet Parse(String str)
        {
            var split = str.Split(',');
            var dataSet = new DataSet();

            dataSet.FilePath = split[0];

            if (split.Length > 1) {
                var format = split[1].Split('-');
                if (format.Length == 1) {
                    dataSet.YColumn = int.Parse(format[0]);
                } else if (format.Length == 2) {
                    dataSet.XColumn = int.Parse(format[0]);
                    dataSet.YColumn = int.Parse(format[1]);
                }
            }

            if (split.Length > 2) {
                dataSet.Label = split[2];
            }

            if (split.Length > 3) {
                dataSet.Pen = new Pen(Color.FromArgb(int.Parse(split[3], NumberStyles.HexNumber)));
            }

            if (split.Length > 4) {
                dataSet.Pen.Width = float.Parse(split[4]);
            }

            return dataSet;
        }

        public String FilePath { get; set; }
        public String Label { get; set; }

        public int XColumn { get; set; }
        public int YColumn { get; set; }

        public Pen Pen { get; set; }

        public PointF[] GetPoints()
        {
            return File.ReadAllLines(FilePath)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0 && !x.StartsWith("#"))
                .Select(x => x.Split(' '))
                .Select(x => new PointF(float.Parse(x[XColumn]), float.Parse(x[YColumn])))
                .ToArray();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int width = 768;
            int height = 384;

            int margin = 8;

            var xAxis = new Axis();
            var yAxis = new Axis();

            var dataSets = new List<DataSet>();

            for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
                    case "-w":
                    case "--width":
                        width = int.Parse(args[++i]);
                        break;
                    case "-h":
                    case "--height":
                        height = int.Parse(args[++i]);
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

            using (var bmp = new Bitmap(width, height)) {
                using (var ctx = Graphics.FromImage(bmp)) {
                    ctx.SmoothingMode = SmoothingMode.HighQuality;

                    ctx.Clear(Color.Transparent);

                    var axisPen = new Pen(Color.DarkGray, 2f);
                    var majorGridPen = new Pen(Color.LightGray, 2f);
                    var minorGridPen = new Pen(Color.LightGray, 1f);

                    for (int i = 1; i * xAxis.MajorInterval <= xAxis.Maximum; ++i) {
                        int x = i * xAxis.MajorInterval * (width - margin * 2) / xAxis.Maximum;
                        ctx.DrawLine(majorGridPen, margin + x, margin, margin + x, height - margin + 4);
                    }

                    for (int i = 1; i * xAxis.MinorInterval <= xAxis.Maximum; ++i) {
                        int x = i * xAxis.MinorInterval * (width - margin * 2) / xAxis.Maximum;
                        ctx.DrawLine(minorGridPen, margin + x, margin, margin + x, height - margin);
                    }

                    for (int i = 1; i * yAxis.MajorInterval <= yAxis.Maximum; ++i) {
                        int y = i * yAxis.MajorInterval * (height - margin * 2) / yAxis.Maximum;
                        ctx.DrawLine(majorGridPen, margin - 4, margin + y, width - margin, margin + y);
                    }

                    for (int i = 1; i * yAxis.MinorInterval <= yAxis.Maximum; ++i) {
                        int y = i * yAxis.MinorInterval * (height - margin * 2) / yAxis.Maximum;
                        ctx.DrawLine(minorGridPen, margin, margin + y, width - margin, margin + y);
                    }

                    foreach (var dataSet in dataSets) {
                        var points = dataSet.GetPoints()
                            .Select(p => new PointF(
                                margin + (p.X - xAxis.Minimum) * (width - margin * 2) / xAxis.Maximum,
                                margin + (yAxis.Maximum - p.Y - yAxis.Minimum) * (height - margin * 2) / yAxis.Maximum))
                            .ToArray();

                        ctx.DrawLines(dataSet.Pen, points);
                    }

                    ctx.DrawLine(axisPen, margin, margin, margin, height - margin);
                    ctx.DrawLine(axisPen, margin, height - margin, width - margin, height - margin);
                }

                bmp.Save("test.png");
            }
        }
    }
}
