using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonExtensionFunctions : MonoBehaviour
{
    public Vector3 nudgeAmnt;
    [Tooltip("Set to negative for infinite")]
    public int maxNudges = 5;
    public int nudgesLeft;

    public GameObject[] objsToNudge;
    public Vector3[] nudgeAmounts;

    private void Start()
    {
        nudgesLeft = maxNudges;
        if(objsToNudge.Length!=nudgeAmounts.Length)
        {
            print("OBJs and Nudge Vectors inequal");
        }
    }

    public void NudgeAllOBJs()
    {
        
        if (maxNudges < 0 || nudgesLeft > 0)
        {


            for (int i = 0; i < objsToNudge.Length; i++)
            {
                objsToNudge[i].transform.localPosition += nudgeAmounts[i];
            }
        }
        if (nudgesLeft > 0)
        {
            nudgesLeft--;
        }
    }

    public void NudgeOBJX(GameObject go)
    {
        
        if(maxNudges<0 || nudgesLeft>0)
        go.transform.localPosition += nudgeAmnt;
        if (nudgesLeft > 0)
        {
            nudgesLeft--;
        }
    }
    public void SwapThisEnabled(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }
    public void IncrementNudges(int howMuch)
    {
        if(nudgesLeft<maxNudges*2)
        nudgesLeft += howMuch;
    }

}
