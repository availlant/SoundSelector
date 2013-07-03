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
        public float[] Frames { get; set; }

        public AudioFile(string fullname)
        {
            Fullname = fullname;
        }
    }
}
