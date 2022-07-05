using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("This should have only ONE Text component in children")]
    public GameObject canvasPopup;
    public string message;
    private GameObject canRef; //to hold onto in order to destroy when mouse exits.
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        //print("you just hovered over " + myItem);
        canRef = Instantiate(canvasPopup, transform, false);
        canRef.transform.position = Input.mousePosition;
        canRef.GetComponentInChildren<Text>().text = message;
            //nameCanvas.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canRef != null)
        {
            Destroy(canRef);
        }

    }
}
