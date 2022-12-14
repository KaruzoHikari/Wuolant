using System;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;

namespace Wuolant;

public class ImageRenderListener : IEventListener
{
    private string exportPath;
    private int index;
    private List<int> indirectReferences;

    public ImageRenderListener(string exportPath)
    {
        this.exportPath = exportPath;
    }

    public void SetIndirectReferences(List<int> references)
    {
        this.indirectReferences = references;
    }

    public void EventOccurred(IEventData data, EventType type)
    {
        if (data is not ImageRenderInfo imageData)
        {
            return;
        }
        
        try
        {
            // We obtain the image object
            PdfImageXObject imageObject = imageData.GetImage();
            if (imageObject == null)
            {
                Console.WriteLine("Image could not be read.");
            }
            else
            {
                // Now we check if it's a non-ad image (from the indirect reference ID, we calculated the non-ad ones beforehand)
                int objectNumber = imageObject.GetPdfObject().GetIndirectReference().GetObjNumber();
                Console.WriteLine($"Image is: {objectNumber}");
                if (indirectReferences.Contains(objectNumber))
                {
                    // We write the image in the output path
                    Console.WriteLine("Image is not an ad!");
                    File.WriteAllBytes(string.Format(exportPath, index++, imageObject.IdentifyImageFileExtension()), imageObject.GetImageBytes(true));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Couldn't read image: {0}.", ex.ToString());
        }
    }

    public ICollection<EventType> GetSupportedEvents()
    {
        return null;
    }
}