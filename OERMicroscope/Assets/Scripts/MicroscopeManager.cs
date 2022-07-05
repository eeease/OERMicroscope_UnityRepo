using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SpecimenArray
{
    public string specimenName;
    public string courseName;
    
    public List<Sprite> specimenZoom;

}
public class MicroscopeManager : MonoBehaviour
{
    public static MicroscopeManager MM;
    public Animator microscopeAnim;
    bool zoomed = false;
    //float minVal = -5;
    //int minLimit = -10;
    //float maxVal = 5;
    //int maxLimit = 10;

   
    //public int targetCoarseFocus;
    public int stageHeight;
    public float atViewStageHeight = -3;

    public float fineFocus;
    public Button fineFocUp, fineFocDown; //going to invoke this while changing oculars (the button OnClick does more than just IncreaseFineFocus)
    public int chosenSlide;
    public int objectiveLens; // 0=4x, 1=10x, 2=40x, 3=100x

    [Header("UI Elements")]
    public GameObject slideboxSV;
    public GameObject coursesSV;
    public GameObject helperArrowCFocus, helperArrowOcular,
        loadingPanel, errorRedBox, quitPanel;
    public Text userMessage;
    public Text userGreeting, slideText, ocularText, totalMagText, courseHeaderText;
    public InputField nameInput;
    public List<string> slideNames;
    public Image currentSlide;
    public Sprite[] ocularZooms;
    public Image ocularImage;
    public GameObject slideInMicroscope; //turn this on after loading a slide. (prob after anim)
    Material slideMat; //for blurring, use _BlurIntensity
    public List<Sprite> allSlideImages; //the original slide (4x)

    public GameObject slidePrefab; //!Extendability phase: going to instantiate the buttons according to course?
    public List<GameObject> slidesTEMP = new List<GameObject>();
    public List<int> sTempIndexes = new List<int>(); //storing reference to what index each specimenSlide is when loading.

    public List<SpecimenArray> specimenSlide;
    public GameObject slideButtonParent;
    List<Button> slideButtons = new List<Button>();

    List<Image> interactibleObjs = new List<Image>();
    public List<GameObject> offDuringHelp = new List<GameObject>();
    public GameObject controlsPanel;
    public Color iHelpOn, iHelpOff; //what color should it be when on?  when off?
    bool helpOn = false;

    public bool musicOn;
    public bool buttSFXCD = true; //putting the click on cooldown cause i'm for loop invoking it (might change that later)
    public AudioClip click, slideAnimSFX, errorSFX;

    public int ogSlideCount; //get this in start and trim slidebox of extra (loaded) specimens

    //private void OnGUI()
    //{
    //    EditorGUILayout.LabelField("Min val:", minVal.ToString());
    //    EditorGUILayout.LabelField("Max val:", maxVal.ToString());
    //    EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
    //}

