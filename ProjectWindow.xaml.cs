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
                fText.Name = fileName;
                fText.Show();
            }
        }

        private void cmdNewFile_Click(object sender, RoutedEventArgs e)
        {            
            TextName tN = new TextName(this);
            tN.Show();            
        }
        
        private void cmdReaload_Click(object sender, RoutedEventArgs e)
        {
            LoadFilesInProject();
        }        
    }
}
