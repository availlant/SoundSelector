﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Fingerprints
{
    public class AbsComparator : IComparer<float>
    {
        #region IComparer<float> Members

        public int Compare(float x, float y)
        {
            return Math.Abs(y).CompareTo(Math.Abs(x));
        }

        #endregion
    }

    public class Fingerprints
    {
        AbsComparator abs = new AbsComparator();

        /// <summary>
        /// Sets all other wavelet values to 0 except whose which make part of Top Wavelet [top wavelet &gt; 0 ? 1 : -1]
        /// </summary>
        /// <param name="frames">
        /// Frames with 32 logarithmically spaced frequency bins
        /// </param>
        /// <param name="topWavelets">
        /// The top Wavelets.
        /// </param>
        /// <returns>
        /// Signature signature. Array of encoded Boolean elements (wavelet signature)
        /// </returns>
        /// <remarks>
        ///   Negative Numbers = 01
        ///   Positive Numbers = 10
        ///   Zeros            = 00
        /// </remarks>
        public bool[] ExtractTopWavelets(float[][] frames, int topWavelets)
        {
            int rows = frames.GetLength(0); /*128*/
            int cols = frames[0].Length; /*32*/
            float[] concatenated = new float[rows * cols]; /* 128 * 32 */
            for (int row = 0; row < rows; row++)
            {
                Array.Copy(frames[row], 0, concatenated, row * frames[row].Length, frames[row].Length);
            }

            int[] indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            Array.Sort(concatenated, indexes, abs);
            bool[] result = EncodeFingerprint(concatenated, indexes, topWavelets);
            return result;
        }

        /// <summary>
        ///   Encode the integer representation of the fingerprint into a Boolean array
        /// </summary>
        /// <param name = "concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name = "indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name = "topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        public bool[] EncodeFingerprint(float[] concatenated, int[] indexes, int topWavelets)
        {
            bool[] result = new bool[concatenated.Length * 2]; // Concatenated float array
            for (int i = 0; i < topWavelets; i++)
            {
                int index = indexes[i];
                double value = concatenated[i];
                if (value > 0)
                {
                    // positive wavelet
                    result[index * 2] = true;
                }
                else if (value < 0)
                {
                    // negative wavelet
                    result[(index * 2) + 1] = true;
                }
            }

            return result;
        }

        /// <summary>
        ///   Decode the signature of the fingerprint
        /// </summary>
        /// <param name = "signature">Signature to be decoded</param>
        /// <returns>Array of doubles with positive [10], negatives [01], and zeros [00]</returns>
        public double[] DecodeFingerprint(bool[] signature)
        {
            int len = signature.Length / 2;
            double[] result = new double[len];
            for (int i = 0; i < len * 2; i += 2)
            {
                if (signature[i])
                {
                    // positive if first is true
                    result[i / 2] = 1;
                }
                else if (signature[i + 1])
                {
                    // negative if second is true
                    result[i / 2] = -1;
                }

                // otherwise '0'
            }

            return result;
        }
    }
}
