using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Business.Model;

namespace Business
{
    public static class FolderManager
    {
        /// <summary>
        ///   Retourne la liste des fichiers audio contenus dans le dossier
        /// </summary>
        /// <param name = "path">Dossier ou chercher</param>
        /// <returns>List des fichiers audio</returns>
        public static List<string> GetMusicFiles(string path)
        {
            List<string> files = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);

            try
            {
                foreach (FileInfo file in root.GetFiles("*.mp3", SearchOption.AllDirectories))
                {
                    try
                    {
                        files.Add(file.FullName);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }

            return files;
        }
    }
}
