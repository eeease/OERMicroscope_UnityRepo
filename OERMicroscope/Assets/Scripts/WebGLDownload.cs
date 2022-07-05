using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;

public class WebGLDownload : MonoBehaviour
{
    public enum ImageFormat
    {
        jpg,
        png
    }
    private bool _isRecording = false;
    [DllImport("__Internal")]
    private static extern void DownloadFileJsLib(byte[] byteArray, int byteLength, string fileName);
    public void SavePNG()
    {
        string fName = MicroscopeManager.MM.nameInput.text;
        fName = fName.Replace("\n", "").Replace("\r","");
        //print(fName);
        GetScreenshot(ImageFormat.png, 1, fName + System.DateTime.Now);//insert person's name from input field as 3rd arg.
    }
    /// <summary>
    /// ___
    /// <para>bytes -> The bytes to be downloaded</para>
    /// <para>fileName -> The downloaded file name (without extension)</para>
    /// <para>fileExtension -> WebGLDownload.FileExtension.jpg/png/zip/</para>
    /// </summary>
    public void DownloadFile(byte[] bytes, string fileName, string fileExtension)
    {
        if (fileName == "") fileName = "UnnamedFile";
#if UNITY_EDITOR
        //string path = UnityEditor.EditorUtility.SaveFilePanel("Save file...", "", fileName, fileExtension);
        StartCoroutine(FileBrowserSave());
        //System.IO.File.WriteAllBytes(path, bytes);
        //Debug.Log("File saved: " + path);
        //FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );



#elif UNITY_WEBGL
        DownloadFileJsLib(bytes, bytes.Length, fileName + "." + fileExtension);
#else
        StartCoroutine(FileBrowserSave());
#endif
    }

    /// <summary>
    /// ___
    /// <para>imageFormat -> WebGLDownload.ImageFormat.jpg/png</para>
    /// <para>screenshotUpscale -> Upscale the frame. default = 1</para>
    /// <para>fileName -> Optional filename. Empty filename creates a name texture.width x texture.height in pixel + current datetime</para>
    /// </summary>
    public void GetScreenshot(ImageFormat imageFormat, int screenshotUpscale, string fileName = "")
    {
        if (!_isRecording) StartCoroutine(RecordUpscaledFrame(imageFormat, screenshotUpscale, fileName));
    }

    IEnumerator RecordUpscaledFrame(ImageFormat imageFormat, int screenshotUpscale, string fileName)
    {
        _isRecording = true;
        yield return new WaitForEndOfFrame();
        try
        {
            if (fileName == "")
            {
                int resWidth = Camera.main.pixelWidth * screenshotUpscale;
                int resHeight = Camera.main.pixelHeight * screenshotUpscale;
                string dateFormat = "yyyy-MM-dd-HH-mm-ss";
                fileName = resWidth.ToString() + "x" + resHeight.ToString() + "px_" + System.DateTime.Now.ToString(dateFormat);
            }
            Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture(screenshotUpscale);
            if (imageFormat == ImageFormat.jpg) DownloadFile(screenShot.EncodeToJPG(), fileName, "jpg");
            else if (imageFormat == ImageFormat.png) DownloadFile(screenShot.EncodeToPNG(), fileName, "png");
            Object.Destroy(screenShot);
        }
        catch (System.Exception e)
        {
            Debug.Log("Original error: " + e.Message);
        }
        _isRecording = false;
    }

    IEnumerator FileBrowserSave()
    {
        string fName = MicroscopeManager.MM.nameInput.text;
        fName = fName.Replace("\n", "").Replace("\r", "");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.FilesAndFolders,false,"C\\", fName + ".png", "Save As", "Save");
        //StartCoroutine(FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.FilesAndFolders, false, "C:\\", "Screenshot.png", "Save As", "Save"));
        if (FileBrowser.Success)
        {
            ScreenCapture.CaptureScreenshot(FileBrowser.Result[0]);
        }
    }


}