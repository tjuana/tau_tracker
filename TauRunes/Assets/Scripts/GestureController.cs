using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureController : MonoBehaviour
{
    private const float ADJUSTMENT_FACTOR = 0.01f;
    private const int DELAY = 50;

    private Camera _camera;

    public TauTrackerClient.SensorEnum _wrist;
    public TauTrackerClient.SensorEnum _thumb;
    public TauTrackerClient.SensorEnum _index;
    public TauTrackerClient.SensorEnum _middle;
    public TauTrackerClient.SensorEnum _ring;
    public TauTrackerClient.SensorEnum _pinky;
    //for debug
    private int delayCounter = 0;


    public GameObject pointFinger;
    public GameObject middleFinger;
    public GameObject ringFinger;
    public GameObject pinkyFinger;

    public GameObject pointZone;

    private CapsuleCollider pointCollider;
    private CapsuleCollider middleCollider;
    private CapsuleCollider ringCollider;
    private CapsuleCollider pinkyCollider;

    private BoxCollider pointZoneBoxCollider;

    void Start()
    {
        pointCollider = pointFinger.GetComponentInChildren<CapsuleCollider>();
        middleCollider = middleFinger.GetComponentInChildren<CapsuleCollider>();
        ringCollider = ringFinger.GetComponentInChildren<CapsuleCollider>();
        pinkyCollider = pinkyFinger.GetComponentInChildren<CapsuleCollider>();

        pointZoneBoxCollider = pointZone.GetComponent<BoxCollider>();
    }

    void Update()
    {


        SensorData wristSensorData = TauTrackerClient.Instance.GetSensorData(_wrist);
        SensorData thumbSensorData = TauTrackerClient.Instance.GetSensorData(_thumb);
        SensorData indexSensorData = TauTrackerClient.Instance.GetSensorData(_index);
        SensorData middleSensorData = TauTrackerClient.Instance.GetSensorData(_middle);
        SensorData ringSensorData = TauTrackerClient.Instance.GetSensorData(_ring);
        SensorData pinkySensorData = TauTrackerClient.Instance.GetSensorData(_pinky);

        if (wristSensorData != null && thumbSensorData != null && indexSensorData != null &&
            middleSensorData != null && ringSensorData != null && pinkySensorData != null)
        {
            Vector3 wristPos = wristSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
            Vector3 thumbPos = thumbSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
            Vector3 indexPos = indexSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
            Vector3 middlePos = middleSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
            Vector3 ringPos = ringSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
            Vector3 pinkyPos = pinkySensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;

            Quaternion indexQuaternion = indexSensorData.GetRotation();
            Quaternion middleQuaternion = middleSensorData.GetRotation();
            Quaternion ringQuaternion = ringSensorData.GetRotation();
            Quaternion pinkyQuaternion = pinkySensorData.GetRotation();

            Vector3 indexEulerAngles = indexQuaternion.eulerAngles;
            Vector3 middleEulerAngles = middleQuaternion.eulerAngles;
            Vector3 ringEulerAngle = ringQuaternion.eulerAngles;
            Vector3 pinkyEulerAngle = Quaternion.ToEulerAngles(pinkyQuaternion);

            Vector3 indexDirection = indexPos - wristPos;
            Vector3 thumbDirection = thumbPos - wristPos;
            Vector3 middleDirection = middlePos - wristPos;
            Vector3 ringDirection = ringPos - wristPos;
            Vector3 pinkyDirection = pinkyPos - wristPos;

            var middleProjectionToIndexDirection = Vector3.Dot(indexDirection, middleDirection) * ADJUSTMENT_FACTOR;
            var ringProjectionToIndexDirection = Vector3.Dot(indexDirection, ringDirection) * ADJUSTMENT_FACTOR;
            var pinkyProjectionToIndexDirection = Vector3.Dot(indexDirection, pinkyDirection) * ADJUSTMENT_FACTOR;

            var indexMagnitude = indexDirection.magnitude;

            //Debug.Log("index  " + indexEulerAngles + "  middle" +
            //          middleEulerAngles);

            if (indexMagnitude > middleProjectionToIndexDirection &&
                indexMagnitude > ringProjectionToIndexDirection &&
                indexMagnitude > pinkyProjectionToIndexDirection)
            {
                delayCounter++;
                if (delayCounter > DELAY)
                {
                    //Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDRAW");
                    delayCounter = DELAY;
                }
            }
            else
            {
                delayCounter = 0;
            }


            //if (counter > 10)
            //{
            //    Debug.Log(indexEulerAngles + "           " + middleEulerAngles + "    " + ringEulerAngle + "    " +
            //              "    " + pinkyEulerAngle);
            //    counter = 0;
            //}

            //counter++;
            //Debug.Log(wristPos + "  " + thumbPos + "    " + indexPos + "    " +
            //      "    " + middlePos + "    " + ringPos + "   " + pinkyPos);

            //if (indexEulerAngles.y < 0 && middleEulerAngles.y > 0 &&
            //    ringEulerAngle.y > 0 && pinkyEulerAngle.y > 0)
            //{
            //    StartCoroutine(Draw(indexPos));
            //}
        }

    }
    IEnumerator Draw(Vector3 point)
    {
        yield return new WaitForSeconds(1);

        Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDRAW");
        Plane screenPlane = new Plane(Camera.main.transform.forward * -1,
            this.transform.position);
        Ray mRay = Camera.main.ScreenPointToRay(point);
        float rayDistance;
        if (screenPlane.Raycast(mRay, out rayDistance))
        {
            this.transform.position = mRay.GetPoint(rayDistance);
        }
    }
}
