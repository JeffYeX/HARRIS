﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;

namespace HARRIS
{
    class HarrisDetector
    {
        private float k;
        private float threshold;
        //private float percentage = 0.1f;

        //private int maximaSuppressionDimension = 10;

        private readonly int suppressionParm= 3;

        // Gaussian smoothing parameters
        private readonly int size = 3;

        private float[,] m_harrisResponses;

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisDetector(Image image, Matrix<byte> matrixImage, float k, float threshold, bool gaussCheckBox, bool detectEdgesOnly)
        {
            Initialize(k, threshold);
            //var width = image.Width;
            //var height = image.Height;
            //var srcStride = _grayImage.Stride;
            //var srcOffset = srcStride - width;
            //// 1. Calculate partial differences`
            //var diffx = new float[height, width];
            //var diffy = new float[height, width];
            //var diffxy = new float[height, width];
            var resultList = ComputeDerivatives(image, matrixImage);
            if (gaussCheckBox)
            {
                var mDerivatives = ApplyGaussToDerivatives(resultList, size);
                var harrisResponses = ComputeHarrisResponses(mDerivatives, detectEdgesOnly);
                m_harrisResponses = harrisResponses;
            }
            else
            {
                var harrisResponses = ComputeHarrisResponses(resultList, detectEdgesOnly);
                m_harrisResponses = harrisResponses;
            }
            //var mDerivatives = ApplyGaussToDerivatives(resultList, size);
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

        private List<Matrix<float>> ComputeDerivatives(Image image, Matrix<byte> matrixImage)
        {
            var verticalSobelM = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            var horizontalSobelM = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            //verticalSobelM.Data[0, 0] = 0;
            //horizontalSobelM.Data[0, 0] = 0;

            for (var r = 1; r < matrixImage.Rows - 2; r++)
            {
                //Console.WriteLine("r:" + r);
                for (var c = 1; c < matrixImage.Cols - 2; c++)
                {
                    //Console.WriteLine(" c:" + c);
                    var v1 = matrixImage.Data[r - 1, c];
                    //Console.WriteLine(" test");
                    var v2 = matrixImage.Data[r, c];
                    var v3 = matrixImage.Data[r + 1, c];
                    verticalSobelM.Data[r, c] = v1 + v2 + v3;

                    var h1 = matrixImage.Data[r, c - 1];
                    //Console.WriteLine(" test");
                    var h2 = matrixImage.Data[r, c];
                    var h3 = matrixImage.Data[r, c + 1];
                    horizontalSobelM.Data[r, c] = h1 + h2 + h3;
                }
            }
            /*
            //var horizontalSobelM = new Matrix<float>(matrixImage.Rows, matrixImage.Cols - 2);
            for (var r = 0; r < matrixImage.Rows; r++)
            {
                //Console.WriteLine("r:" + r);
                for (var c = 1; c < matrixImage.Cols - 1; c++)
                {
                    //Console.WriteLine(" c:" + c);
                    var a1 = matrixImage.Data[r, c - 1];
                    //Console.WriteLine(" test");
                    var a2 = matrixImage.Data[r, c];
                    var a3 = matrixImage.Data[r, c + 1];

                    horizontalSobelM.Data[r, c - 1] = a1 + a2 + a3;
                }
            }
            */
            var dx = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            var dy = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            var dxy = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);

            for (int r = 1; r < matrixImage.Rows - 2; r++)
            {
                for (int c = 1; c < matrixImage.Cols - 2; c++)
                {
                    //Console.WriteLine("r:" + r + "c:" + c);
                    var h = (horizontalSobelM.Data[r, c] - horizontalSobelM.Data[r + 1, c])*0.166666667f;
                    var v = (verticalSobelM.Data[r, c] - verticalSobelM.Data[r, c + 1])*0.166666667f;
                    dx.Data[r, c] = h*h;
                    dy.Data[r, c] = v*v;
                    dxy.Data[r, c] = h*v;
                }
            }

            var resultList = new List<Matrix<float>> {dx, dy, dxy};


            /*
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
            */
            return resultList;
        }


