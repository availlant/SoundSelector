using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Model
{
    public class AudioFile
    {
        public string Fullname { get; set; }

        public float[] Data { get; set; }
        public float[][] Frames { get; set; }
        public List<float[][]> SpectralImages { get; set; }
        public List<bool[]> Fingerprints { get; set; }
        public List<long[]> Signatures { get; set; }

        public AudioFile(string fullname)
        {
            Fullname = fullname;
        }
    }
}
