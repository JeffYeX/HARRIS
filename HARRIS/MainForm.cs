using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace HARRIS
{
    public partial class MainForm : Form
    {
        private Image _grayImage;
        private Matrix<byte> _matrixImage;
        private Graphics _g;
        private OpenFileDialog _ofd;
        private List<int[,]> _resPts;

        public MainForm()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ofd = new OpenFileDialog();
            if (_ofd.ShowDialog() != DialogResult.OK) return;
            var inputImage = new Bitmap(_ofd.FileName);
            var matImage = CvInvoke.Imread(_ofd.FileName, ImreadModes.Grayscale);
            _matrixImage = new Matrix<byte>(matImage.Rows, matImage.Cols);
            matImage.CopyTo(_matrixImage);
            _grayImage = MakeGrayscale3(inputImage);
            pictureBox1.Image = _grayImage;
        }

        private void savePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var spd = new SaveFileDialog
            {
                Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "Points",
                DefaultExt = "txt"
            };
            if (spd.ShowDialog() != DialogResult.OK) return;
            using (var bw = new StreamWriter(File.Create(spd.FileName)))
            {
                foreach (var points in _resPts)
                {
                    bw.Write("X: {0}, Y: {1}\r\n", points[0,0], points[0,1]);
                }
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sid = new SaveFileDialog
            {
                Filter = @"jpg files (*.jpg)|*.jpg|All files (*.*)|*.*",
                FileName = "Image",
                DefaultExt = "jpg"
            };
            if (sid.ShowDialog() != DialogResult.OK) return;
            _grayImage.Save(sid.FileName);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var k = (float) numK.Value;
            var threshold = (float) numThreshold.Value;
            var gaussCheckBox = checkBox1.Checked;
            var detectEdgesOnly = checkBox2.Checked;

            var harris = new HarrisDetector(_matrixImage, k, threshold, gaussCheckBox, detectEdgesOnly);
            _resPts = detectEdgesOnly ? harris.ReturnEdgePoints() : harris.GetMaximaPoints();

            _g = Graphics.FromImage(_grayImage);
            foreach (var point in _resPts)
            {
                PaintCross(_g, new Point(point[0,0], point[0,1]));
            }
            Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _grayImage = MakeGrayscale3(new Bitmap(_ofd.FileName));
            pictureBox1.Image = _grayImage;
            Refresh();
        }

        private Bitmap MakeGrayscale3(Bitmap original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);
            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);
            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });
            //create some image attributes
            var attributes = new ImageAttributes();
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);
            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private void PaintCross(Graphics g, Point loc)
        {
            //Half length of the line.
            const int HALF_LEN = 2;

            //Draw horizontal line.

            Point p1 = new Point(loc.X - HALF_LEN, loc.Y);

            Point p2 = new Point(loc.X + HALF_LEN, loc.Y);

            g.DrawLine(Pens.Red, p1, p2);

            //Draw the vertical line.

            p1 = new Point(loc.X, loc.Y - HALF_LEN);

            p2 = new Point(loc.X, loc.Y + HALF_LEN);

            g.DrawLine(Pens.Red, p1, p2);

        }
    }
}
