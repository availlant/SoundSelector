using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
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

            MyDataGridViewButtonColumn columnLire = new MyDataGridViewButtonColumn();
            columnLire.Name = "lire";
            columnLire.Text = "Lire";
            columnLire.HeaderText = "Lecture";
            columnLire.UseColumnTextForButtonValue = true;
            columnLire.cellClick += columnLire_cellClick;

            dataGridViewResults.Columns.Add(columnLire);

            MyDataGridViewButtonColumn columnSupp = new MyDataGridViewButtonColumn();
            columnSupp.Name = "supp";
            columnSupp.Text = "Supprimer";
            columnSupp.HeaderText = "Suppression";
            columnSupp.UseColumnTextForButtonValue = true;
            columnSupp.cellClick += columnSupp_cellClick;

            dataGridViewResults.Columns.Add(columnSupp);

            dataGridViewResults.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewResults.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewResults.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewResults.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewResults.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        void columnSupp_cellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewResults.Rows[e.RowIndex].Cells[1].Value == null)
                return;

            string filePath = dataGridViewResults.Rows[e.RowIndex].Cells[1].Value.ToString();

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Le fichier n'existe plus", "Fichier introuvable");
            }
            else
            {
                if (MessageBox.Show("Êtes-vous sûr de vouloir supprimer le fichier ?", "Suppression", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        File.Delete(filePath);
                        MessageBox.Show("Le fichier a été supprimé", "Suppression");
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        MessageBox.Show("L'application n'a pas les droits pour supprimer le fichier", "Erreur");
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Le fichier n'a pas pu être supprimer", "Erreur");
                    }                    
                }
            }
        }

        void columnLire_cellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewResults.Rows[e.RowIndex].Cells[1].Value == null)
                return;

            string filePath = dataGridViewResults.Rows[e.RowIndex].Cells[1].Value.ToString();

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Le fichier n'existe plus", "Fichier introuvable");
            }
            else
            {
            }
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

        private void dataGridViewResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewResults.Columns[e.ColumnIndex] is MyDataGridViewButtonColumn)
            {
                ((MyDataGridViewButtonColumn)dataGridViewResults.Columns[e.ColumnIndex]).RaiseEventCellClick(sender, e);
            }
        }
    }

    public class MyDataGridViewButtonColumn : DataGridViewButtonColumn
    {
        public event DataGridViewCellEventHandler cellClick;

        public virtual void RaiseEventCellClick(object sender, DataGridViewCellEventArgs e)
        {
            cellClick(sender, e);
        }
    }
}
