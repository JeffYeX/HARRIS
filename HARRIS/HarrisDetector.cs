using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HARRIS
{
    class HarrisDetector
    {
        private float k = 0.04f;
        private float threshold = 20000f;

        // Non-maximum suppression parameters
        private int r = 3;

        // Gaussian smoothing parameters
        private double sigma = 1.2;
        private float[] kernel;
        private int size = 7;

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisDetector()
        {
            initialize(k, threshold, sigma, r, size);
        }


        private void initialize(float k, float threshold, double sigma, int suppression, int size)
        {
            this.threshold = threshold;
            this.k = k;
            this.r = suppression;
            this.sigma = sigma;
            this.size = size;

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
    }
}
