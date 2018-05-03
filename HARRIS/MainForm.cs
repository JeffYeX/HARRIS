using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HARRIS
{
    public partial class MainForm : Form
    {
        Image _grayImage;

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
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //Image blah = new
                    var inputImage = new Bitmap(ofd.FileName);
                    _grayImage = ToolStripRenderer.CreateDisabledImage(inputImage);
                    pictureBox1.Image = _grayImage;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // get source image size
            var width = _grayImage.Width;
            var height = _grayImage.Height;
            //var srcStride = _grayImage.Stride;
            //var srcOffset = srcStride - width;

            // 1. Calculate partial differences
            var diffx = new float[height, width];
            var diffy = new float[height, width];
            var diffxy = new float[height, width];

            
        }
    }
}
