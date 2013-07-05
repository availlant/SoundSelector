using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.HaarWavelet;
using Business.Model;

namespace Business.FingerprintsCreation
{
    public class WaveletDecomposition
    {
        HaarWaveletDecomposition waveletDecomposition = new HaarWaveletDecomposition();

        public void Transform(AudioFile file)
        {
            Parallel.ForEach(
                file.SpectralImages,
                image => waveletDecomposition.DecomposeImageInPlace(image));
        }
    }
}
