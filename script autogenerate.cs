//The MIT License (MIT)
//
//Copyright (c) 2024 Johannes Sieberer
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using ScanIP;
using System.IO;

class Script
{
        }
    ///
    /// This function iterates through the given directory and creates scanip projects with autosegmented knees for each DICOM series found within the folder.
    ///
    public static void Main (string[] args)
    {
        // Obtain a reference to the application
        App app = App.GetInstance();
        string folderPath = @"E:\Felson MOST OA Study\NWBCT\";
        string saveDirector = @"E:\Felson MOST OA Study\NWBCT_Created_Projects\";
        string[] dirs = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var dir in dirs)
        {
        var dicomSeries = app.SearchForDicom(dir,true);
        try{
        Doc doc = App.GetDocument();
        doc.Close();
        }
        catch{}

            foreach(var dicom in dicomSeries)
            {
            StringVector files = dicom.GetFiles();
            string subdir = files[0].Replace(folderPath,"");
            string[] parts = subdir.Split('\\');
            string knee = string.Empty;
            if (subdir.Contains("LEFT_KNEE"))
            {
                knee = "_Left";
            }
            else
            {
                 knee = "_Right";
            }
            string filePath = saveDirector + parts[0] + knee + ".sip" ;
            CreateProject(dicom, app, filePath);
            }
        }
    }
    ///
    /// This function imports the given dicom, scales it to a resolution doable for the autosegmentation algorithm, starts the autsoegmentation for the tibia, femur, and patella, and savews and closes the project.
    ///
    public static void CreateProject(DicomSeries dicom, App app, string filePath)
    {
        app.ImportDicom(dicom, new CommonImportConstraints().SetWindowLevel(3107.0, -494.5).SetPixelType(Doc.PixelType.Int16), ImportOptions.DicomTagsImportPolicy.Anonymise, false, true);
        var doc = App.GetDocument();
        App.GetDocument().ResampleDataByPercentChange(50.0, 50.0, 50.0, Doc.InterpolationMethod.LinearInterpolation, Doc.InterpolationMethod.LinearInterpolation);
        doc.GetAutoSegmenters().GetASOrtho().ApplyKneeCTTool(new KneeParts(ASOrtho.KneePart.Femur, ASOrtho.KneePart.Tibia, 
        ASOrtho.KneePart.Patella),true, AutoSegmenters.RegionOfInterest.WholeVolume, false);
       doc.SaveAs(filePath);
        doc.Close();
    }
}