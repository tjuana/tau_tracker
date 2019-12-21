using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Calibrator : MonoBehaviour
{


    public Transform tauBaseObject;
    public bool touched = false;

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

        Debug.Log("calibration triggered");
        tauBaseObject.rotation = Quaternion.Euler(v.x, angle, v.z);

    }

    // Update is called once per frame
    void Update()
    {
        //OVRInput.Update();


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 v = tauBaseObject.rotation.eulerAngles;
            tauBaseObject.rotation = Quaternion.Euler(v.x, v.y + 10, v.z);
            PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
        }



        if (Input.touchCount > 0)
        {
            if (touched == false) {
                touched = true;
                Vector3 v = tauBaseObject.rotation.eulerAngles;
                tauBaseObject.rotation = Quaternion.Euler(v.x, v.y + 10, v.z);
                PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
            }

        }
        else {
            touched = false;
        }

        if (Input.GetMouseButtonDown(0)) {
            Vector3 v = tauBaseObject.rotation.eulerAngles;
            tauBaseObject.rotation = Quaternion.Euler(v.x, v.y + 10, v.z);
            PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Vector3 v = tauBaseObject.position;
            tauBaseObject.position = new Vector3(v.x, v.y + 0.1f, v.z);
            //PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector3 v = tauBaseObject.position;
            tauBaseObject.position = new Vector3(v.x, v.y - 0.1f, v.z);
            //PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
        }

    }



}
