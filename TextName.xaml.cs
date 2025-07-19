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
    /// Interaction logic for TextName.xaml
    /// </summary>
    public partial class TextName : Window
    {
        private ProjectWindow pw;

        public TextName(ProjectWindow pw)
        {
            InitializeComponent();
            this.pw = pw;
        }        

        private void cmdConfirm_Click(object sender, RoutedEventArgs e)
        {            
            AddTextFile(pw.project, pw.projectZipPath, txtName.Text);
            pw.LoadFilesInProject();
            this.Close();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddTextFile(ProjectData project, string zipPath, string fileName)
        {
            string tempZip = System.IO.Path.GetTempFileName();
            File.Copy(zipPath, tempZip, true);

            using (var zip = ZipFile.Open(tempZip, ZipArchiveMode.Update))
            {
                var entry = zip.CreateEntry($"texts/{fileName}");

                using (var entryStream = entry.Open())
                {
                    FlowDocument doc = new FlowDocument(new Paragraph(new Run("")));
                    TextRange textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
                    textRange.Save(entryStream, DataFormats.XamlPackage);
                }

                project.texts.Add($"texts/{fileName}");

                zip.GetEntry("project.json")?.Delete();
                var newJson = zip.CreateEntry("project.json");
                using var jsonWriter = new StreamWriter(newJson.Open());
                jsonWriter.Write(JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true }));
            }

            File.Copy(tempZip, zipPath, true);
            File.Delete(tempZip);
        }
    }
}
