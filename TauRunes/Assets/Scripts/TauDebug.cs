using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauDebug : MonoBehaviour {

    public GameObject debugInfo;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            debugInfo.SetActive(!debugInfo.activeSelf);
        }
    }
}
