using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Write.MainWindow;

namespace Write
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        public string projectZipPath;
        public ProjectData project;        

        public ProjectWindow(string projectZipPath, ProjectData project)
        {
            InitializeComponent();

            this.projectZipPath = projectZipPath;
            this.project = project;

            lblNameProject.Text = project.name;
            LoadFilesInProject();
        }

        public void LoadFilesInProject()
        {
            lstFiles.Items.Clear();
            foreach (string file in project.texts)
            {
                lstFiles.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
        }

        private void cmdOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItem is string fileName)
            {
                string fullInternalPath = $"texts/{fileName}.mytext";

                FileText fText = new FileText(projectZipPath, fullInternalPath);
                
                fText.Show();
            }
        }

        private void cmdNewFile_Click(object sender, RoutedEventArgs e)
        {            
            TextName tN = new TextName(this);
            tN.Show();            
        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {            
            if (lstFiles.SelectedItem is string fileName)
            {
                DeleteTextFile(projectZipPath, $"{fileName}");
                LoadFilesInProject();
            }            
        }

        private void cmdReaload_Click(object sender, RoutedEventArgs e)
        {
            LoadFilesInProject();
        }

        private void MoveToTrash(ProjectData project, string projectZipPath, string fileName)
        {
            string tempZip = System.IO.Path.GetTempFileName();
            File.Copy(projectZipPath, tempZip, true);

            using (var zip = ZipFile.Open(tempZip, ZipArchiveMode.Update))
            {
                string oldPath = $"texts/{fileName}";
                string newPath = $"trash/{fileName}";

                var entry = zip.GetEntry(oldPath);
                if (entry == null)
                {
                    MessageBox.Show("El archivo no existe.");
                    return;
                }

                // Copiar contenido a nueva entrada en trash/
                using var originalStream = entry.Open();
                var memoryStream = new MemoryStream();
                originalStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                // Eliminar entrada original
                entry.Delete();

                // Crear nueva entrada en trash/
                var trashEntry = zip.CreateEntry(newPath);
                using var trashStream = trashEntry.Open();
                memoryStream.CopyTo(trashStream);

                // Actualizar el JSON: quitar de texts y poner en trash
                var item = project.texts.FirstOrDefault(t => t == fileName);
                if (item != null)
                {
                    project.texts.Remove(item);
                    //project.trash ??= new List<String>();
                    project.trash.Add(item);
                }

                // Actualizar project.json
                zip.GetEntry("project.json")?.Delete();
                var jsonEntry = zip.CreateEntry("project.json");
                using var jsonWriter = new StreamWriter(jsonEntry.Open());
                jsonWriter.Write(JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true }));
            }

            File.Copy(tempZip, projectZipPath, true);
            File.Delete(tempZip);
        }


        private void DeleteTextFile(string projectZipPath, string fileToDelete)
        {
            string tempZipPath = System.IO.Path.GetTempFileName();

            //Delete the fileText in the project
            project.texts.RemoveAll(t => 
                string.Equals(System.IO.Path.GetFileNameWithoutExtension(t), fileToDelete, StringComparison.OrdinalIgnoreCase)
                || string.Equals(t, $"texts/{fileToDelete}.mytext", StringComparison.OrdinalIgnoreCase));

            using (var originalZip = ZipFile.OpenRead(projectZipPath))
            using (var tempZip = ZipFile.Open(tempZipPath, ZipArchiveMode.Update))
            {
                foreach (var entry in originalZip.Entries)
                {
                    //Modify the zip
                    if (!entry.FullName.Equals($"texts/{fileToDelete}.mytext", StringComparison.OrdinalIgnoreCase)
                        && !entry.FullName.Equals("project.json", StringComparison.OrdinalIgnoreCase)
                        && !System.IO.Path.GetFileNameWithoutExtension(entry.FullName).Equals(fileToDelete, StringComparison.OrdinalIgnoreCase))
                    {
                        using var input = entry.Open();
                        var newEntry = tempZip.CreateEntry(entry.FullName);
                        using var output = newEntry.Open();
                        input.CopyTo(output);
                    }
                }

                //New project.json
                var jsonEntry = tempZip.CreateEntry("project.json");
                using var jsonWriter = new StreamWriter(jsonEntry.Open());
                jsonWriter.Write(JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true }));
            }

            File.Delete(projectZipPath);
            File.Move(tempZipPath, projectZipPath);

            using (var zip = ZipFile.OpenRead(projectZipPath))
            {
                foreach(var entry in zip.Entries)
                {
                    MessageBox.Show(entry.FullName);
                }
            }       
        }
    }
}