    private void Awake()
    {
        MM = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        slideMat = currentSlide.materialForRendering;
        //start it blurry:
        slideMat.SetFloat("_BlurIntensity", -3);
        foreach(Button b in slideButtonParent.GetComponentsInChildren<Button>())
        {
            slideButtons.Add(b);
            b.onClick.AddListener(() => TurnOnOtherButtons(b));
        }

        //give buttons ability to make noise:
        foreach(Button b in FindObjectsOfType<Button>())
        {
            b.onClick.AddListener(() => ButtonClickSFX());
        }

        foreach(Image i in FindObjectsOfType<Image>())
        {
            if(i.transform.CompareTag("Interactable"))
            {
                interactibleObjs.Add(i);
            }
        }
        chosenSlide = -1; //can use this to check if a slide has been loaded at all.
        slideText.text = "None.  Please load slide.";
        UpdateDebugText();
        ogSlideCount = specimenSlide.Count;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            print(!quitPanel.activeSelf);
            quitPanel.SetActive(!quitPanel.activeSelf);
        }
    }

    public void IncFineFocus(int amnt)
    {
        fineFocus += amnt;
        fineFocus = Mathf.Clamp(fineFocus, -5, 5);
        if(fineFocus == 0)
        {
            //currentSlide.color = Color.white;
            if(buttSFXCD)
            SendUserMessage("Slide is focused!", false);
            ChecklistOperator.clo.ToggleChecklist("fineFocus", true);
        }
        else
        {
            //print(Mathf.Abs(fineFocus) * 40);
            //print(currentSlide.color.a);
            ChecklistOperator.clo.ToggleChecklist("fineFocus", false);
            //if we just want to do alpha:
            //currentSlide.color = new Color(1, 1, 1, 1-(Mathf.Abs(fineFocus))/10);
            
        }
        if(slideMat==null)
        {
            slideMat = currentSlide.materialForRendering;
        }
        //actual blurring:
        slideMat.SetFloat("_BlurIntensity", fineFocus);
        //currentSlide.material = slideMat;
    }

    public void SetStageHeight(int amnt)
    {
        stageHeight = amnt;
    }

    public void IncStageHeight(int amnt)
    {
        stageHeight += amnt;
        stageHeight = Mathf.Clamp(stageHeight, -5, 5);
        //if (Mathf.Abs(stageHeight) < 5)
        //{
        //    if(stageHeight<0)
        //    {
        //        stageHeight = -5;
        //    }
        //    else
        //    {
        //        stageHeight = 5;
        //    }

        //}
        
        if (stageHeight == 0)
        {
            ToggleHelperArrow(helperArrowCFocus, false);
            currentSlide.enabled = true;
            ChecklistOperator.clo.ToggleChecklist("coarseFocus", true);
            microscopeAnim.SetTrigger("zoomIn");
            zoomed = true;
        }
        else
        {
            //ToggleHelperArrow(true);
            currentSlide.enabled = false;
            ChecklistOperator.clo.ToggleChecklist("coarseFocus", false);

        }
    }

    public void IncAVStage(float amnt)
    {
        atViewStageHeight += amnt;
        atViewStageHeight = Mathf.Clamp(atViewStageHeight, -5, 5);

        if (atViewStageHeight==0)
        {
            ChecklistOperator.clo.ToggleChecklist("stageAtTop", true);

        }
        else
        {
            ChecklistOperator.clo.ToggleChecklist("stageAtTop", false);

        }
    }
    public void LoadSlide(int index)
    {
        if (objectiveLens != 0)
        {
            SendUserMessage("Change zoom to 4x before loading slide!", true);
            ToggleHelperArrow(helperArrowOcular, true);
        }
        else
        {
            //trim the extra loaded specimens:
            if (specimenSlide.Count > ogSlideCount)
            {
                print("trimming extra loaded slides");
                specimenSlide.RemoveRange(ogSlideCount, specimenSlide.Count - ogSlideCount);

            }
            chosenSlide = index;
            print("loading slide " + index + ": " + specimenSlide[chosenSlide].specimenName);

            //!!right now it's starting them all at lowest magnification:
            currentSlide.sprite = specimenSlide[chosenSlide].specimenZoom[0];
            //currentSlide.sprite = allSlideImages[chosenSlide];
            SendUserMessage("Slide loaded!", false);
            ToggleHelperArrow(helperArrowOcular, false);

            PlayAnyClip(slideAnimSFX);
            slideInMicroscope.SetActive(false);
            slideInMicroscope.SetActive(true); //budget anim, turning off then back on.
            if (stageHeight != 0)
            {
                ToggleHelperArrow(helperArrowCFocus, true);
            }
            //!!should we make it blurry every time you load a slide?
            fineFocus = -3f;
            IncFineFocus(0);

           

        }
      
    }
    public void LoadSlide(SpecimenArray specificSpecimen)
    {
        if (objectiveLens != 0)
        {
            SendUserMessage("Change zoom to 4x before loading slide!", true);
            ToggleHelperArrow(helperArrowOcular, true);
        }
        else
        {
            //chosenSlide = index;
            print("loading slide " + specificSpecimen.specimenName);

            //!!right now it's starting them all at lowest magnification:
            currentSlide.sprite = specificSpecimen.specimenZoom[0];
            //currentSlide.sprite = allSlideImages[chosenSlide];
            SendUserMessage("Slide loaded!", false);
            ToggleHelperArrow(helperArrowOcular, false);

            PlayAnyClip(slideAnimSFX);
            slideInMicroscope.SetActive(false);
            slideInMicroscope.SetActive(true); //budget anim, turning off then back on.
            if (stageHeight != 0)
            {
                ToggleHelperArrow(helperArrowCFocus, true);
            }
            //!!should we make it blurry every time you load a slide?
            fineFocus = -3f;
            IncFineFocus(0);
            //add it to the list and change the index to the new number:
            specimenSlide.Add(specificSpecimen);
            chosenSlide = specimenSlide.Count - 1;

        }
    }
    public void IncrementOcular()
    {



        objectiveLens += 1;
        
        //could also use IndexOf if that will be easier in the extendable version:
        //ex. specimenSlide[allSlideImages.IndexOf(currentSlide.sprite)]...
        if (chosenSlide >= 0)
        {
            if(objectiveLens<=2)
            {
                int randPos = Random.Range(1, 5);
                //IncFineFocus(Random.Range(1, 5)); //when going UP an ocular, make it so user has to press fine focus DOWN.
                buttSFXCD = false; //we don't want the fine focus button playing a bunch of clips.

                for (int i = 0; i < randPos; i++)
                {

                    fineFocUp.onClick.Invoke();
                }
                buttSFXCD = true;
            }

            //Reset objective lens to 0 (4x) if the slide doesn't have 100x OR the objective lens is already at 100x
            if (specimenSlide[chosenSlide].specimenZoom.Count - 1 < objectiveLens ||
                specimenSlide[chosenSlide].specimenZoom[objectiveLens] == null ||
                objectiveLens > 3)
            {
                objectiveLens = 0;

                if (fineFocus>0)
                {
                    int randNeg = (int)fineFocus;
                    //print(randNeg);
                    buttSFXCD = false;

                    for (int i = 0; i < randNeg*2; i++) //ex fineFoc = 3.  You want to press the DOWN button 6 times to end up at -3 (blurry, need to press up to refocus)
                    {
                        fineFocDown.onClick.Invoke();
                        //print("moving back down to scanning");

                    }
                    buttSFXCD = true;

                }

                //IncFineFocus(Random.Range(-1,-5)); //when going DOWN an ocular, make it so user has to press fine focus UP.

            }
            currentSlide.sprite = specimenSlide[chosenSlide].specimenZoom[objectiveLens];

        }
        //edge case where user is messing with lenses before loading a slide:
        if(chosenSlide==-1 && objectiveLens>2)
        {
            objectiveLens = 0;
        }
        //always turn off the helper arrow if you go back to 4x:
        if(objectiveLens==0)
        {
            ToggleHelperArrow(helperArrowOcular, false);

        }
        ocularImage.sprite = ocularZooms[objectiveLens];

    }
    public void FocusSlide(int ver)
    {
        //currentSlide.sprite = 
    }
    public void TurnOnOtherButtons(Button stayOn)
    {
        if(objectiveLens==0)
        {
            stayOn.interactable = false;
            foreach (Button b in slideButtons)
            {
                if (b != stayOn)
                {
                    b.interactable = true;
                }
            }
        }
    
        
    }

    public void UpdateDebugText()
    {
        string ocul = "";
        string tMag = "";
        switch (objectiveLens)
        {
            case 0:
                ocul = "4x";
                tMag = "40x";
                break;
            case 1:
                ocul = "10x";
                tMag = "100x";
                break;
            case 2:
                ocul = "40x";
                tMag = "400x";
                break;
            case 3:
                ocul = "100x";
                tMag = "1000x";
                break;
            default:
                ocul = "Something is wrong here...";
                break;
        }
        if(chosenSlide<specimenSlide.Count && chosenSlide>=0)
        slideText.text = specimenSlide[chosenSlide].specimenName;

        ocularText.text = ocul;
        totalMagText.text = tMag;
    }

    public void SendUserMessage(string what, bool err)
    {
        if (err)
        {
            PlayAnyClip(errorSFX);
            errorRedBox.SetActive(false); //just in case it's on, this should reset anim.
            errorRedBox.SetActive(true);
        }
        //if(chosenSlide>=0)
        //{
        userMessage.enabled = true;
            userMessage.text = what;
            StopAllCoroutines();
            StartCoroutine(FadeTextDelay(2f));
        //}
    }

    public IEnumerator FadeTextDelay(float d)
    {
        yield return new WaitForSeconds(d);
        //!with time, fade this instead:
        userMessage.enabled = false;
        errorRedBox.SetActive(false);
    }

    public void SwapHelp()
    {
        if(helpOn)
        {
            foreach(Image i in interactibleObjs)
            {
                //i.enabled = false;
                Color prev = i.color;

                i.color = Color.clear;
                ////turn off the objective highlight and the light highlight:
                //if(i.name.Contains("Light")|| i.name.Contains("Objective"))
                //{
                //    i.enabled = false;
                //}
            }
            //turn off the "controls screen":
            controlsPanel.SetActive(false);

            
            helpOn = false;

        }
        else
        {
            foreach (Image i in interactibleObjs)
            {
                i.enabled = true;
                i.color = iHelpOn; //so we only have to change it here for all of them.
                //if (i.name.Contains("Light") || i.name.Contains("Objective"))
                //{
                //    i.enabled = true;
                //}
            }
            //turn off the "controls screen":
            controlsPanel.SetActive(true);

            helpOn = true;
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(0);
    }

    public void ToggleMusic()
    {
        if(musicOn)
        {
            GetComponent<AudioSource>().Pause();
            musicOn = false;
        }
        else
        {
            GetComponent<AudioSource>().Play();
            musicOn = true;
        }
    }

    public void ButtonClickSFX()
    {
        if(musicOn && buttSFXCD)
        {
            AudioSource.PlayClipAtPoint(click, Camera.main.transform.position);
            //StartCoroutine(ClickCD());
        }
    }

    //public IEnumerator ClickCD()
    //{
    //    buttSFXCD = false;
    //    yield return new WaitForEndOfFrame();
    //    buttSFXCD = true;
    //}

    public void PlayAnyClip(AudioClip cl)
    {
        if(musicOn)
            AudioSource.PlayClipAtPoint(cl, Camera.main.transform.position);

    }

    /// <summary>
    /// loads slides according to course.  Empty = all slides.
    /// </summary>
    /// <param name="course">ex. "BIO101</param>
    public void LoadSlideBox(string course)
    {
        DestroySlideButtons();
        slidesTEMP.Clear();
        sTempIndexes.Clear();

       

        string lowCourse = course.ToLower();
        for(int i=0; i<ogSlideCount; i++)
        {
            //don't instantiate if it doesn't have the param course name:
            //Updated this from || to &&; not sure why it was OR EG 4/26/22 - seems like it causes and issue loading
            //a course (BIO132, ex) after loading a custom specimen.
            if((course!=string.Empty && specimenSlide[i].courseName.ToLower().Contains(lowCourse))
                || course =="all") //all slides button will send in "all" as the param to make them all show up.
            {
                GameObject g = Instantiate(slidePrefab, slideButtonParent.transform);
                //give it OnClick functionality:
                //g.GetComponent<Button>().onClick.AddListener(delegate { LoadSlide(i); });
                //g.GetComponent<Button>().onClick.AddListener(() => UpdateDebugText());

                //set slide name according to our big list:screen
                g.GetComponentInChildren<Text>().text = specimenSlide[i].specimenName;
                //change name in hierarchy for easier nav and for sorting later:
                g.name = specimenSlide[i].specimenName;

                slidesTEMP.Add(g);
                sTempIndexes.Add(i);

            }


        }
        courseHeaderText.text = course.ToUpper() + " - " + slidesTEMP.Count + " SLIDES";
        for (int i = 0; i < slidesTEMP.Count; i++)
        {
            int copy = i;
            slidesTEMP[copy].GetComponent<Button>().onClick.AddListener(()=>LoadSlide(sTempIndexes[copy]));
            slidesTEMP[copy].GetComponent<Button>().onClick.AddListener(() => UpdateDebugText());

        }
    }

    public void LoadSlideBox(List<SpecimenArray> spcArr)
    {
        DestroySlideButtons();
        //this swapping of scrollviews is usually done with buttons:
        coursesSV.SetActive(false);
        slideboxSV.SetActive(true);
        courseHeaderText.text =  "CUSTOM FOLDER";

        for (int i=0; i<spcArr.Count; i++)
        {
            int copy = i;
            GameObject g = Instantiate(slidePrefab, slideButtonParent.transform);
            g.GetComponentInChildren<Text>().text = spcArr[i].specimenName;
            g.GetComponent<Button>().onClick.AddListener(() => LoadSlide(spcArr[copy]));

        }
    }

    //creating an overloaded to send in a predetermined (filtered) list:
    public void LoadSlideBox(List<GameObject> sortedL)
    {
        DestroySlideButtons();
        //slidesTEMP.Clear();
        //slidesTEMP = sortedL;//? I think we can just replace the temp list with this new one?

        for (int i = 0; i < sortedL.Count - 1; i++)
        {
                GameObject g = Instantiate(sortedL[i], slideButtonParent.transform);
                //give it OnClick functionality:
                //g.GetComponent<Button>().onClick.AddListener(delegate { LoadSlide(i); });
                //g.GetComponent<Button>().onClick.AddListener(() => UpdateDebugText());

                //set slide name according to our big list:
                //g.GetComponentInChildren<Text>().text = specimenSlide[i].specimenName;
        }

    }

    public void DestroySlideButtons()
    {
        //first, we need to destroy all of the buttons that were previously loaded:
        foreach (Button g in slideButtonParent.GetComponentsInChildren<Button>())
        {
            if (g.gameObject != slideButtonParent)
            {
                Destroy(g.gameObject);
            }
        }
    }

    //for filtering the slidebox, we can call this separate function:
    public void SlideboxAlphabetize()
    {
        //List<GameObject> sList = new List<GameObject>();
        slidesTEMP.Sort((x, y) => x.name.CompareTo(y.name));
        LoadSlideBox(slidesTEMP);
    }
    public void ToggleHelperArrow(GameObject whichArrow, bool trueOrOff)
    {
        whichArrow.SetActive(trueOrOff);
    }

    public void SwapZoom()
    {
        if(zoomed)
        {
            microscopeAnim.SetTrigger("zoomOut");
            zoomed = false;
        }
        else
        {
            microscopeAnim.SetTrigger("zoomIn");
            zoomed = true;
        }
    }

    //i half hate this but it's late and i'm controlling anim triggers with buttons so...
    public void SetZoomBool(bool whaaaat)
    {
        zoomed = whaaaat;
    }

    public void Quit()
    {
        Application.Quit();
    }

    //from:https://answers.unity.com/questions/1571969/creating-a-search-function-using-an-input-field.html
    // Specify the following function in the `onChange` event of the input field
    public void FilterGameObjects(Text input)
    {
        if(string.IsNullOrEmpty(input.text))
        {
            for (int i = 0; i < slidesTEMP.Count; ++i)
            {
                if (slidesTEMP[i] != null) slidesTEMP[i].SetActive(true);
            }
            print("It's EMPTY!");
        }
        else
        {
            string tLower = input.text.ToLower();
            for (int i = 0; i < slidesTEMP.Count; ++i)
            {
                if (slidesTEMP[i] != null) slidesTEMP[i].SetActive(slidesTEMP[i].name.ToLower().Trim().IndexOf(tLower.Trim()) >= 0);
            }

        }
    }

}

