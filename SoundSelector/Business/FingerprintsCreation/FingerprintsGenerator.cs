using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Model;

namespace Business.FingerprintsCreation
{
    public class FingerprintsGenerator
    {
        #region Constantes

        /*
         * D'après Ciumac Sergiu, qui a détaillé l'algo ici http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting,
         * ces constantes sont à définir de manière empirique selon nos besoins, notamment les variables Treshold... pour définir le taux de marge d'erreur
         * que l'on autorise.
         * J'ai gardé les valeurs qu'il a choisies
         */

        // 1024 (Kb) * BufferSize / SampleRate * SecondsRead * 4 (1 float = 4 bytes) / 1024 (Kb)
        const int Buffersize =
            (int)((1024.0 * BufferSize) / ((double)SampleRate * SecondsToProcess / 1000 * 4 / 1024));

        /// <summary>
        ///   Maximum track length (track's bigger than this value will be discarded)
        /// </summary>
        private const int MaxTrackLength = 60 * 10; /*10 min - maximal track length*/

        /// <summary>
        ///   Number of seconds to process from each song
        /// </summary>
        private const int SecondsToProcess = 10;

        /// <summary>
        ///   Starting processing point
        /// </summary>
        private const int StartProcessingAtSecond = 20;

        /// <summary>
        ///   Buffer size of the application reading songs
        /// </summary>
        /// <remarks>
        ///   Represented in MB.
        ///   Max 100MB will be reserved for the samples read from songs
        /// </remarks>
        private const int BufferSize = 100;

        /// <summary>
        ///   Minimum track length (track's less than this value will be discarded)
        /// </summary>
        private const int MinTrackLength = SecondsToProcess + StartProcessingAtSecond + 1;

        /// <summary>
        ///   Number of LSH tables
        /// </summary>
        private const int NumberOfHashTables = 25;

        /// <summary>
        ///   Number of Min Hash keys per 1 hash function (1 LSH table)
        /// </summary>
        private const int NumberOfKeys = 4;
        
        /// <summary>
        ///   Down sampling rate
        /// </summary>
        /// <remarks>
        ///   If you want to change this, contact ciumac.sergiu@gmail.com
        /// </remarks>
        private const int SampleRate = 5512;

        #endregion

        Dictionary<string, HashSet<long[]>> _fingerprints;
        Dictionary<long, HashSet<string>>[] _hashTables;

        public Dictionary<string, HashSet<long[]>> Fingerprints
        {
            get { return _fingerprints; }
        }

        public Dictionary<long, HashSet<string>>[] HashTables
        {
            get { return _hashTables; }
        }

        public FingerprintsGenerator()
        {
            _hashTables = new Dictionary<long, HashSet<string>>[NumberOfHashTables];
            for (int i = 0; i < NumberOfHashTables; i++)
            {
                _hashTables[i] = new Dictionary<long, HashSet<string>>();
            }

            _fingerprints = new Dictionary<string, HashSet<long[]>>();
        }

        /// <summary>
        /// Je me sers de l'article http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
        /// qui décrit et "vulgarise" un article posté par des ingénieurs de Google sur le meilleur algorithme de comparaison audio
        /// </summary>
        /// <param name="path"></param>
        public void GenerateFingerprints(string path)
        {
            if (_fingerprints.ContainsKey(path))
                return;

            //Pour la facilité, je stocke tout dans cet objet
            AudioFile file = new AudioFile(path);

            //Preprocessing the signal
            Preprocessing preprocessingEngine = new Preprocessing(SampleRate, SecondsToProcess, StartProcessingAtSecond);
            preprocessingEngine.ReadMonoFromFile(file);

            //SpectrogramCreation
            Spectrum spectrumEngine = new Spectrum(SampleRate);
            spectrumEngine.CreateLogSpectrogram(file);
            spectrumEngine.CutLogarithmizedSpectrum(file);

            //Wavelet Decomposition
            WaveletDecomposition waveletDecompositionEngine = new WaveletDecomposition();
            waveletDecompositionEngine.Transform(file);

            // Création de la signature proprement dite
            Fingerprints.Fingerprints fingerprintManager = new Fingerprints.Fingerprints();
            List<bool[]> fingerprints = new List<bool[]>();
            int topWavelets = 200;
            foreach (var spectralImage in file.SpectralImages)
            {
                bool[] image = fingerprintManager.ExtractTopWavelets(spectralImage, topWavelets);
                fingerprints.Add(image);
            }
            file.Fingerprints = fingerprints;

            // MinHash Signature
            MinHashAndLSH hashEngine = new MinHashAndLSH(NumberOfHashTables, NumberOfKeys);
            hashEngine.Hash(file);

            //Store the signatures
            _fingerprints.Add(file.Fullname, new HashSet<long[]>());

            foreach (long[] sign in file.Signatures)
            {
                _fingerprints[file.Fullname].Add(sign);

                for (int i = 0; i < NumberOfHashTables; i++)
                {
                    if (!_hashTables[i].ContainsKey(sign[i]))
                    {
                        _hashTables[i][sign[i]] = new HashSet<string>();
                    }

                    _hashTables[i][sign[i]].Add(file.Fullname);
                }
            }            
        }

        /// <summary>
        ///   Get tracks that correspond to a specific hash signature and pass the threshold value
        /// </summary>
        /// <param name = "hashSignature">Hash signature of the track</param>
        /// <param name = "hashTableThreshold">Number of threshold tables</param>
        /// <returns>Possible candidates</returns>
        public Dictionary<string, int> GetTracks(string file, long[] signature, int hashTableThreshold)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            // loop through all 25 hash tables
            for (int i = 0; i < NumberOfHashTables; i++)
            {
                if (HashTables[i].ContainsKey(signature[i]))
                {
                    HashSet<string> tracks = HashTables[i][signature[i]]; // get the set of tracks that map to a specific hash signature

                    // select all tracks except the original one
                    foreach (string track in tracks.Where(t => t != file))
                    {
                        if (!result.ContainsKey(track))
                        {
                            result[track] = 1;
                        }
                        else
                        {
                            result[track]++;
                        }
                    }
                }
            }

            // select only those tracks that passed threshold votes
            Dictionary<string, int> filteredResult = result.Where(item => item.Value >= hashTableThreshold).ToDictionary(item => item.Key, item => item.Value);
            return filteredResult;
        }
    }
}
