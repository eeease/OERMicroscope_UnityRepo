using UnityEngine;
using UnityEngine.UI;

public class WebGLUpDownExamples : MonoBehaviour
{
    public static WebGLUpload _webGLUpload = null;
    public static WebGLDownload _webGLDownload = null;
    private void Awake()
    {
        _webGLUpload = GetComponent<WebGLUpload>();
        _webGLDownload = GetComponent<WebGLDownload>();
    }

    private void UploadZip()
    {
        //Upload a zip file
        _webGLUpload.UploadFile(WebGLUpload.FileExtension.zip);
    }

    private void UploadTexture(WebGLUpload.ImageFormat imageFormat)
    {
        //Upload a Texture and don't downsize the image (0)
        _webGLUpload.UploadTexture(imageFormat, 0, true, null, null);
    }

    private void UploadTextureToMaterial(WebGLUpload.ImageFormat imageFormat, Material mat)
    {
        //Upload a Texture and set the material texture
        _webGLUpload.UploadTexture(imageFormat, 1024, true, mat, null);
    }

    private void UploadTextureToImage(WebGLUpload.ImageFormat imageFormat, Image img)
    {
        //Upload a Texture and set the image sprite
        _webGLUpload.UploadTexture(imageFormat, 1024, true, null, img);
    }

    private void DownloadFile(byte[] bytes)
    {
        _webGLDownload.DownloadFile(bytes, "myFilename", "myExtension");
    }

    private void DownloadZip()
    {
        ////Zip example(Zip / gzip Multiplatform Native Plugin from the asset store)
        ////----
        //lzip.inMemory mZip = new lzip.inMemory();
        //string myText = "Some text";
        //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(myText);
        //lzip.compress_Buf2Mem(mZip, 9, bytes, "data/" + "myData.dat", null, null);
        //Texture2D tex = null;
        //lzip.compress_Buf2Mem(mZip, 9, tex.EncodeToJPG(), "tex/" + "texName.jpg", null, null);
        //byte[] bZip = mZip.getZipBuffer();
        //_webGLDownload.DownloadFile(bZip, "myZipFilename", "zip");
        //bZip = null;
        //lzip.free_inmemory(mZip);
    }

    private void DownloadTexture(Texture2D tex, WebGLDownload.ImageFormat imageFormat)
    {
        byte[] texBytes;
        if (imageFormat == WebGLDownload.ImageFormat.png) texBytes = tex.EncodeToPNG();
        else texBytes = tex.EncodeToJPG();
        _webGLDownload.DownloadFile(texBytes, "texFileName", imageFormat.ToString());
    }

    private void DownloadScreenshot()
    {
        _webGLDownload.GetScreenshot(WebGLDownload.ImageFormat.jpg, 1, "");
    }
}