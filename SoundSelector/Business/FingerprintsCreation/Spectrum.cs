﻿using System;
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
    }
}