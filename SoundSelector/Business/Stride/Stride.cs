namespace Business.Stride
{
    /// <summary>
    ///   Incremental stride
    /// </summary>
    public class Stride
    {
        private readonly int incrementBy;

        private readonly int firstStride;

        public Stride(int incrementBy, int samplesInFingerprint)
        {
            this.incrementBy = -samplesInFingerprint + incrementBy; /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
            firstStride = 0;
        }

        public Stride(int incrementBy, int samplesInFingerprint, int firstStride)
            : this(incrementBy, samplesInFingerprint)
        {
            this.firstStride = firstStride;
        }

        public int StrideSize
        {
            get
            {
                return incrementBy;
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