using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestryScript : MonoBehaviour {

    public static DontDestryScript DDS;

    public bool setInactive = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //SingletonApplication();
    }

    //private void SingletonApplication()
    //{
    //    if (DDS == null)
    //    {
    //        DontDestroyOnLoad(gameObject);
    //        DDS = this;
    //    }
    //    else
    //    {
    //        if (DDS != this)
    //        {
    //            Destroy(gameObject);
    //        }
    //    }
    //}

    public void Start()
    {
        this.gameObject.SetActive(!setInactive);
    }

}
