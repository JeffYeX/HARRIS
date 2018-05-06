using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HARRIS
{
    class HarrisDetector
    {
        private float k = 0.25f;
        private float threshold = 20000f;
        private float percentage = 0.1f;

        private int maximaSuppressionDimension = 10;

        // Gaussian smoothing parameters
        private int size = 3;

        private float[,] m_harrisResponses;

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisDetector(Image image)
        {
            Initialize(k, threshold);
            //var width = image.Width;
            //var height = image.Height;
            //var srcStride = _grayImage.Stride;
            //var srcOffset = srcStride - width;
            //// 1. Calculate partial differences
            //var diffx = new float[height, width];
            //var diffy = new float[height, width];
            //var diffxy = new float[height, width];
            var resultList = ComputeDerivatives(image);
            var mDerivatives = ApplyGaussToDerivatives(resultList, size);
            var harrisResponses = ComputeHarrisResponses(k, mDerivatives);
            m_harrisResponses = harrisResponses;
        }

        private void Initialize(float k, float threshold)
        {
            this.threshold = threshold;
            this.k = k;
            //this.r = suppression;  , double sigma, int suppression, int size
            //this.sigma = sigma;
            //this.size = size;

            //createGaussian();
            /*
            base.SupportedFormats.UnionWith(new[]
            {
                PixelFormat.Format8bppIndexed,
                PixelFormat.Format24bppRgb,
                PixelFormat.Format32bppRgb,
                PixelFormat.Format32bppArgb
            });
            */
        }
        #endregion

        private List<float[,]> ComputeDerivatives(Image image)
        {
            
            var bitmap = new Bitmap(image);
            var resultList = new List<float[,]> { };
            var vercerticalSobel = new float[bitmap.Height - 2, bitmap.Width];
            //Console.WriteLine(bitmap.Width + "**" + bitmap.Height);
            for (var r = 1; r < bitmap.Height - 1; r++)
            {
                //Console.WriteLine("r:" + r);
                for (var c = 0; c < bitmap.Width; c++)
                {
                    //Console.WriteLine(" c:" + c);
                    float a1 = bitmap.GetPixel(c, r - 1).ToArgb();
                    //Console.WriteLine(" test");
                    float a2 = bitmap.GetPixel(c, r).ToArgb();
                    float a3 = bitmap.GetPixel(c, r + 1).ToArgb();

                    vercerticalSobel[r - 1, c] = a1 + a2 + a2 + a3;
                }
            }

            var horizontalSobel = new float[bitmap.Height, bitmap.Width - 2];
            //Console.WriteLine(bitmap.Width + "**" + bitmap.Height);
            for (var r = 0; r < bitmap.Height; r++)
            {
                //Console.WriteLine("r:" + r);
                for (var c = 1; c < bitmap.Width - 1; c++)
                {
                    //Console.WriteLine(" c:" + c);
                    float a1 = bitmap.GetPixel(c - 1, r).ToArgb();
                    //Console.WriteLine(" test");
                    float a2 = bitmap.GetPixel(c, r).ToArgb();
                    float a3 = bitmap.GetPixel(c + 1, r).ToArgb();

                    horizontalSobel[r, c - 1] = a1 + a2 + a2 + a3;
                }
            }

            var dx = new float[bitmap.Height - 2, bitmap.Width - 2];
            var dy = new float[bitmap.Height - 2, bitmap.Width - 2];
            var dxy = new float[bitmap.Height - 2, bitmap.Width - 2];

            for (int r = 0; r < bitmap.Height - 2; r++)
            {
                for (int c = 0; c < bitmap.Width - 2; c++)
                {
                    dx[r, c] = horizontalSobel[r, c] - horizontalSobel[r + 2, c];
                    dy[r, c] = vercerticalSobel[r, c] - vercerticalSobel[r, c + 2];
                    dxy[r, c] = dx[r, c] * dy[r, c];
                }
            }

            resultList.Add(dx);
            resultList.Add(dy);
            resultList.Add(dxy);

            return resultList;
        }

        private List<float[,]> ApplyGaussToDerivatives(List<float[,]> dMats, int size)
        {
            if (size == 0)
            {
                return dMats;
            }
            var resultList = new List<float[,]> { };

            resultList.Add(GaussFilter(dMats[0], size));
            resultList.Add(GaussFilter(dMats[1], size));
            resultList.Add(GaussFilter(dMats[2], size));

            return resultList;
        }

        private float[,] GaussFilter(float[,] dMats, int size)
        {
            var gaussResult = new float[dMats.GetLength(0) - size * 2, dMats.GetLength(1) - size * 2];
            var verticalGauss = new float[dMats.GetLength(0) - size * 2, dMats.GetLength(1) - size * 2];
            for (int r = size; r < dMats.GetLength(0) - size; r++)
            {
                for (int c = size; c < dMats.GetLength(1) - size; c++)
                {
                    var res = (float)0.0;

                    for (int x = -size; x <= size; x++)
                    {
                        var m = 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * x * x);

                        res += (float)(m * dMats[r - size, c - size]);
                    }
                    verticalGauss[r - size, c - size] = res;
                }
            }

            for (int r = size; r < dMats.GetLength(0) - size; r++)
            {
                for (int c = size; c < dMats.GetLength(1) - size; c++)
                {
                    var res = (float)0.0;

                    for (int x = -size; x <= size; x++)
                    {
                        var m = 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * x * x);

                        res += (float)(m * verticalGauss[r - size, c - size]);
                    }
                    gaussResult[r - size, c - size] = res;
                }
            }

            return gaussResult;
        }

        private float[,] ComputeHarrisResponses(float k, List<float[,]> mDerivatives)
        {
            var result = new float[mDerivatives[1].GetLength(0), mDerivatives[0].GetLength(1)];
            for (int r = 0; r < mDerivatives[1].GetLength(0); r++)
            {
                for (int c = 0; c < mDerivatives[1].GetLength(1); c++)
                {
                    var a11 = mDerivatives[0][r, c] * mDerivatives[0][r, c];
                    var a22 = mDerivatives[1][r, c] * mDerivatives[1][r, c];
                    var a21 = mDerivatives[0][r, c] * mDerivatives[1][r, c];
                    var a12 = mDerivatives[0][r, c] * mDerivatives[1][r, c];

                    var det = a11 * a22 - a12 * a21;
                    var trace = a11 + a22;

                    //var M = Math.Abs(det - k * trace * trace);

                    
                    result[r, c] = Math.Abs(det - k * trace * trace);
                }
            }

            return result;
        }

        public float[,] ReturnHarris()
        {
            return m_harrisResponses;
        }

        public List<Point> GetMaximaPoints(float percentage, float size, int maximaSuppressionDimension)
        {
            var topPoint = new List<Point> { };
            var maximaSuppressionMat = new int[m_harrisResponses.GetLength(0), m_harrisResponses.GetLength(1)];


            


            //Sort<float>(ToJagged(m_harrisResponses), 2);
            Console.WriteLine("sdsd");
            for (int r = 0; r < m_harrisResponses.GetLength(0); r++)
            {
                for (int c = 0; c < m_harrisResponses.GetLength(1); c++)
                {
                    var p = new Point(c, r);
                    
                }
            }


            return topPoint;
        }

        private void Sort<T>(T[][] data, int col)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            Array.Sort<T[]>(data, (x, y) => comparer.Compare(x[col], y[col]));
        }

        private float[][] ToJagged(float[,] array)
        {
            int height = array.GetLength(0), width = array.GetLength(1);
            float[][] jagged = new float[height][];

            for (int i = 0; i < height; i++)
            {
                float[] row = new float[width];
                for (int j = 0; j < width; j++)
                {
                    row[j] = array[i, j];
                }
                jagged[i] = row;
            }
            return jagged;
        }
    }
}
