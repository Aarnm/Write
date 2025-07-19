using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FileText : Window
    {
        private TextSelection selectedText;

        private string projectZipPath;
        private string internalFile;

        public FileText(string projectZipPath, string internalFile)
        {
            InitializeComponent();
            
            this.projectZipPath = projectZipPath;
            this.internalFile = internalFile;
            selectedText = txtText.Selection;
            txtFontSize.Text = txtText.FontSize.ToString();

            LoadText();
        }

        private void LoadText()
        {
            using (var zip = ZipFile.Open(projectZipPath, ZipArchiveMode.Read))
            {
                var entry = zip.GetEntry(internalFile);

                if (entry != null)
                {
                    using (var stream = entry.Open())
                    {
                        TextRange range = new TextRange(txtText.Document.ContentStart, txtText.Document.ContentEnd);
                        try
                        {
                            range.Load(stream, DataFormats.XamlPackage);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The text didn't exist.");
                }
            }
        }

        private void cmdSaveFile_Click(object sender, RoutedEventArgs e)
        {
            string temp = System.IO.Path.GetTempFileName();
            File.Copy(projectZipPath, temp, true);

            using (var zip = ZipFile.Open(temp, ZipArchiveMode.Update))
            {
                //Delete old entry
                zip.GetEntry(internalFile)?.Delete();

                //Create entry in zip
                var entry = zip.CreateEntry(internalFile);
                using (var entryStream = entry.Open())
                {                    
                    TextRange textRange = new TextRange(txtText.Document.ContentStart, txtText.Document.ContentEnd);
                    textRange.Save(entryStream, DataFormats.XamlPackage);
                }
            }

            //Replace original zip
            File.Copy(temp, projectZipPath, true);
            File.Delete(temp);

            Console.WriteLine("Save done!");

            //Poner algo para mostar que se a guardado, usar linea de abajo?
        }

        private void txtText_LostFocus(object sender, RoutedEventArgs e)
        {
            selectedText = txtText.Selection;                        
        }

        private void cmbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtText != null)
            {
                var selection = txtText.Selection;

                if (!selection.IsEmpty)
                {
                    object currentFamily = cmbFont.FontFamily;

                    if (currentFamily != DependencyProperty.UnsetValue)
                    {
                        if (cmbFont.SelectedIndex == 0)
                        {                        
                            cmbFont.FontFamily = new FontFamily("MS Gothic");
                            currentFamily = cmbFont.FontFamily;
                            selection.ApplyPropertyValue(TextElement.FontFamilyProperty, currentFamily);
                        }
                        else if (cmbFont.SelectedIndex == 1)
                        {
                            cmbFont.FontFamily = new FontFamily("Arial");
                            currentFamily = cmbFont.FontFamily;
                            selection.ApplyPropertyValue(TextElement.FontFamilyProperty, currentFamily);
                        }
                        else if (cmbFont.SelectedIndex == 2)
                        {
                            cmbFont.FontFamily = new FontFamily("Calibri Light");
                            currentFamily = cmbFont.FontFamily;
                            selection.ApplyPropertyValue(TextElement.FontFamilyProperty, currentFamily);
                        }
                    }                
                }            
            }
        }

        //Button Plus Sizes
        private void cmdPlusFont_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedText.IsEmpty)
            {
                TextPointer start = selectedText.Start;
                TextPointer end = selectedText.End;

                TextPointer pointer = start;

                while (pointer != null && pointer.CompareTo(end) < 0)
                {
                    if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        string text = pointer.GetTextInRun(LogicalDirection.Forward);
                        TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                        if (next == null || next.CompareTo(end) > 0)
                            next = end;

                        // Create a range over this text segment
                        TextRange range = new TextRange(pointer, next);

                        object currentSize = range.GetPropertyValue(TextElement.FontSizeProperty);
                        double size;

                        if (currentSize != DependencyProperty.UnsetValue && double.TryParse(currentSize.ToString(), out size))
                        {
                            size += 2;
                        }
                        else
                        {
                            size = 12;
                        }

                        range.ApplyPropertyValue(TextElement.FontSizeProperty, size);

                        pointer = next;
                    }
                    else
                    {
                        pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                    }
                }
            }
        }

        //Button Minus Sizes
        private void cmdMinusFont_Click(object sender, RoutedEventArgs e)
        {
            TextPointer start = selectedText.Start;
            TextPointer end = selectedText.End;

            TextPointer pointer = start;

            while (pointer != null && pointer.CompareTo(end) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = pointer.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                    if (next == null || next.CompareTo(end) > 0)
                        next = end;

                    // Create a range over this text segment
                    TextRange range = new TextRange(pointer, next);

                    object currentSize = range.GetPropertyValue(TextElement.FontSizeProperty);
                    double size;

                    if (currentSize != DependencyProperty.UnsetValue && double.TryParse(currentSize.ToString(), out size))
                    {
                        size -= 2;
                    }
                    else
                    {
                        size = 1;
                    }

                    range.ApplyPropertyValue(TextElement.FontSizeProperty, size);

                    pointer = next;
                }
                else
                {
                    pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }
        
        //Change size in textbox
        private void txtFontSize_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!selectedText.IsEmpty)
            {
                if (txtFontSize.Text != "")
                {
                    selectedText.ApplyPropertyValue(TextElement.FontSizeProperty, txtFontSize.Text);
                }                
            }
        }

        private void txtFontSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (!selectedText.IsEmpty)
            {
                if (e.Key == Key.Enter)
                {                    
                    if (txtFontSize.Text != "")
                    {
                        selectedText.ApplyPropertyValue(TextElement.FontSizeProperty, txtFontSize.Text);                        
                    }
                }
            }
        }

        //Colors
        private void cmdColorChange_Click(object sender, RoutedEventArgs e)
        {
            TextPointer start = selectedText.Start;
            TextPointer end = selectedText.End;

            TextPointer pointer = start;

            while (pointer != null && pointer.CompareTo(end) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = pointer.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                    if (next == null || next.CompareTo(end) > 0)
                        next = end;

                    // Create a range over this text segment
                    TextRange range = new TextRange(pointer, next);
                    
                    object color = new SolidColorBrush(Colors.Black);

                    if (sender.Equals(cmdColorWhite))
                        color = new SolidColorBrush(Colors.White);
                    else if (sender.Equals(cmdColorBlack))
                        color = new SolidColorBrush(Colors.Black);
                    else if (sender.Equals(cmdColorRed))
                        color = new SolidColorBrush(Colors.Red);

                    range.ApplyPropertyValue(TextElement.ForegroundProperty, color);

                    pointer = next;
                }
                else
                {
                    pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }

        //UpperCase cmd
        private void cmdUpperLower_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedText.IsEmpty)
            {
                TextPointer start = selectedText.Start;
                TextPointer end = selectedText.End;

                TextPointer pointer = start;                

                while (pointer != null && pointer.CompareTo(end) < 0)
                {
                    if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        string text = pointer.GetTextInRun(LogicalDirection.Forward);
                        TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                        if (next == null || next.CompareTo(end) > 0)
                            next = end;

                        // Create a range over this text segment
                        TextRange range = new TextRange(pointer, next);

                        string change;
                        if (sender.Equals(cmdUpperCase))
                            change = range.Text.ToUpper();
                        else
                            change = range.Text.ToLower();

                        if (change != null)
                        {
                            //Have to save the format
                            var formatting = new
                            {
                                FontSize = range.GetPropertyValue(TextElement.FontSizeProperty),
                                FontWeight = range.GetPropertyValue(TextElement.FontWeightProperty),
                                FontStyle = range.GetPropertyValue(TextElement.FontStyleProperty),
                                Foreground = range.GetPropertyValue(TextElement.ForegroundProperty),
                                FontFamily = range.GetPropertyValue(TextElement.FontFamilyProperty)
                            };

                            range.Text = change;

                            //Paste the format
                            range.ApplyPropertyValue(TextElement.FontSizeProperty, formatting.FontSize);
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, formatting.FontWeight);
                            range.ApplyPropertyValue(TextElement.FontStyleProperty, formatting.FontStyle);
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, formatting.Foreground);
                            range.ApplyPropertyValue(TextElement.FontFamilyProperty, formatting.FontFamily);
                        }

                        pointer = next;
                    }
                    else
                    {
                        pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                    }
                }                
            }
        }

        //Bold cmd
        private void cmdBoldText_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedText.IsEmpty)
            {
                TextPointer start = selectedText.Start;
                TextPointer end = selectedText.End;

                TextPointer pointer = start;

                while (pointer != null && pointer.CompareTo(end) < 0)
                {
                    if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        string text = pointer.GetTextInRun(LogicalDirection.Forward);
                        TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                        if (next == null || next.CompareTo(end) > 0)
                            next = end;

                        // Create a range over this text segment
                        TextRange range = new TextRange(pointer, next);

                        object weight = range.GetPropertyValue(TextElement.FontWeightProperty);

                        if (!weight.Equals(FontWeights.Bold))
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);                            
                        else                            
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                        Debug.WriteLine(range.GetPropertyValue(TextElement.FontWeightProperty));

                        pointer = next;
                    }
                    else
                    {
                        pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                    }
                }
            }
        }

        private void cmdItalicText_Click(object sender, RoutedEventArgs e)
        {
            TextPointer start = selectedText.Start;
            TextPointer end = selectedText.End;

            TextPointer pointer = start;

            while (pointer != null && pointer.CompareTo(end) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = pointer.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                    if (next == null || next.CompareTo(end) > 0)
                        next = end;

                    // Create a range over this text segment
                    TextRange range = new TextRange(pointer, next);

                    object weight = range.GetPropertyValue(TextElement.FontStyleProperty);

                    if (!weight.Equals(FontStyles.Italic))                    
                        range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);                    
                    else                    
                        range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);                    

                    pointer = next;
                }
                else
                {
                    pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                }
            }        
        }

        private void cmdSubText_Click(object sender, RoutedEventArgs e)
        {
            TextPointer start = selectedText.Start;
            TextPointer end = selectedText.End;

            TextPointer pointer = start;

            while (pointer != null && pointer.CompareTo(end) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = pointer.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);

                    if (next == null || next.CompareTo(end) > 0)
                        next = end;

                    // Create a range over this text segment
                    TextRange range = new TextRange(pointer, next);

                    object weight = range.GetPropertyValue(Inline.TextDecorationsProperty);

                    if (!weight.Equals(TextDecorations.Baseline))
                    {
                        range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Baseline);
                    }
                    else
                    {
                        range.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                    }

                    Debug.WriteLine(range.GetPropertyValue(TextElement.FontWeightProperty));

                    pointer = next;
                }
                else
                {
                    pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }
    }
}
