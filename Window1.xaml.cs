using System;
using System.Collections.Generic;
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

namespace Write
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        TextSelection selectedText;
        public Window1()
        {
            InitializeComponent();

            selectedText = txtText.Selection;
            txtFontSize.Text = txtText.FontSize.ToString();
        }

        private void IncreaseSelectedFontSizes(double amount)
        {
            TextPointer start = txtText.Selection.Start;
            TextPointer end = txtText.Selection.End;

            TextPointer pointer = start;

            while (pointer != null && pointer.CompareTo(end) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = pointer.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = pointer.GetPositionAtOffset(text.Length);

                    // Create a range over this text segment
                    TextRange range = new TextRange(pointer, next);

                    object currentSize = range.GetPropertyValue(TextElement.FontSizeProperty);
                    double size;

                    if (currentSize != DependencyProperty.UnsetValue && double.TryParse(currentSize.ToString(), out size))
                    {
                        size += amount;                        
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

        private void txtText_LostFocus(object sender, RoutedEventArgs e)
        {
            selectedText = txtText.Selection;            

            //MessageBox.Show(selectedText.Text);
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
                TextPointer start = txtText.Selection.Start;
                TextPointer end = txtText.Selection.End;

                TextPointer pointer = start;

                while (pointer != null && pointer.CompareTo(end) < 0)
                {
                    if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        string text = pointer.GetTextInRun(LogicalDirection.Forward);
                        TextPointer next = pointer.GetPositionAtOffset(text.Length);

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
            TextPointer start = txtText.Selection.Start;
            TextPointer end = txtText.Selection.End;

            TextPointer pointer = start;

            while (pointer != null && pointer.CompareTo(end) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = pointer.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = pointer.GetPositionAtOffset(text.Length);

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
    }
}
