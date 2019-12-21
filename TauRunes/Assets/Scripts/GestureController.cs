using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureController : MonoBehaviour
{
    const float ADJUSTMENT_FACTOR = 0.01f;

    public TauTrackerClient.SensorEnum wrist;
    public TauTrackerClient.SensorEnum thumb;
    public TauTrackerClient.SensorEnum index;
    public TauTrackerClient.SensorEnum middle;
    public TauTrackerClient.SensorEnum ring;
    public TauTrackerClient.SensorEnum pinky;
    //for debug
    private int counter = 0;

    void Start()
    {
        
    }

    void Update()
    {
        counter++;
        if (counter > 50)
        {
            SensorData wristSensorData = TauTrackerClient.Instance.GetSensorData(wrist);
            SensorData thumbSensorData = TauTrackerClient.Instance.GetSensorData(thumb);
            SensorData indexSensorData = TauTrackerClient.Instance.GetSensorData(index);
            SensorData middleSensorData = TauTrackerClient.Instance.GetSensorData(middle);
            SensorData ringSensorData = TauTrackerClient.Instance.GetSensorData(ring);
            SensorData pinkySensorData = TauTrackerClient.Instance.GetSensorData(pinky);

            if (wristSensorData != null && thumbSensorData != null && indexSensorData != null &&
                middleSensorData != null && ringSensorData != null && pinkySensorData != null)
            {
                Vector3 wristPos = wristSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
                                                                  //Quaternion s1_rot = sensorData1.GetRotation();
                Vector3 thumbPos = thumbSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
                Vector3 indexPos = indexSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
                Vector3 middlePos = middleSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
                Vector3 ringPos = ringSensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
                Vector3 pinkyPos = pinkySensorData.GetPosition(); // TauTrackerClient.Instance.trackerPositionFactor;
                                                                  //Vector3 pinch_pos = (s1_pos + s2_pos) * 0.5f;

                //Debug.Log(wristPos + "  " + thumbPos + "    " + indexPos + "    " +
                //      "    " + middlePos + "    " + ringPos + "   " + pinkyPos);

                Vector3 indexDirection = indexPos - wristPos;
                Vector3 thumbDirection = thumbPos - wristPos;
                Vector3 middleDirection = middlePos - wristPos;
                Vector3 ringDirection = ringPos - wristPos;
                Vector3 pinkyDirection = pinkyPos - wristPos;

                var middleProjectionToIndexDirection = Vector3.Dot(indexDirection, middleDirection) * ADJUSTMENT_FACTOR;
                var ringProjectionToIndexDirection = Vector3.Dot(indexDirection, ringDirection) * ADJUSTMENT_FACTOR;
                var pinkyProjectionToIndexDirection = Vector3.Dot(indexDirection, pinkyDirection) * ADJUSTMENT_FACTOR;

                var indexMagnitude = indexDirection.magnitude;

                //Debug.Log("index magnitude = " + indexMagnitude + "     middle projection to index derection" +
                //          middleProjectionToIndexDirection);

                if (indexMagnitude > middleProjectionToIndexDirection &&
                    indexMagnitude > ringProjectionToIndexDirection &&
                    indexMagnitude > pinkyProjectionToIndexDirection)
                {
                    Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDRAW");
                }

                counter = 0;
            }

        }
    }

    void OnPinch(Vector3 pinch_position, Quaternion s1_rot)
    {
        // ...
        // Check if we pinched a movable object and grab the closest one.
        //Collider[] close_things = Physics.OverlapSphere(pinch_position, 1f);
        //for (int j = 0; j < close_things.Length; ++j)
        //{
        //    Collider grabbed_object_ = close_things[j];
        //    Rigidbody rb = grabbed_object_.GetComponent<Rigidbody>();
        //    if (rb)
        //    {
        //        Vector3 distance = pinch_position - grabbed_object_.transform.position;
        //        rb.velocity = Vector3.zero;
        //        rb.angularVelocity = Vector3.zero;
        //        rb.AddForce(2000 * distance);

        //        Quaternion quat10 = s1_rot * Quaternion.Inverse(grabbed_object_.transform.rotation);
        //        rb.AddTorque(500 * quat10.x, 500 * quat10.y, 500 * quat10.z, ForceMode.Force);
        //    }

        //}
    }
}
