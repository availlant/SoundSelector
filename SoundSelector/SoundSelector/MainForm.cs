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
            dataGridViewResults.Columns.Add("collection", "Numéro");
            dataGridViewResults.Columns.Add("path", "Fichier");
            dataGridViewResults.Columns.Add("lire", "Lecture");
            dataGridViewResults.Columns.Add("supprimer", "Suppression");

            dataGridViewResults.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewResults.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewResults.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewResults.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewResults.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void PopulateGrid(Dictionary<string, List<string>> results)
        {
            int numCollection = 1;

            foreach (var paire in results)
            {
                dataGridViewResults.Rows.Add(numCollection, paire.Key);

                foreach (var file in paire.Value)
                {
                    dataGridViewResults.Rows.Add(numCollection, file);
                }

                numCollection++;
            }
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
            dataGridViewResults.Rows.Clear();

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
                e.Result = compEngine.CompareAll();
            }
            else
            {
                List<string> doublons = compEngine.Compare();

                e.Result = new Dictionary<string, List<string>>() { { textBoxFile.Text, doublons } };
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
                Dictionary<string, List<string>> results = (Dictionary<string, List<string>>)e.Result;

                PopulateGrid(results);
            }
            catch (Exception)
            {
            }
        }

        #endregion        
    }
}
