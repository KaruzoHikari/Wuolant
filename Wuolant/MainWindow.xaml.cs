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
                    int count = 0;
                    if (files != null)
                    {
                        foreach (string path in files)
                        {
                            count++;
                            Console.WriteLine($"Received {path}!");
                            // We send it to the application so it processes it
                            ((App)Application.Current).ProcessPDF(path);
                        }
                    }

                    MessageBox.Show($"Finished processing {count} files!\nGood luck!");
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