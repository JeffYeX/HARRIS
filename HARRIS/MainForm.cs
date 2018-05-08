using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() != DialogResult.OK) return;
                //Image blah = new
                var inputImage = new Bitmap(ofd.FileName);
                //var matImage = new Mat(new Size(inputImage.Width, inputImage.Height), DepthType.Cv32F, 1);
                var matImage = CvInvoke.Imread(ofd.FileName, ImreadModes.Grayscale);
                    
                //_matrixImage = new Matrix<float>(matImage.Rows, matImage.Cols);
                //matImage.CopyTo(_matrixImage);
                _matrixImage = new Matrix<byte>(matImage.Rows, matImage.Cols);
                matImage.CopyTo(_matrixImage);
                _grayImage = MakeGrayscale3(inputImage);
                //_grayImage = ToolStripRenderer.CreateDisabledImage(inputImage);
                pictureBox1.Image = _grayImage;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // get source image size
            //var width = _grayImage.Width;
            //var height = _grayImage.Height;
            //var srcStride = _grayImage.Stride;
            //var srcOffset = srcStride - width;

            // 1. Calculate partial differences
            //var diffx = new float[height, width];
            //var diffy = new float[height, width];
            //var diffxy = new float[height, width];
            //float k = 0.25f;
            //_g.Clear(Color.Transparent);

            //pictureBox1.Image = _grayImage;
            var k = (float) numK.Value;
            var threshold = (float) numThreshold.Value;
            //float threshold = 200000000f;
            //float percentage = 0.1f;
            _g = Graphics.FromImage(_grayImage);
            //_g.Clear(Color.Transparent);
            
            //pictureBox1.Image = _grayImage;
            //int maximaSuppressionDimension = 10;
            //int size = 3;

            var harris = new HarrisDetector(_grayImage, _matrixImage, k, threshold);

            //var blah = harris.ReturnHarris();
            /*
            Graphics _g = Graphics.FromImage(_grayImage);
            for (int i = 0; i < resPts.Count; i++)
            {
                for (int c = 0; c < resPts[]; c++)
                {
                    if (blah[i, c] > 1.0 * Math.Pow(10, 28))
                    {

                        PaintCross(_g, new Point(i, c));
                    }
                }
            }
            */
            //var resPts = harris.GetMaximaPoints(percetage, size, maximaSuppressionDimension);
            var resPts = harris.GetMaximaPoints();
            
            foreach (var point in resPts)
            {
                PaintCross(_g, new Point(point[0,0], point[0,1]));
            }
            //pictureBox1.Image = _grayImage;
            Refresh();
            //pictureBox1.Image = _grayImage;
        }

        private Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);
            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });
            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();
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
