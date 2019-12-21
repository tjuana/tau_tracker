using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAction : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("POINT ZONE TRIGGERED");
    }
}
