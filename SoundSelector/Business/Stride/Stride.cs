﻿using System;
namespace Business.Stride
{
    /// <summary>
    ///   Incremental stride
    /// </summary>
    public class Stride
    {
       private static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));

        private readonly int firstStride;

        public Stride(int minStride, int maxStride, int samplesPerFingerprint)
        {
            Min = minStride;
            Max = maxStride;
            SamplesPerFingerprint = samplesPerFingerprint;
            firstStride = Random.Next(minStride, maxStride);
        }

        public Stride(int minStride, int maxStride, int samplesPerFingerprint, int firstStride)
            : this(minStride, maxStride, samplesPerFingerprint)
        {
            this.firstStride = firstStride;
        }

        /// <summary>
        ///  Gets or sets minimal step between consecutive strides
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        ///   Gets or sets maximal step between consecutive strides
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        ///   Gets or sets number of samples per signature
        /// </summary>
        public int SamplesPerFingerprint { get; set; }

        public int StrideSize
        {
            get
            {
                return -SamplesPerFingerprint + Random.Next(Min, Max);
            }
        }

        public int FirstStrideSize
        {
            get
            {
                return firstStride;
            }
        }
    }
}