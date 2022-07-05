using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderMoveOtherObj : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Slider mySlide;
    public Transform objToMove;
    //public bool moveX, moveY;
    public float minX, maxX;
    [Tooltip("Set this to > 0 for snapping")]
    public float targetVal; //for snapping
    bool firstMove = true;
    public Slider otherSlide; //for my purposes i need to link two sliders.
    // Start is called before the first frame update
    void Start()
    {
        mySlide = GetComponent<Slider>();
        mySlide.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void MoveOBJ()
    {
        objToMove.localPosition = new Vector3(PercentThrough(mySlide.value, minX, maxX),
            objToMove.transform.localPosition.y,
            objToMove.transform.localPosition.z);

        if(otherSlide)
        {
            otherSlide.value = mySlide.value;
        }
    }
    public float PercentThrough(float p, float min, float max)
    {
        float percTh = min+ ((max-min)*p);
        return percTh;
    }
    public void SetMinOnce()
    {
        if (firstMove)
        {
            minX = objToMove.localPosition.x;
        }
    }

    public void SnapTo(float val)
    {

    }

    //was calling this in OnValueChanged in inspector but that doesn't
    //seem to work with two OnValueChanged.
    public void ValueChangeCheck()
    {
         MoveOBJ();
        ChecklistOperator.clo.ToggleChecklist("adjustOc", false);


    }
    //according to docs, down is required to receive up...
    public void OnPointerDown(PointerEventData eventData)
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetVal > 0 && Mathf.Abs(targetVal - mySlide.value) < 0.1)
        {
            mySlide.value = targetVal;
            ChecklistOperator.clo.ToggleChecklist("adjustOc", true);

            print(targetVal - mySlide.value + ", snapping");
        }
    }
}
