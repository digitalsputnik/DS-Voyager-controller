using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableGO : MonoBehaviour
{

    public GameObject[] myGameobject;

    //Enable all GameObjects
    public void enableGO()
    {
        foreach ( GameObject GO in myGameobject)
        {
            GO.SetActive(true);
        }
    }

    //Disable all GameObjects
    public void disableGO()
    {
        foreach (GameObject GO in myGameobject)
        {
            GO.SetActive(false);
        }
    }
}
