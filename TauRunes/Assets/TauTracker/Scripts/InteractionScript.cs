using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionScript : MonoBehaviour {

    bool currentlyPinch;
    public TauTrackerClient.SensorEnum sensor1, sensor2;
    // Use this for initialization
    void Start () {
		
	}

    void Update()
    {
        SensorData sensorData1 = TauTrackerClient.Instance.GetSensorData(sensor1);
        SensorData sensorData2 = TauTrackerClient.Instance.GetSensorData(sensor2);
        if (sensorData1 != null && sensorData2 != null)
        {
            Vector3 s1_pos = sensorData1.GetPosition() / TauTrackerClient.Instance.trackerPositionFactor;
            Quaternion s1_rot = sensorData1.GetRotation();
            Vector3 s2_pos = sensorData2.GetPosition() / TauTrackerClient.Instance.trackerPositionFactor;
            Vector3 distance = s1_pos - s2_pos;
            Vector3 pinch_pos = (s1_pos + s2_pos) * 0.5f;

            //print(distance.magnitude);

            if (distance.magnitude < 1.5)
            {
                //print("pinch!");
                OnPinch(pinch_pos, s1_rot);
            }
        }
    }

    void OnPinch(Vector3 pinch_position, Quaternion s1_rot)
    {
        // ...
        // Check if we pinched a movable object and grab the closest one.
        Collider[] close_things = Physics.OverlapSphere(pinch_position, 1f);
        for (int j = 0; j < close_things.Length; ++j)
        {
            Collider grabbed_object_ = close_things[j];
            Rigidbody rb = grabbed_object_.GetComponent<Rigidbody>();
            if (rb) {
                Vector3 distance = pinch_position - grabbed_object_.transform.position;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(2000 * distance);

                Quaternion quat10 = s1_rot * Quaternion.Inverse(grabbed_object_.transform.rotation);
                rb.AddTorque(500 * quat10.x, 500 * quat10.y, 500 * quat10.z, ForceMode.Force);
            }

        }
    }
}
