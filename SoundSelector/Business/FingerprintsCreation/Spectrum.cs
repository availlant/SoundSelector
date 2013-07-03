using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Model;

namespace Business.FingerprintsCreation
{
    public class Spectrum
    {
        int _sampleRate;

        const int minFrequency = 318;
        const int maxFrequency = 2000;

        public Spectrum(int sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public float[][] CreateLogSpectrogram(AudioFile file)
        {
            //2048 samples each 64 samples
            //Donc WdftSize = 2048 et Overlap = 64
            int width = (file.Data.Length - 2048) / 64;

            float[][] frames = new float[width][];
            int[] logFrequenciesIndexes = GenerateLogFrequencies();
            for (int i = 0; i < width; i++)
            {
                // FFT Transform
                float[] complexSignal = fftService.FFTForward(samples, i * 64, 2048);
                // Band Filtering
                frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndexes, configuration.LogBins);
            }

            file.Frames = frames;
        }

        private int[] GenerateLogFrequenciesDynamicBase()
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
    }
}
