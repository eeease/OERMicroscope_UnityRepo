using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChecklistOperator : MonoBehaviour
{
    public static ChecklistOperator clo;
    public GameObject cListDaddy;
    private void Awake()
    {
        clo = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleChecklist(Toggle t)
    {
        t.isOn = !t.isOn;
    }
    public void ToggleChecklist(string t, bool turnOn)
    {
        foreach(Toggle k in cListDaddy.GetComponentsInChildren<Toggle>())
        {
            if(k.gameObject.name==t)
            {
                k.isOn = turnOn;
            }
        }
    }
}
