using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        Action<double> _reportProgress;

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

        public ComparisonEngine(string file, string folder, Action<double> reportProgress)
        {
            this._file = file;
            this._folder = folder;
            this._reportProgress = reportProgress;

            if (!String.IsNullOrWhiteSpace(file))
                files.Add(_file);
            files.AddRange(FolderManager.GetMusicFiles(folder));
        }

        public void GetFingerprints(FingerprintsGenerator fpGenerator)
        {
            int nbFiles = files.Count;
            int curseur = 0;

            foreach (string path in files)
            {
                try
                {
                    fpGenerator.GenerateFingerprints(path);
                }
                catch (Exception e)
                {
                    Debug.Write("Erreur génération des empreintes : " + e.Message);
                }

                curseur++;
                _reportProgress(nbFiles != 0 ? (double)curseur * 100 / nbFiles : 100);
            }
        }

        /// <summary>
        /// Recherche des doublons d'un fichier
        /// </summary>
        /// <returns></returns>
        public List<string> Compare()
        {
            FingerprintsGenerator fpGenerator = new FingerprintsGenerator();

            GetFingerprints(fpGenerator);

            var fileWithFingerprints = fpGenerator.Fingerprints.Where(x => x.Key == _file).First();

            Dictionary<string, int> fileDoublons = new Dictionary<string, int>();

            //Parcours des tableaux de signatures
            foreach (long[] fingerprint in fileWithFingerprints.Value)
            {
                Dictionary<string, int> signatureMatch = fpGenerator.GetTracks(fileWithFingerprints.Key, fingerprint, _thresholdVotes);

                foreach (var file in signatureMatch)
                {
                    if (fileDoublons.ContainsKey(file.Key))
                        fileDoublons[file.Key]++;
                    else
                        fileDoublons[file.Key] = 1;
                }
            }

            List<string> listDoublons = fileDoublons.Where(x => x.Value > _thresholdFingerprintsToVote).Select(x => x.Key).ToList<string>();

            return listDoublons;
        }

        /// <summary>
        /// Recherche des doublons de lanière générale
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> CompareAll()
        {
            FingerprintsGenerator fpGenerator = new FingerprintsGenerator();

            GetFingerprints(fpGenerator);

            Dictionary<string, List<string>> doublons = new Dictionary<string, List<string>>();

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
                        //doublons en un seul exemplaire : si A doublon de B, ne pas ajouter B doublon de A
                        if (!doublons.ContainsKey(file.Key))
                        {
                            if (fileDoublons.ContainsKey(file.Key))
                                fileDoublons[file.Key]++;
                            else
                                fileDoublons[file.Key] = 1;
                        }
                    }
                }

                List<string> listDoublons = fileDoublons.Where(x => x.Value > _thresholdFingerprintsToVote).Select(x => x.Key).ToList<string>();

                if (listDoublons.Count > 0)
                {
                    doublons.Add(fileWithFingerprints.Key, listDoublons);
                }
            }

            return doublons;
        }
    }
}
