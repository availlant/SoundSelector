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
        ///   Number of threshold votes for a file to be considerate a duplicate
        /// </summary>
        private const int ThresholdVotes = 5;

        /// <summary>
        ///   Value of threshold percentage of fingerprints that needs to be gathered
        ///   in order to be considered a possible result
        /// </summary>
        private const int ThresholdFingerprintsToVote = 7;

        /// <summary>
        ///   Down sampling rate
        /// </summary>
        /// <remarks>
        ///   If you want to change this, contact ciumac.sergiu@gmail.com
        /// </remarks>
        private const int SampleRate = 5512;

        #endregion

        List<AudioFile> files = new List<AudioFile>(Buffersize);

        public FingerprintsGenerator()
        {
        }

        /// <summary>
        /// Je me sers de l'article http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
        /// qui décrit et "vulgarise" un article posté par des ingénieurs de Google sur le meilleur algorithme de comparaison audio
        /// </summary>
        /// <param name="path"></param>
        public void GenerateFingerprints(string path)
        {
            //Pour la facilité, je stocke tout dans cet objet (TODO : à revoir)
            AudioFile file = new AudioFile(path);

            //Preprocessing the signal
            Preprocessing preprocessingEngine = new Preprocessing(SampleRate, SecondsToProcess, StartProcessingAtSecond);
            preprocessingEngine.ReadMonoFromFile(file);

            //SpectrogramCreation
            Spectrum spectrumEngine = new Spectrum(SampleRate);
            spectrumEngine.CreateLogSpectrogram(file);
        }
    }
}
