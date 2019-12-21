using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauHand : MonoBehaviour {

    public enum HandType { LeftHand, RightHand, AppItem };
    public HandType handType;
    public Transform handMain;
    public Transform thumbFinger;
    public Transform indexFinger;
    public Transform middleFinger;
    public Transform ringFinger;
    public Transform pinkyFinger;
    public bool busy;

    //void Start () {}

    void Awake()
    {
        switch (handType)
        {
            case (HandType.LeftHand):
                if (TauController.LeftHand == null) TauController.SetLeftHand(this);
                else Debug.Log("TauController.LeftHand is already set, ignoring duplicate");
                break;
            case (HandType.RightHand):
                if (TauController.RightHand == null) TauController.SetRightHand(this);
                else Debug.Log("TauController.RightHand is already set, ignoring duplicate");
                break;
            case (HandType.AppItem):
                if (TauController.AppItem == null) TauController.SetAppItem(this);
                else Debug.Log("TauController.AppItem is already set, ignoring duplicate");
                break;
        }

    }

    //void Update () {}
}
