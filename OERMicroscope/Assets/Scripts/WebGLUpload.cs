using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebGLUpload : MonoBehaviour
{
    public enum ImageFormat
    {
        jpg,
        png
    }
    public enum FileExtension
    {
        zip,
        myownformat
    }
    [DllImport("__Internal")]
    private static extern void UploadFileJsLib(string gameObjectName, string methodName, string filter, bool multiple);
    [DllImport("__Internal")]
    private static extern void UploadFolderJsLib(string gameObjectName, string methodName, string filter, bool multiple);

    [DllImport("__Internal")]
    private static extern void UploadTextureJsLib(string gameObjectName, string methodName, int maxSize, string imageFormat);
    private bool _nonReadable = true;
    private Material _targetMaterial = null;
    private Image _targetImage = null;

    /// <summary>
    /// ___
    /// <para>imageFormat -> Use "jpg" to allow jpg and png images. Use "png" if you need textures with alpha! (allow png only) you can edit the filter in the .jslib file</para>
    /// <para>maxSize -> downsize large images. Max pixel size for the larger side (width or height, only for WebGL -> function in the .jslib) 0 = disabled</para>
    /// <para>nonReadable -> should be "true" unless you have to edit the pixels (less memory usage)</para>
    /// <para>targetMaterial -> set a material for the texture target. default = null</para>
    /// <para>targetImage -> set a image for the texture target (it creates a sprite). default = null</para>
    /// </summary>
    public void UploadTexture(ImageFormat imageFormat, int maxSize, bool nonReadable, Material targetMaterial = null, Image targetImage = null)
    {
        _nonReadable = nonReadable;
        _targetMaterial = targetMaterial;
        _targetImage = targetImage;
#if UNITY_EDITOR
        string[] allImages = new string[] { "images", imageFormat.ToString() };
        if (imageFormat == ImageFormat.jpg) allImages = new string[] { "jpg/png images", "png,jpg,jpeg" };
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Load a texture...", "", allImages);
        //string path = UnityEditor.EditorUtility.OpenFilePanel("Load a texture...", "", imageFormat.ToString());
        StartCoroutine(LoadTexture(path));
#elif UNITY_WEBGL
        UploadTextureJsLib(gameObject.name, "LoadTexture", maxSize, imageFormat.ToString());
#endif
    }
    /// <summary>
    /// ___
    /// <para>fileExtension -> Use your file extension "zip" e.g.</para>
    /// <para>fileExtension -> Edit the enum values to add more extensions</para>
    /// </summary>
    public void UploadFile(FileExtension fileExtension)
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Load a file...", "", fileExtension.ToString());
        StartCoroutine(LoadFile(path));
#elif UNITY_WEBGL
        UploadFileJsLib(gameObject.name, "LoadFile", fileExtension.ToString());
#endif
    }

    //~~~EG-Duplicating this so I can use an UploadFolder Function in WebGL *fingers crossed*
    public void UploadFolder(FileExtension fileExtension)
    {
#if UNITY_EDITOR
        string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Select Folder...", "", false);
        StartCoroutine(LoadFile(paths[0]));
#elif UNITY_WEBGL
        UploadFolderJsLib(gameObject.name, "LoadFile", fileExtension.ToString());
#endif
    }

    //Load the texture from blob or from url. Called from the .jslib
    private IEnumerator LoadTexture(string url)
    {
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url, _nonReadable);
        yield return uwr.SendWebRequest();
        if (uwr.error != null) Debug.Log(uwr.error);
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
            Debug.Log("Loaded texture size: " + texture.width + "x" + texture.height + "px" + " | URL: " + url);

            //apply the texture to a material or image
            if (_targetMaterial) SetMaterialTexture(_targetMaterial, texture, false);
            else if (_targetImage) _targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            else
            {
                //Do something with the texture...
                TextureResultExamples(texture);
            }
        }
    }

    private void TextureResultExamples(Texture2D tex)
    {
        Debug.Log("unused texture here");
    }

    //URP -> Set the material textures
    public void SetMaterialTexture(Material mat, Texture2D tex, bool emissionInclusive)
    {
        mat.SetTexture("_BaseMap", tex);
        if (emissionInclusive) mat.SetTexture("_EmissionMap", tex);
    }

    //Load the byte[] from blob or from url. Called from the .jslib
    private IEnumerator LoadFile(string url)
    {
        using UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();
        if (uwr.error != null) Debug.Log(uwr.error);
        else
        {
            byte[] result = new byte[uwr.downloadHandler.data.Length];
            System.Array.Copy(uwr.downloadHandler.data, 0, result, 0, uwr.downloadHandler.data.Length);
            Debug.Log("Loaded file size: " + uwr.downloadHandler.data.Length + " bytes");

            //Do something with the byte array now...
            ByteResultExamples(result);
        }
    }

    private void ByteResultExamples(byte[] result)
    {
        Debug.Log("unused file here");

        //Zip example (Zip / gzip Multiplatform Native Plugin from the asset store)
        //----
        //bool validZip = lzip.validateFile(null, result);
        //if (validZip)
        //{
        //      bool exist = lzip.entryExists(null, "data/" + "myfile.dat", result);
        //      if (exist)
        //      {
        //          byte[] fileBuffer = lzip.entry2Buffer(null, "data/" + "myfile.dat", result);
        //          string myfileString = System.Text.Encoding.ASCII.GetString(fileBuffer);
        //          fileBuffer = lzip.entry2Buffer(null, "tex/" + "mytexture.jpg", result);
        //          Texture2D mytexture = new Texture2D(2, 2);
        //          mytexture.LoadImage(fileBuffer);
        //      }
        //}

        //Texture2D example (if byte[] array from an image)
        //----
        //Texture2D tex = new Texture2D(2, 2);
        //tex.LoadImage(result);
    }
}