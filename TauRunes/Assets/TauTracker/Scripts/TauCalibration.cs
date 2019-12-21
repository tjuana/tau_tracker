using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using wvr;


public class TauCalibration : MonoBehaviour {

    public Transform tauBaseObject;
    public Transform posRecenter;
    public Transform tauLeftOrientationalFinger;
    public Transform tauRightOrientationalFinger;

    //public WVR_DeviceType device = WVR_DeviceType.WVR_DeviceType_Controller_Right;

    private bool calibrated;

    void Start() {
        float angle = PlayerPrefs.GetFloat("calibrationAngle", 0);
        if (angle != 0) CalibrateAngle(angle);

        //Time.timeScale = 0.5F;
    }

    void Update()
    {

        if (Vector3.Distance(tauLeftOrientationalFinger.position, tauRightOrientationalFinger.position) < 0.020f)
        {
            if (Quaternion.Angle(tauLeftOrientationalFinger.rotation, tauRightOrientationalFinger.rotation) < 22f && !calibrated) {
                CalibrateAngle();
            }
        }
        else {
            calibrated = false;
        }

        /*
        if ((WaveVR_Controller.Input(device).connected && WaveVR_Controller.Input(device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Menu)) || Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 v = tauBaseObject.rotation.eulerAngles;
            tauBaseObject.rotation = Quaternion.Euler(v.x, v.y + 10, v.z);
            PlayerPrefs.SetFloat("calibrationAngle", v.y + 10);
        }


        if ((WaveVR_Controller.Input(device).connected && WaveVR_Controller.Input(device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_DPad_Down)) || Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = posRecenter.position;
            pos.z -= 0.15f;
            posRecenter.position = pos;
        }

        if ((WaveVR_Controller.Input(device).connected && WaveVR_Controller.Input(device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_DPad_Up)) || Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = posRecenter.position;
            pos.z += 0.15f;
            posRecenter.position = pos;
        }

        if ((WaveVR_Controller.Input(device).connected && WaveVR_Controller.Input(device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_DPad_Left)) || Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = posRecenter.position;
            pos.x -= 0.15f;
            posRecenter.position = pos;
        }

        if ((WaveVR_Controller.Input(device).connected && WaveVR_Controller.Input(device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_DPad_Right)) || Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = posRecenter.position;
            pos.x += 0.15f;
            posRecenter.position = pos;
        }
        */
    }

    void CalibrateAngle(float angle = 0) {
        Vector3 v = tauBaseObject.rotation.eulerAngles;
        tauBaseObject.rotation = Quaternion.Euler(v.x, 0, v.z);

        if (angle == 0) {
            Quaternion targetAngle = Quaternion.Lerp(tauLeftOrientationalFinger.rotation, tauRightOrientationalFinger.rotation, .5f);
            Vector3 affected = targetAngle * new Vector3(0, 0, 1);
            affected = Vector3.ProjectOnPlane(affected, Vector3.up);
            angle = Vector3.SignedAngle(affected, new Vector3(0, 0, -1), Vector3.up);
            PlayerPrefs.SetFloat("calibrationAngle", angle);
        }

        Debug.Log("calibration triggered");
        tauBaseObject.rotation = Quaternion.Euler(v.x, angle, v.z);

        calibrated = true;
    }

}

