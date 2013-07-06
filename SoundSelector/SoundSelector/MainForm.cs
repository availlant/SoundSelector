using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Business;

namespace SoundSelector
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void InitGrid()
        {
            dataGridViewResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //dataGridViewResults.Columns.Add("path", "Fichier");
        }

        #region Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;

            InitGrid();
        }

        private void buttonParcourirFileToSearch_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Audio Files (*.mp3, *.wav) |*.mp3;*.wav";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.FileName = "";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFile.Text = openFileDialog.FileName;
            }
        }

        private void buttonParcourirFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Sélectionnez le dossier dans lequel se fera la recherche";
            folderBrowserDialog.ShowNewFolderButton = false;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void buttonRecherche_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBoxFolder.Text))
            {
                return;
            }

            backgroundWorkerComparaison = new BackgroundWorker();
            backgroundWorkerComparaison.WorkerReportsProgress = true;
            backgroundWorkerComparaison.DoWork += backgroundWorkerComparaison_DoWork;
            backgroundWorkerComparaison.ProgressChanged += backgroundWorkerComparaison_ProgressChanged;
            backgroundWorkerComparaison.RunWorkerCompleted += backgroundWorkerComparaison_RunWorkerCompleted;
            backgroundWorkerComparaison.RunWorkerAsync();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxFile.Text = String.Empty;
            textBoxFolder.Text = String.Empty;
        }

        private void buttonClearGrid_Click(object sender, EventArgs e)
        {
            dataGridViewResults.DataSource = null;
        }

        private void backgroundWorkerComparaison_DoWork(object sender, DoWorkEventArgs e)
        {
            ComparisonEngine compEngine = new ComparisonEngine(textBoxFile.Text, textBoxFolder.Text, (double progress) =>
            {
                backgroundWorkerComparaison.ReportProgress((int)Math.Round(progress, 0));
            });

            if (String.IsNullOrWhiteSpace(textBoxFile.Text))
            {
                Dictionary<string, List<string>> couplesDoublons = compEngine.CompareAll();

                List<string> results = new List<string>();

                int i = 1;

                foreach (var dict in couplesDoublons)
                {
                    results.Add(i + " - " + dict.Key);

                    foreach (string doublon in dict.Value)
                    {
                        results.Add(i + " - " + doublon);
                    }

                    i++;
                }

                e.Result = results;
            }
            else
            {
                List<string> doublons = compEngine.Compare();

                e.Result = doublons;
            }
        }

        private void backgroundWorkerComparaison_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarComp.Value = e.ProgressPercentage;
        }

        private void backgroundWorkerComparaison_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                List<string> results = (List<string>)e.Result;

                dataGridViewResults.DataSource = results.Select(x => new { Value = x }).ToList();
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