        private List<Matrix<float>> ApplyGaussToDerivatives(List<Matrix<float>> dMats, int size)
        {
            if (size == 0)
            {
                return dMats;
            }
            var resultList = new List<Matrix<float>>
            {
                GaussFilter(dMats[0], size),
                GaussFilter(dMats[1], size),
                GaussFilter(dMats[2], size)
            };


            return resultList;
        }

        private Matrix<float> GaussFilter(Matrix<float> dMats, int size)
        {
            
            var gaussResult = new Matrix<float>(dMats.Rows - size * 2, dMats.Cols - size * 2);
            var verticalGauss = new Matrix<float>(dMats.Rows - size * 2, dMats.Cols - size * 2);
            for (int r = size; r < dMats.Rows - size; r++)
            {
                for (int c = size; c < dMats.Cols - size; c++)
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

            for (int r = size; r < dMats.Rows - size; r++)
            {
                for (int c = size; c < dMats.Cols - size; c++)
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

        private float[,] ComputeHarrisResponses(List<Matrix<float>> mDerivatives, bool detectEdgesOnly)
        {
            var result = new float[mDerivatives[1].Rows, mDerivatives[0].Cols];
            for (int r = 0; r < mDerivatives[1].Rows; r++)
            {
                for (int c = 0; c < mDerivatives[1].Cols; c++)
                {
                    var dx = mDerivatives[0][r, c];// * mDerivatives[0][r, c];
                    var dy = mDerivatives[1][r, c];// * mDerivatives[1][r, c];
                    var dxy = mDerivatives[2][r, c];
                    //var a12 = mDerivatives[0][r, c] * mDerivatives[1][r, c];

                    var det = dx * dy;// - dxy * dxy;
                    var trace = dx + dy;

                    //var M = Math.Abs(det - k * trace * trace);

                    var edgeMeasure = det - k * trace * trace;
                    //Console.WriteLine(edgeMeasure);
                    if (detectEdgesOnly && edgeMeasure < 0)
                    {
                        result[r, c] = edgeMeasure;
                    }
                    else if (edgeMeasure > threshold)
                    {
                        result[r, c] = edgeMeasure;
                    }
                    //result[r, c] = Math.Abs(det - k * trace * trace);
                }
            }

            return result;
        }

        public List<int[,]> ReturnEdgePoints()
        {
            var edgesList = new List<int[,]>();
            for (int r = 0; r < m_harrisResponses.GetLength(0) - 2; r++)
            {
                for (int c = 0; c < m_harrisResponses.GetLength(1) - 2; c++)
                {
                    if (m_harrisResponses[r,c] < -threshold)
                    {
                        edgesList.Add(new[,] {{c, r}});
                    }
                }
            }
            return edgesList;
        }

        public List<int[,]> GetMaximaPoints()
        {
            var cornersList = new List<int[,]>();
            //var maximaSuppressionMat = new int[m_harrisResponses.GetLength(0), m_harrisResponses.GetLength(1)];
            for (int y = suppressionParm; y < m_harrisResponses.GetLength(0) - suppressionParm; y++)
            {
                for (int x = suppressionParm; x < m_harrisResponses.GetLength(1) - suppressionParm; x++)
                {
                    var currentValue = m_harrisResponses[y, x];

                    for (int i = -suppressionParm; (currentValue != 0.0) && (i <= suppressionParm); i++)
                    {
                        for (int j = -suppressionParm; j <= suppressionParm; j++)
                        {
                            if (m_harrisResponses[y + i, x + j] > currentValue)
                            {
                                currentValue = 0;
                                break;
                            }
                        }
                    }

                    if (currentValue != 0)
                    {
                        cornersList.Add(new[,] {{x, y}});
                    }
                }
            }

            

            /*
            //Sort<float>(ToJagged(m_harrisResponses), 2);
            Console.WriteLine("sdsd");
            for (int r = 0; r < m_harrisResponses.GetLength(0); r++)
            {
                for (int c = 0; c < m_harrisResponses.GetLength(1); c++)
                {
                    var p = new Point(c, r);
                    
                }
            }
            */

            return cornersList;
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
