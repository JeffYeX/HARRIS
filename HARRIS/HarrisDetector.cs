using System;
using System.Collections.Generic;
using Emgu.CV;

namespace HARRIS
{
    class HarrisDetector
    {
        // Harris parameters
        private float _k;
        private float _threshold;
        private readonly int _suppressionParm= 3;

        // Gaussian smoothing parameters
        private const int Size = 3;

        private float[,] _mHarrisResponses;

        #region Constructors

        public HarrisDetector(Matrix<byte> matrixImage, float k, float threshold, bool gaussCheckBox, bool detectEdgesOnly)
        {
            Initialize(k, threshold);
            var resultList = ComputeDerivatives(matrixImage);
            if (gaussCheckBox)
            {
                var mDerivatives = ApplyGaussToDerivatives(resultList, Size);
                var harrisResponses = ComputeHarrisResponses(mDerivatives, detectEdgesOnly);
                _mHarrisResponses = harrisResponses;
            }
            else
            {
                var harrisResponses = ComputeHarrisResponses(resultList, detectEdgesOnly);
                _mHarrisResponses = harrisResponses;
            }
        }

        private void Initialize(float k, float threshold)
        {
            _threshold = threshold;
            _k = k;
        }
        #endregion

        private List<Matrix<float>> ComputeDerivatives(Matrix<byte> matrixImage)
        {
            var verticalSobelM = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            var horizontalSobelM = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);

            for (var r = 1; r < matrixImage.Rows - 2; r++)
            {
                for (var c = 1; c < matrixImage.Cols - 2; c++)
                {
                    var v1 = matrixImage.Data[r - 1, c];
                    var v2 = matrixImage.Data[r, c];
                    var v3 = matrixImage.Data[r + 1, c];
                    verticalSobelM.Data[r, c] = v1 + v2 + v3;

                    var h1 = matrixImage.Data[r, c - 1];
                    var h2 = matrixImage.Data[r, c];
                    var h3 = matrixImage.Data[r, c + 1];
                    horizontalSobelM.Data[r, c] = h1 + h2 + h3;
                }
            }

            var dx = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            var dy = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);
            var dxy = new Matrix<float>(matrixImage.Rows - 1, matrixImage.Cols - 1);

            for (var r = 1; r < matrixImage.Rows - 2; r++)
            {
                for (var c = 1; c < matrixImage.Cols - 2; c++)
                {
                    var h = (horizontalSobelM.Data[r, c] - horizontalSobelM.Data[r + 1, c])*0.166666667f;
                    var v = (verticalSobelM.Data[r, c] - verticalSobelM.Data[r, c + 1])*0.166666667f;
                    dx.Data[r, c] = h*h;
                    dy.Data[r, c] = v*v;
                    dxy.Data[r, c] = h*v;
                }
            }

            var resultList = new List<Matrix<float>> {dx, dy, dxy};

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
            for (var r = size; r < dMats.Rows - size; r++)
            {
                for (var c = size; c < dMats.Cols - size; c++)
                {
                    var res = (float)0.0;

                    for (var x = -size; x <= size; x++)
                    {
                        var m = 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * x * x);

                        res += (float)(m * dMats[r - size, c - size]);
                    }
                    verticalGauss[r - size, c - size] = res;
                }
            }

            for (var r = size; r < dMats.Rows - size; r++)
            {
                for (var c = size; c < dMats.Cols - size; c++)
                {
                    var res = (float)0.0;

                    for (var x = -size; x <= size; x++)
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
            for (var r = 0; r < mDerivatives[1].Rows; r++)
            {
                for (var c = 0; c < mDerivatives[1].Cols; c++)
                {
                    var dx = mDerivatives[0][r, c];
                    var dy = mDerivatives[1][r, c];
                    var dxy = mDerivatives[2][r, c];

                    var det = dxy * dxy;
                    var trace = dx + dy;

                    var edgeMeasure = det - _k * trace * trace;

                    if (detectEdgesOnly && edgeMeasure < 0)
                    {
                        result[r, c] = edgeMeasure;
                    }
                    else if (edgeMeasure > _threshold)
                    {
                        result[r, c] = edgeMeasure;
                    }
                }
            }

            return result;
        }

        public List<int[,]> ReturnEdgePoints()
        {
            var edgesList = new List<int[,]>();
            for (var r = 0; r < _mHarrisResponses.GetLength(0) - 2; r++)
            {
                for (var c = 0; c < _mHarrisResponses.GetLength(1) - 2; c++)
                {
                    if (_mHarrisResponses[r,c] < -_threshold)
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
            for (var y = _suppressionParm; y < _mHarrisResponses.GetLength(0) - _suppressionParm; y++)
            {
                for (var x = _suppressionParm; x < _mHarrisResponses.GetLength(1) - _suppressionParm; x++)
                {
                    var currentValue = _mHarrisResponses[y, x];

                    for (var i = -_suppressionParm; (currentValue != 0.0) && (i <= _suppressionParm); i++)
                    {
                        for (var j = -_suppressionParm; j <= _suppressionParm; j++)
                        {
                            if (!(_mHarrisResponses[y + i, x + j] > currentValue)) continue;
                            currentValue = 0;
                            break;
                        }
                    }

                    if (currentValue != 0.0)
                    {
                        cornersList.Add(new[,] {{x, y}});
                    }
                }
            }

            return cornersList;
        }
    }
}
