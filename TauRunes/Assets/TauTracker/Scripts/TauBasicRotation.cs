using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauBasicRotation : MonoBehaviour {

    public Transform tauBaseObject;

    // Use this for initialization
    void Start()
    {
        float angle = PlayerPrefs.GetFloat("calibrationAngle", 0);
        if (angle != 0) CalibrateAngle(angle);
    }


    void CalibrateAngle(float angle = 0)
    {
        Vector3 v = tauBaseObject.rotation.eulerAngles;
        tauBaseObject.rotation = Quaternion.Euler(v.x, 0, v.z);

        Debug.Log("[TAU]calibration triggered");
        tauBaseObject.rotation = Quaternion.Euler(v.x, angle, v.z);

    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 v = tauBaseObject.rotation.eulerAngles;
            tauBaseObject.rotation = Quaternion.Euler(v.x, v.y + 10, v.z);
            PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
        }
    }


}
