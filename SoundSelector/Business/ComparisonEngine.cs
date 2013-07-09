using System;
using System.Collections.Generic;
using System.Configuration;
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

        private List<string> files = new List<string>();

        private Action<double> _reportProgress;

        #region Constantes

        /*/// <summary>
        ///   Number of threshold votes for a file to be considerate a duplicate
        /// </summary>
        private const int _thresholdVotes = 5;

        /// <summary>
        ///   Value of threshold percentage of fingerprints that needs to be gathered
        ///   in order to be considered a possible result
        /// </summary>
        private const int _thresholdFingerprintsToVote = 7;*/

        /// <summary>
        ///   Number of threshold votes for a file to be considerate a duplicate
        /// </summary>
        private int _thresholdVotes;
        /// <summary>
        ///   Value of threshold percentage of fingerprints that needs to be gathered
        ///   in order to be considered a possible result
        /// </summary>
        private int _thresholdFingerprintsToVote;

        #endregion

        /// <summary>
        /// Crée une nouvelle instance de la classe ComparisonEngine. Cette classe permet la recherche de doublons dans un dossier "Folder" (méthode CompareAll)
        /// ou la recherche des doublons d'un fichier "File" dans un dossier "Folder" (méthode Compare)
        /// </summary>
        /// <param name="file">File</param>
        /// <param name="folder">Folder</param>
        /// <param name="reportProgress">Le traitement s'éxecute de manière asynchrone, reportProgress sera appelée à chaque fois qu'un fichier audio aura été analysé</param>
        public ComparisonEngine(string file, string folder, Action<double> reportProgress)
        {
            this._file = file;
            this._folder = folder;
            this._reportProgress = reportProgress;

            int i;
            _thresholdVotes = int.TryParse(ConfigurationSettings.AppSettings["thresholdVotes"], out i) ? i : 5;
            _thresholdFingerprintsToVote = int.TryParse(ConfigurationSettings.AppSettings["thresholdFingerprintsToVote"], out i) ? i : 7;

            if (!String.IsNullOrWhiteSpace(file))
                files.Add(_file);
            files.AddRange(FolderManager.GetMusicFiles(folder));
        }

        private void GetFingerprints(FingerprintsGenerator fpGenerator)
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
        /// Recherche les doublons du fichier "File" dans le dossier "Folder"
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
        /// Recherche les doublons dans le dossier "Folder"
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> CompareAll()
        {
            FingerprintsGenerator fpGenerator = new FingerprintsGenerator();

            GetFingerprints(fpGenerator);

            Dictionary<string, List<string>> doublons = new Dictionary<string, List<string>>();
            List<string> filesAlreadyFound = new List<string>();

            //Parcours des fichiers
            foreach (var fileWithFingerprints in fpGenerator.Fingerprints)
            {
                if (filesAlreadyFound.Contains(fileWithFingerprints.Key))
                    continue;

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

                if (listDoublons.Count > 0)
                {
                    doublons[fileWithFingerprints.Key] = new List<string>(listDoublons);
                    filesAlreadyFound.Add(fileWithFingerprints.Key);
                    filesAlreadyFound.AddRange(listDoublons);
                }
            }

            return doublons;
        }
    }
}
