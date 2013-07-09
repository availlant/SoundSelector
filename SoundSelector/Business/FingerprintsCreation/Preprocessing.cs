using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Model;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;

namespace Business.FingerprintsCreation
{
    public class Preprocessing : IDisposable
    {
        private int _sampleRate;
        private int _secondsToRead;
        private int _startAtSecond;

        private const int DefaultSampleRate = 44100;

        #region IDisposable - Singleton
        private static int initializedInstances;

        private bool alreadyDisposed;

        ~Preprocessing()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(false);
            alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                if (!isDisposing)
                {
                    // release managed resources
                }

                    if (initializedInstances == 1)
                    {
                        // 0 - free all loaded plugins
                        if (!Bass.BASS_PluginFree(0))
                        {
                            Debug.WriteLine("Could not unload plugins for Bass library.");
                        }

                        if (!Bass.BASS_Free())
                        {
                            Debug.WriteLine("Could not free Bass library. Possible memory leakage.");
                        }
                    }

                    initializedInstances--;
            }
        }
        #endregion        

        public Preprocessing(int sampleRate, int secondsToRead, int startAtSecond)
        {
            _sampleRate = sampleRate;
            _secondsToRead = secondsToRead;
            _startAtSecond = startAtSecond;

            InitBass();
        }

        private void InitBass()
        {
                if (initializedInstances == 0)
                {
                    string targetPath = Environment.CurrentDirectory;

                    // Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
                    BassNet.Registration("gleb.godonoga@gmail.com", "2X155323152222");

                    // Dummy calls made for loading the assemblies
#pragma warning disable 168
                    bool isBassLoad = Bass.LoadMe(targetPath);
                    bool isBassMixLoad = BassMix.LoadMe(targetPath);
                    bool isBassFxLoad = BassFx.LoadMe(targetPath);
                    int bassVersion = Bass.BASS_GetVersion();
                    int bassMixVersion = BassMix.BASS_Mixer_GetVersion();
                    int bassfxVersion = BassFx.BASS_FX_GetVersion();
#pragma warning restore 168
                    var loadedPlugIns = Bass.BASS_PluginLoadDirectory(targetPath);
                    if (!loadedPlugIns.Any(p => p.Value.EndsWith("bassflac.dll")))
                    {
                        throw new Exception("Couldnt load the bass flac plugin!");
                    }

                    // Set Sample Rate / MONO
                    if (!Bass.BASS_Init(-1, DefaultSampleRate, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero))
                    {
                        throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                    }

                    /*Set floating parameters to be passed*/
                    if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true))
                    {
                        throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                    }
                }

                initializedInstances++;
            
        }

        public void ReadMonoFromFile(AudioFile file)
        {
            float[] data;

            data = ReadMonoFromFile(file.Fullname);

            file.Data = data;
        }
        
        private float[] ReadMonoFromFile(string pathToFile)
        {
            // create streams for re-sampling
            int stream = Bass.BASS_StreamCreateFile(pathToFile, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); // Decode the stream

            if (stream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            int mixerStream = BassMix.BASS_Mixer_StreamCreate(_sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (!BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (_startAtSecond > 0)
            {
                if (!Bass.BASS_ChannelSetPosition(stream, _startAtSecond))
                {
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                }
            }

            float[] buffer = new float[_sampleRate * 20 * 4]; // 20 seconds buffer
            List<float[]> chunks = new List<float[]>();
            int totalBytesToRead = _secondsToRead == 0 ? int.MaxValue : _secondsToRead * _sampleRate * 4;
            int totalBytesRead = 0;
            while (totalBytesRead < totalBytesToRead)
            {
                // get re-sampled/mono data
                int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, buffer.Length * 4);

                if (bytesRead == -1)
                {
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                }

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;

                float[] chunk;

                if (totalBytesRead > totalBytesToRead)
                {
                    chunk = new float[(totalBytesToRead - (totalBytesRead - bytesRead)) / 4];
                    Array.Copy(buffer, chunk, (totalBytesToRead - (totalBytesRead - bytesRead)) / 4);
                }
                else
                {
                    chunk = new float[bytesRead / 4]; // each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesRead / 4);
                }

                chunks.Add(chunk);
            }

            if (totalBytesRead < (_secondsToRead * _sampleRate * 4))
            {
                return null; /*not enough samples to return the requested data*/
            }

            float[] data = ConcatenateChunksOfSamples(chunks);

            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(stream);
            return data;
        }

        protected float[] ConcatenateChunksOfSamples(List<float[]> chunks)
        {
            if (chunks.Count == 1)
            {
                return chunks[0];
            }

            float[] samples = new float[chunks.Sum(a => a.Length)];
            int index = 0;
            foreach (float[] chunk in chunks)
            {
                Array.Copy(chunk, 0, samples, index, chunk.Length);
                index += chunk.Length;
            }

            return samples;
        }        
    }
}
