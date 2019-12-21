using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauController : MonoBehaviour {

    public static TauHand LeftHand { get; private set; }
    public static TauHand RightHand { get; private set; }
    public static TauHand AppItem { get; private set; }

    public static TauController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SoftReset() {
        LeftHand.busy = false;
        RightHand.busy = false;
    }

    public static void SetLeftHand(TauHand hand) {
        LeftHand = hand;
    }

    public static void SetRightHand(TauHand hand)
    {
        RightHand = hand;
    }

    public static void SetAppItem(TauHand item)
    {
        AppItem = item;
    }
}
