using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PNGExporter
{

    int count = 0;
    readonly string fileName = "Terrain";

    public void ExportPNG()
    {

        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop); // path to desktop
        string pngFilePath = Path.Combine(path, fileName + ".png");

        // ensure unique file name
        while (File.Exists(pngFilePath))
        {
            count++;
            pngFilePath = Path.Combine(path, fileName + $"({count}).png");
        }

        ScreenCapture.CaptureScreenshot(pngFilePath);

        Debug.Log("Screenshot saved to: " + pngFilePath);

    }

}
