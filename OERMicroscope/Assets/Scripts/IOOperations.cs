using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleFileBrowser;
#if Unity_Editor
using UnityEditor;
#endif

public class IOOperations : MonoBehaviour
{
    public List<string> specimens;
    public List<string> specImages;
    public SpecimenArray singleSpecimen;
    public List<SpecimenArray> slideBoxSpecimenArray;
    public List<Sprite> specimenSprites;

    public string[] filesArray; //for sorting.  should get rid of list, probably.

    // Start is called before the first frame update
    void Start()
    {
        singleSpecimen = new SpecimenArray();
    }
    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Backslash))
        //{
        //    MicroscopeManager.MM.LoadSlideBox(slideBoxSpecimenArray);

        //}
    }

    //testing this for WebGL - trying to call this from CanvasSampleOpenFolder.cs
    public void LoadSpecimenFolder(string path)
    {
        specimens.Clear();
        specImages.Clear();

        string[] files = Directory.GetFiles(path);
        filesArray = files;
        foreach (string fRaw in files)
        {
            string f = fRaw.ToLower();
            if (f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg"))
            {
                specImages.Add(f);
            }
        }
        StartCoroutine(LoadFullSpecimen());
    }

    public IEnumerator PopulateSlideBoxSpecArray(string path, int arrIndex)
    {
        
        string[] files = Directory.GetFiles(path);
        ns.NumericComparer ns = new ns.NumericComparer();
        System.Array.Sort(files, ns);
        slideBoxSpecimenArray[arrIndex].specimenZoom = new List<Sprite>();
        //int specZoomInd = 0;
        MicroscopeManager.MM.loadingPanel.SetActive(true);
        foreach (string s in files)
        {
            WWW www = new WWW("file:///" + s);
            //print("creating sprite from: " + s);
            while (!www.isDone)
                yield return null;
            //print("creating sprite FOR REAL");
            string sLow = s.ToLower();
            if (sLow.EndsWith(".jpg") || sLow.EndsWith(".png") || sLow.EndsWith(".jpeg"))
            {
                Sprite icon = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(.5f, .5f));
                //print("index = " + arrIndex + " " + slideBoxSpecimenArray[arrIndex].specimenName);
                //!!This is causing NRE issues 6/6/22:
                //FIXED by initializing list on line 49
                slideBoxSpecimenArray[arrIndex].specimenZoom.Add(icon);

            }


        }
        MicroscopeManager.MM.loadingPanel.SetActive(false);

    }

    //using this to load a specimen -EG 4/26
    //still using this - EG 6/6/22
    public void NewBrowserCoroutineTest()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    //deprecating 6/6/22
//    //this is what's being called by Load Custom Specimen button atm (3/28/22)
//    public void LoadSpecimenFolder()
//    {
//        specimens.Clear();
//        specImages.Clear();
//#if UNITY_EDITOR
//        //string path = EditorUtility.OpenFolderPanel("Load Specimen", "", "");
//        string[] path = SFB.StandaloneFileBrowser.OpenFolderPanel("Load Specimen", "", false);
        
