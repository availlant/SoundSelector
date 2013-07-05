using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Model;
using Business.Stride;

namespace Business.FingerprintsCreation
{
    public class Spectrum
    {
        Stride.Stride strideBetweenConsecutiveImages = new Stride.Stride(5115, 128 * 64);//64 = overlap; 128 = fingerprintsLenght

        int _sampleRate;

        const int minFrequency = 318;
        const int maxFrequency = 2000;

        public Spectrum(int sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public void CreateLogSpectrogram(AudioFile file)
        {
            //2048 samples each 64 samples
            //Donc WdftSize = 2048 et Overlap = 64
            int width = (file.Data.Length - 2048) / 64;

            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * 2048]; /*even - Re, odd - Img, thats how Exocortex works*/
            int[] logFrequenciesIndexes = GenerateLogFrequencies();
            for (int i = 0; i < width; i++)
            {               
                // take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < 2048; j++)
                {
                    complexSignal[2 * j] = (float) file.Data[i * 64 + j];
                    complexSignal[(2 * j) + 1] = 0;
                }
                // FFT Transform
                Fourier.FFT(complexSignal, 2048, FourierDirection.Forward);
                // Band Filtering
                frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndexes, 32);
            }

            file.Frames = frames;
        }

        public void CutLogarithmizedSpectrum(AudioFile file)
        {
            int start = strideBetweenConsecutiveImages.FirstStrideSize / 64;
            int logarithmicBins = file.Frames[0].Length;
            List<float[][]> spectralImages = new List<float[][]>();

            int width = file.Frames.GetLength(0);

            while (start + 128 < width)
            {
                float[][] spectralImage = AllocateMemoryForFingerprintImage(128, logarithmicBins);
                for (int i = 0; i < 128; i++)
                {
                    Array.Copy(file.Frames[start + i], spectralImage[i], logarithmicBins);
                }

                start += 128 + (strideBetweenConsecutiveImages.StrideSize / 64);
                spectralImages.Add(spectralImage);
            }

            file.SpectralImages = spectralImages;
        }

        private float[] ExtractLogBins(float[] spectrum, int[] logFrequenciesIndex, int logBins)
        {
            int width = spectrum.Length / 2;
            float[] sumFreq = new float[logBins]; /*32*/
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = logFrequenciesIndex[i];
                int higherBound = logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < higherBound; k++)
                {
                    double re = spectrum[2 * k] / ((float)width / 2);
                    double img = spectrum[(2 * k) + 1] / ((float)width / 2);
                    sumFreq[i] += (float)((re * re) + (img * img));
                }

                sumFreq[i] /= higherBound - lowBound;
            }

            return sumFreq;
        }

        private int[] GenerateLogFrequencies()
        {
            double logBase =
                Math.Exp(
                    Math.Log((float)maxFrequency / minFrequency) / 32);
            double mincoef = (float)2048 / _sampleRate * minFrequency;
            int[] indexes = new int[32 + 1];
            for (int j = 0; j < 32 + 1; j++)
            {
                int start = (int)((Math.Pow(logBase, j) - 1.0) * mincoef);
                indexes[j] = start + (int)mincoef;
            }

            return indexes;
        }

        private float[][] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
        {
            float[][] frames = new float[fingerprintLength][];
            for (int i = 0; i < fingerprintLength; i++)
            {
                frames[i] = new float[logBins];
            }

            return frames;
        }
    }
}
