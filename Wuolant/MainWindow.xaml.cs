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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wuolant
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

        public void FileDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // We iterate the paths of all the dropped files
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    int countValid = 0;
                    int countInvalid = 0;
                    if (files != null)
                    {
                        foreach (string path in files)
                        {
                            Console.WriteLine($"Received {path}!");
                            // We send it to the application so it processes it
                            bool validPdf = ((App)Application.Current).ProcessPDF(path);
                            if (validPdf)
                            {
                                countValid++;
                            }
                            else
                            {
                                countInvalid++;
                            }
                        }
                    }

                    string text = $"Finished cleaning {countValid} files!";
                    if (countInvalid > 0)
                    {
                        text += $"\nThere were {countInvalid} files that couldn't be processed.";
                    }
                    text += "\nGood luck!";
                    MessageBox.Show(text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error processing the files!\n\n" + ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }
    }
}