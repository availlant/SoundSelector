using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.FingerprintsCreation;
using Business.Model;

namespace Business
{
    public class ComparisonEngine
    {
        private string _file;
        private string _folder;

        List<string> files = new List<string>();

        public ComparisonEngine(string file, string folder)
        {
            this._file = file;
            this._folder = folder;

            files.AddRange(FolderManager.GetMusicFiles(folder));
        }

        public void GetFingerprints()
        {
            FingerprintsGenerator fpGenerator = new FingerprintsGenerator();            

            foreach (string path in files)
            {
                try
                {
                    fpGenerator.GenerateFingerprints(path);                    
                }
                catch (Exception e)
                {
                }
            }
        }

        public void Compare()
        {
            GetFingerprints();
        }
    }
}
