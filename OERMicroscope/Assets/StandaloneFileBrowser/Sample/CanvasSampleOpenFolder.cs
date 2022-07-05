using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
//Edited by Eric Guadara
//for use in WebGL folder opening with OER Microscope

[RequireComponent(typeof(Button))]
public class CanvasSampleOpenFolder : MonoBehaviour, IPointerDownHandler {
    //public RawImage output;
    public string selectedPath; //to keep track of in Inspector

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFolder(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFolder(gameObject.name, "OnFolderUpload", "", false);
    }

    // Called from browser
    public void OnFolderUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData) { }

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Title", "", false);
        if (paths.Length > 0) {
            //StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
            StartCoroutine(OutputRoutine(paths[0]));

        }
    }
#endif

    private IEnumerator OutputRoutine(string url) {
        var loader = new WWW(url);
        selectedPath = url;
        yield return loader;
        FindObjectOfType<IOOperations>().LoadSpecimenFolder(url);
    }
}