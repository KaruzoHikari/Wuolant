using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using Image = iText.Layout.Element.Image;

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

        public void ProcessPDF(string readPath)
        {
            // First it checks whether this actually comes from Wuolah (in case you're re-dragging an already adblocked file)
            if (!IsWuolahFile(readPath))
            {
                return;
            }
            
            // Now it creates a temp path to store the pdf images
            string randomPath = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(randomPath);
            Console.WriteLine($"Created temp path at {randomPath}!");
            
            // Then it retrieves them and creates a clean PDF with them
            ExportImages(readPath, randomPath);
            CreateCleanPdf(readPath, randomPath);
            
            // Finally it deletes the temp path
            Directory.Delete(randomPath, true);
        }

        private bool IsWuolahFile(string path)
        {
            // A little hacky, we just check whether pdf-lib created the file (our files aren't pdf-lib)
            using (PdfReader reader = new PdfReader(path))
            {
                using (PdfDocument pdfDocument = new PdfDocument(reader))
                {
                    PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
                    return info.GetCreator() != null && info.GetCreator().StartsWith("pdf-lib");
                }
            }
        }

        private void ExportImages(string readPath, string extractionPath)
        {
            using (PdfReader reader = new PdfReader(readPath))
            {
                using (PdfDocument pdfDocument = new PdfDocument(reader))
                {
                    ImageRenderListener strategy = new ImageRenderListener(extractionPath + Path.DirectorySeparatorChar + @"ExtractedImage-{0}.{1}");
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);

                    // We iterate the PDF page by page
                    // Our first step is gonna be obtaining the IDs for the non-ad images
                    // Then we let iText render the document so we can snatch and save the image if it's valid
                    for (var i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        strategy.SetIndirectReferences(GetImageReferences(pdfDocument.GetPage(i)));
                        parser.ProcessPageContent(pdfDocument.GetPage(i));
                    }
                }
            }
        }

        private List<int> GetImageReferences(PdfPage pdfPage)
        {
            // We're gonna do it the raw way i'm so tired
            // All the image objects contain an ID called indirect reference ID
            // If we iterate from the top, we can know which IDs belong to the embedded pdf pages (it literally says so)
            // The external objects are like this: <</EmbeddedPdfPage-2708199956 26 0 R /Image-6703682664 24 0 R /Image-7888911063 25 0 R /Image-979344929 23 0 R >>
            // So once we have one of these, we search its *inner* Image object, and store its indirect reference ID
            
            // First we find all external embedded pdf objects
            string externalData = pdfPage.GetPdfObject().ToString();
            List<string> rawExternalReferences = ExtractFromString(externalData, @"/EmbeddedPdfPage-", " 0 R");
            List<int> imageReferences = new List<int>();
            foreach (string reference in rawExternalReferences)
            {
                // We obtain its ID
                int objectId = int.Parse(reference.Split(" ")[1]);
                
                // And now we find the inner image ID
                // We're searching for this: /XObject <</R8 51 0 R >>
                string imageData = pdfPage.GetDocument().GetPdfObject(objectId).ToString();
                List<string> rawImageReferences = ExtractFromString(imageData, @"/XObject <</", " 0 R >>");
                
                // In theory there's only 1, so let's handle it manually (if there's not 1, the exception should get thrown anyway)
                // todo lol apparently some documents have MORE than 1? recheck that
                int finalId = int.Parse(rawImageReferences[0].Split(" ")[1]);
                imageReferences.Add(finalId);
            }
            
            return imageReferences;
        }
        
        private List<string> ExtractFromString(string source, string start, string end)
        {
            // Here we'll extract all the strings that exist between X and Y
            var results = new List<string>();
            string pattern = string.Format("{0}({1}){2}", Regex.Escape(start), ".+?", Regex.Escape(end));
            foreach (Match m in Regex.Matches(source, pattern))
            {
                results.Add(m.Groups[1].Value);
            }
            return results;
        }

        private void CreateCleanPdf(string originalPath, string imagesPath)
        {
            // We write the file in a new output folder.
            // If the file exists, we overwrite it.
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
            
            PdfWriter writer = new PdfWriter(cleanPath);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc);

            // We obtain the images by creation date (so we get the ones that were processed first)
            DirectoryInfo di = new DirectoryInfo(imagesPath);
            FileSystemInfo[] files = di.GetFileSystemInfos();
            var orderedFiles = files.OrderBy(f => f.CreationTime);
            
            // Now we interate them
            foreach (FileSystemInfo path in orderedFiles)
            {
                ImageData data = ImageDataFactory.Create(path.FullName);

                // Rotate the horizontal documents to vertical (and hopefully this will work lol)
                Image img = new Image(data);
                if (data.GetWidth() > data.GetHeight())
                {
                    img.SetRotationAngle(-Math.PI/2);
                }
                
                // And finally add it
                Console.WriteLine($"Adding image from {path.FullName}!");
                document.Add(img);
            }

            // Closing the objects
            document.Close(); 
            pdfDoc.Close();
            writer.Close();
        }
    }
}