//#else
//        //custom way of getting the path
//        string[] path = SFB.StandaloneFileBrowser.OpenFolderPanel("Load Specimen", "", false);
//#endif
//        string[] files = Directory.GetFiles(path[0], "*.png");
//        filesArray = files;
//        foreach(string f in files)
//        {
//            if(f.EndsWith(".png"))
//            {
//                specImages.Add(f);
//            }
//        }
//        StartCoroutine(LoadFullSpecimen());
//    }

    //choose a folder with several sub folders.  Each subfolder should contain a full specimen (4, 10, 40 slides)
    //this needs to call LoadSlideBox() from MicroscopeManager and send in the subfolders.

    public void LoadSlideBoxButtonHelper()
    {
        StartCoroutine(ShowLoadDialogCoroutineSlideBox());
    }
    //this is redundant but i'm at the end of this functionality and want to be done with it:
    IEnumerator ShowLoadDialogCoroutineSlideBox()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, Application.dataPath, null, "Load Entire Slidebox", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            //for (int i = 0; i < FileBrowser.Result.Length; i++)
            //{
            //    //Debug.Log(FileBrowser.Result[i]);
            //    //update the slides information text:
            //    string formatted = FileBrowser.Result[i];
            //    int ind = formatted.LastIndexOf("/") + 1;
            //    MicroscopeManager.MM.slideText.text = "Custom Folder";
            //}

            if (FileBrowser.Result.Length == 1)
            {
                LoadEntireSlideBox(FileBrowser.Result[0]);
            }

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
        else
        {
            MicroscopeManager.MM.SendUserMessage("No folder selected.", false);
        }
    }
    public void LoadEntireSlideBox(string path)
    {
        //string[] path = SFB.StandaloneFileBrowser.OpenFolderPanel("Load Full Slidebox", "", false);
        string[] folders = Directory.GetDirectories(path);
        slideBoxSpecimenArray.Clear();
        for(int i=0; i<folders.Length; i++)
        {
            int copy = i;
            //doctor the full path of the folder so we only get the specimen name:
            string og = folders[copy];
            int lastSlash=0;
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform==RuntimePlatform.WindowsEditor)
            {
                lastSlash = og.LastIndexOf("\\") + 1;
            }
            else
            {
                lastSlash = og.LastIndexOf("/") + 1;
            }
            print(og.Substring(lastSlash));
           
            //populate the local specimen array:
            SpecimenArray sa = new SpecimenArray();
            slideBoxSpecimenArray.Add(sa);
            //set the course name (we probably won't use this)
            //sa.courseName = path[0].Substring(lastSlash);
            //set the specimen name:
            sa.specimenName = og.Substring(lastSlash);
           
          
        }
        //StartCoroutine(PopulateSlideBoxSpecArray(folders[0], 0));
        //maybe i have to do this AFTER the previous loop fully runs?
        for (int i = 0; i < slideBoxSpecimenArray.Count; i++)
        {
            int copy = i;
            StartCoroutine(PopulateSlideBoxSpecArray(folders[copy], copy));

        }

        //!!this should be delayed and I should add a loading screen before actually doing this:
        MicroscopeManager.MM.LoadSlideBox(slideBoxSpecimenArray);
    }

    public void OpenPanel()
    {
        specimens.Clear();
        specImages.Clear();
#if UNITY_EDITOR
        string[] path = SFB.StandaloneFileBrowser.OpenFolderPanel("Load Specimen", "", false);

        //string path = EditorUtility.OpenFolderPanel("Load Slidebox", "", "");
#else
        string[] path = SFB.StandaloneFileBrowser.OpenFolderPanel("Load Specimen", "", false);

#endif
        string[] folders = Directory.GetDirectories(path[0]);
        string[] files = Directory.GetFiles(path[0]);
        //foreach(string file in files)
        //{
        //    if (file.EndsWith(".png"))
        //    {
        //        print(file);
        //    }
        //        //File.Copy(file, EditorApplication.currentScene);
        //}
        foreach (string folder in folders)
        {
          
            //print(folder);
            specimens.Add(folder);
            string[] filesTwo = Directory.GetFiles(folder);
            foreach(string s in filesTwo)
            {
                if (s.EndsWith(".png"))
                {
                    specImages.Add(s);
                    //print(s);
                }

            }
            //File.Copy(file, EditorApplication.currentScene);
        }
        StartCoroutine(LoadImage());

        //StartCoroutine(LoadFullSpecimen());

    }
    public IEnumerator LoadImage()
    {
        WWW www = new WWW("file:///" + specImages[0]);
        while (!www.isDone)
            yield return null;

        Sprite icon = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(.5f, .5f));
        MicroscopeManager.MM.currentSlide.sprite = icon;

    }

    public IEnumerator LoadFullSpecimen()
    {
        if (MicroscopeManager.MM.objectiveLens != 0)
        {
            MicroscopeManager.MM.SendUserMessage("Change zoom to 4x before loading slide!", true);
            MicroscopeManager.MM.ToggleHelperArrow(MicroscopeManager.MM.helperArrowOcular, true);

        }
        else
        {

            MicroscopeManager.MM.ToggleHelperArrow(MicroscopeManager.MM.helperArrowOcular, false);
            //sorting the specimen files in human alphabetical order
            ns.NumericComparer ns = new ns.NumericComparer();
            System.Array.Sort(filesArray, ns);
            //specImages.Sort();
            //specImages.Sort((x, y) => x.CompareTo(y));
            singleSpecimen.specimenName = specImages[0];
            //print("beginning to load full specimen " + specImages[0]);

            //make new specimenarray:

            //declare how many zooms it has (some have 4, some 3)

            //populate zoom amounts after sorting
            specimenSprites.Clear();
            //!!When creating sprites, I should make sure it's ONLY getting jpgs or pngs.
            foreach (string s in filesArray)
            {
                WWW www = new WWW("file:///" + s);
                while (!www.isDone)
                    yield return null;
                string sLow = s.ToLower();
                if (sLow.EndsWith(".jpg") || sLow.EndsWith(".png") || sLow.EndsWith(".jpeg"))
                {
                    Sprite icon = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(.5f, .5f));
                    specimenSprites.Add(icon);
                }
            }

            //this is adding from the list, which i may abandon:
            singleSpecimen.specimenZoom = specimenSprites;


            //foreach(Sprite spr in specimenSprites)
            //{
            //    singleSpecimen.specimenZoom.Add(spr);
            //}
            //load slide:
            MicroscopeManager.MM.LoadSlide(singleSpecimen);
        }
    }

    //Might even want to use this:
    /*
     * 
     *     public void ShowIcon(string iconPath)
    {
        byte[] data = File.ReadAllBytes(iconPath);
        Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        texture.LoadImage(data);
        texture.name = Path.GetFileNameWithoutExtension(iconPath);
        Sprite icon = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        SpriteRenderer iconRenderer = GetComponent<SpriteRenderer>();
        iconRenderer.sprite = icon;
    }

    */

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, Application.dataPath, null, "Load Specimen Folder", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
            {
                Debug.Log(FileBrowser.Result[i]);
                //update the slides information text:
                string formatted = FileBrowser.Result[i];
                int ind = 0;
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    ind = formatted.LastIndexOf("\\") + 1;
                }
                else
                {
                    ind = formatted.LastIndexOf("/") + 1;
                }
                MicroscopeManager.MM.slideText.text = "Custom: " +formatted.Substring(ind);
            }

            if (FileBrowser.Result.Length==1)
            {
                LoadSpecimenFolder(FileBrowser.Result[0]);
            }
           
            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
        else
        {
            MicroscopeManager.MM.SendUserMessage("No folder selected.", false);
        }
    }

   


}
