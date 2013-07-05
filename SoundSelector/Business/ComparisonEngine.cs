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

        #region Constantes

        /// <summary>
        ///   Number of threshold votes for a file to be considerate a duplicate
        /// </summary>
        private const int _thresholdVotes = 5;

        /// <summary>
        ///   Value of threshold percentage of fingerprints that needs to be gathered
        ///   in order to be considered a possible result
        /// </summary>
        private const int _thresholdFingerprintsToVote = 7;

        #endregion

        public ComparisonEngine(string file, string folder)
        {
            this._file = file;
            this._folder = folder;

            files.AddRange(FolderManager.GetMusicFiles(folder));            
        }

        public void GetFingerprints(FingerprintsGenerator fpGenerator)
        {    
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
            FingerprintsGenerator fpGenerator = new FingerprintsGenerator();

            GetFingerprints(fpGenerator);

            List<List<string>> doublons = new List<List<string>>();

            //Parcours des fichiers
            foreach (var fileWithFingerprints in fpGenerator.Fingerprints)
            {
                Dictionary<string, int> fileDoublons = new Dictionary<string, int>();

                //Parcours des tableaux de signatures
                foreach (long[] fingerprint in fileWithFingerprints.Value)
                {
                    Dictionary<string, int> signatureMatch = fpGenerator.GetTracks(fileWithFingerprints.Key, fingerprint, _thresholdVotes);

                    foreach (var file in signatureMatch)
                    {
                        if (fileDoublons.ContainsKey(file.Key)) fileDoublons[file.Key]++; else fileDoublons[file.Key] = 1;
                    }
                }

                List<string> listDoublons =  fileDoublons.Where(x => x.Value > _thresholdFingerprintsToVote).Select(x => x.Key).ToList<string>();

                if (listDoublons.Count > 0)
                {
                    listDoublons.Add(fileWithFingerprints.Key);
                    doublons.Add(listDoublons);
                }
            }
        }
    }
}
