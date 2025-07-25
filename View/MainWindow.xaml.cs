using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Write
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        public class ProjectData
        {
            public string name { get; set; }
            public List<FileWithText> texts { get; set; } = new();
            public List<Reference> references { get; set; } = new();
            public List<FileWithText> trash { get; set; } = new();
        }

        public class FileWithText
        {            
            public string name { get; set; }
            public string content { get; set; }    
            public bool isDelete { get; set; }
            public string modified { get; set; }
            public string created { get; set; }            
        }

        public class Reference
        {
            public int id { get; set; }
            public string name { get; set; }
            public string definition { get; set; }
            public List<string> targets { get; set; } = new();            
        }

        public class ProjectWindows
        {
            public string textContent { get; set; }
        }

        //Change . and filter
        private void cmdCreate_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Proyecto MyWriter (*.myprojz)|*.myprojz",
                DefaultExt = ".myprojz"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string path = saveDialog.FileName;

                using (FileStream zipToOpen = new(path, FileMode.Create))
                using (ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create))
                {
                    var project = new ProjectData
                    {
                        name = System.IO.Path.GetFileNameWithoutExtension(path)
                    };

                    var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
                    var entry = archive.CreateEntry("project.json");
                    using (var writer = new StreamWriter(entry.Open()))
                    {
                        writer.Write(json);
                    }

                    archive.CreateEntry("texts/");
                    archive.CreateEntry("trash/");
                }

                Console.WriteLine("Project created!");

                ShowProject(path);                
            }
        }

        //Change . and filter
        private void cmdLoad_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Proyecto MyWriter (*.myprojz)|*.myprojz"
            };

            if (openDialog.ShowDialog() == true)
            {
                ShowProject(openDialog.FileName);
            }
        }

        private void ShowProject(string path)
        {
            //string path = oPath.FileName;

            MessageBox.Show(path);

            using ZipArchive archive = ZipFile.OpenRead(path);
            var entry = archive.GetEntry("project.json");

            using var reader = new StreamReader(entry.Open());
            string json = reader.ReadToEnd();
            ProjectData project = JsonSerializer.Deserialize<ProjectData>(json);

            // Abrir ventana del proyecto y pasarle path + project
            var projectWindow = new ProjectWindow(path, project);
            projectWindow.Show();
        }
    }
}