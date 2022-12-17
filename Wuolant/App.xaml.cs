using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using Image = iText.Layout.Element.Image;
using Path = System.IO.Path;

namespace Wuolant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // This is used so that the console output can appear in the Rider console
        // (So stupid that it doesn't work by default in WPF applications)
        private const int ATTACH_PARENT_PROCESS = -1;
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        public static void AttachToParentConsole()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }
        
        // Beginning of the program
        private void Start(object sender, StartupEventArgs e)
        {
            AttachToParentConsole();
        }

        public bool ProcessPDF(string readPath, bool isA4)
        {
            // First it checks whether this actually comes from Wuolah (in case you're re-dragging an already adblocked file)
            if (!IsWuolahFile(readPath))
            {
                Console.WriteLine("File isn't a valid Wuolah PDF.");
                return false;
            }
            
            // Then it creates the clean PDF
            CreateCleanPdf(readPath, isA4);
            return true;
        }
        
        private bool IsWuolahFile(string path)
        {
            // First we check if it's actually a PDF
            if (!path.EndsWith(".pdf"))
            {
                return false;
            }
            
            try
            {
                // Now's a little hacky, we just check whether pdf-lib created the file (our files aren't pdf-lib)
                PdfReader reader = new PdfReader(path);
                PdfDocument pdfDocument = new PdfDocument(reader);

                PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
                bool isWuolah = info.GetCreator() != null && info.GetCreator().StartsWith("pdf-lib");
                pdfDocument.Close();
                reader.Close();
                return isWuolah;
            }
            catch (Exception ignored)
            {
                // The PDF isn't valid, so we skip it
                return false;
            }
        }

        private void CreateCleanPdf(string readPath, bool isA4)
        {
            // It creates a clean path to export the new PDF
            // It'll be inside a folder called "Wuolan't", in the same directory as the original file
            string cleanPath = GetOutputPath(readPath);
            Console.WriteLine($"Writing to {cleanPath}");
            
            // We read the original file
            PdfReader reader = new PdfReader(readPath);
            reader.SetUnethicalReading(true);
            PdfDocument originalPdf = new PdfDocument(reader);
            
            // And we create a writer for the new PDF
            PdfWriter writer = new PdfWriter(cleanPath);
            PdfDocument writerPdf = new PdfDocument(writer);
            Document writerDocument = new Document(writerPdf);
            
            // Now we iterate page by page to find the embedded PDFs 
            int currentCleanPage = 0;
            for (var i = 1; i <= originalPdf.GetNumberOfPages(); i++)
            {
                // Each page will have a Resources object, where the different page objects will be embedded
                PdfPage page = originalPdf.GetPage(i);
                PdfDictionary pageDict = page.GetPdfObject();
                PdfDictionary resources = pageDict.GetAsDictionary(PdfName.Resources);
                PdfDictionary xObjects = resources.GetAsDictionary(PdfName.XObject);
                
                // We iterate the XObjects inside the Resources instance, that's where the original PDFs are stored (as an X-Form)
                // We're searching for names like "/EmbeddedPdfPage-2708199956"
                foreach (PdfName name in xObjects.KeySet())
                {
                    if (name.GetValue().StartsWith("EmbeddedPdfPage"))
                    {
                        // Found the proper xObject. We load it in a wrapper object.
                        PdfObject ogObject = xObjects.GetAsStream(name).CopyTo(writerPdf);
                        PdfStream pdfObject = ogObject as PdfStream;
                        PdfFormXObject form = new PdfFormXObject(pdfObject);

                        // Now we create an image based on the X-Form, which we'll save in the new PDF
                        Image image = new Image(form);
                        
                        // We create the new page for the image. It'll be either the original image size, or A4
                        // If it's A4, we don't need to make it fit the page. Adding the image to the doc already handles that
                        // Otherwise, we need to make each page the size of the image
                        if (!isA4)
                        {
                            PageSize newPageSize = new PageSize(image.GetImageWidth(), image.GetImageHeight());
                            writerPdf.AddNewPage(newPageSize);
                            image.SetFixedPosition(++currentCleanPage, 0, 0);
                            image.ScaleToFit(newPageSize.GetWidth(), newPageSize.GetHeight());   
                        }
                        
                        // And we finally add it to the doc
                        writerDocument.Add(image);
                    }
                }
            }

            // Closing the reader and writer
            writerPdf.Close();
            originalPdf.Close();
            Console.WriteLine("Finished cleaning PDF!");
        }
        
        private string GetOutputPath(string originalPath)
        {
            // The output path will be a new folder called "Wuolan't" in the same directory
            // In there, a file with the same name will be created (or overwritten!)
            string newDirectory = Path.GetDirectoryName(originalPath) + Path.DirectorySeparatorChar + "Wuolan't";
            string fileName = Path.GetFileName(originalPath);
            string cleanPath = newDirectory + Path.DirectorySeparatorChar + fileName;
            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }
            if (File.Exists(cleanPath))
            {
                File.Delete(cleanPath);
            }

            return cleanPath;
        }
    }
}