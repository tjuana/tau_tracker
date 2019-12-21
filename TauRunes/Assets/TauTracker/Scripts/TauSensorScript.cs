using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauSensorScript : MonoBehaviour
{

    public TauTrackerClient.SensorEnum sensor;  // should appear as a drop down in editor
    public float forwardShift, upShift, rightShift = 0;

    public enum IKEnum
    {
        Disable,
        DebugLines,
        CapsulePrimitive,
    };
    public IKEnum InverseKinematicsType;
    public float IKCapsuleWidth = 0.4f;

    //private Rigidbody rb;
    private List<GameObject> IKobjList = new List<GameObject>();
    private TauTrackerClient tauTrackerInstance;

    //void Start() 
    //{
    //    rb = GetComponent<Rigidbody>();
    //}

    Vector3 ConvertToUnityNotion(Vector3 raw_vector)
    {
        return new Vector3(-raw_vector.y, raw_vector.z, raw_vector.x);
    }

    void Update() //prev.fixed
    {
        if (tauTrackerInstance == null)
        {
            tauTrackerInstance = TauTrackerClient.Instance;
        }
        else
        {
            SensorData sensorData = tauTrackerInstance.GetSensorData(sensor);
            if (sensorData != null && sensorData.active)
            {
                Vector3 sens_pos = sensorData.GetPosition();
                Quaternion sens_rot = sensorData.GetRotation();
                float pos_factor = tauTrackerInstance.trackerPositionFactor;
                Transform parent_transform = transform.parent.transform;

                transform.localPosition = (
                    parent_transform.rotation * sens_pos / pos_factor +
                    transform.forward * forwardShift / 10 +
                    transform.up * upShift / 10 +
                    transform.right * rightShift / 10
                );

                transform.SetPositionAndRotation(
                    parent_transform.position +
                    (
                        parent_transform.rotation * sens_pos / pos_factor +
                        transform.forward * forwardShift / 10 +
                        transform.up * upShift / 10 +
                        transform.right * rightShift / 10
                    ),
                    parent_transform.rotation * sens_rot
                );
                /*
                rb.MovePosition((
                    transform.parent.transform.position + transform.parent.transform.rotation * sensorData.GetPosition() / TauTrackerClient.Instance.trackerPositionFactor +
                    rb.transform.forward * forwardShift / 10 +
                    rb.transform.up * upShift / 10 +
                    rb.transform.right * rightShift / 10)
                    );
                rb.MoveRotation(transform.parent.transform.rotation * sensorData.GetRotation());
                */
                Vector3 bone_base = Vector3.zero;

                switch (InverseKinematicsType)
                {
                    case IKEnum.Disable:
                        break;
                    case IKEnum.DebugLines:
                        foreach (Vector3 bone_tip in sensorData.ik_data)
                        {
                            if (bone_base == Vector3.zero) bone_base = bone_tip;
                            else
                            {
                                var convertedBase = ConvertToUnityNotion(bone_base) / pos_factor;
                                var convertedTip = ConvertToUnityNotion(bone_tip) / pos_factor;
                                Debug.DrawLine(convertedBase, convertedTip, Color.red);
                                bone_base = Vector3.zero;
                            }
                        }
                        break;
                    case IKEnum.CapsulePrimitive:
                        if (sensorData.ik_data != null && sensorData.ik_data_counter > 0)
                        {
                            int counter = 0;
                            foreach (Vector3 bone_tip in sensorData.ik_data)
                            {
                                if (bone_base == Vector3.zero)
                                {
                                    bone_base = bone_tip;
                                }
                                else
                                {
                                    var convertedBase = ConvertToUnityNotion(bone_base) / pos_factor;
                                    var convertedTip = ConvertToUnityNotion(bone_tip) / pos_factor;

                                    var IKCollection = parent_transform.Find("IKCollection");
                                    if (IKCollection == null)
                                    {
                                        GameObject container = new GameObject("IKCollection");
                                        container.transform.parent = parent_transform;
                                        IKCollection = container.transform;

                                    }

                                    var halfWayVector = (convertedBase + convertedTip) * 0.5f;
                                    if (IKobjList.Count <= counter)
                                    {
                                        GameObject container = new GameObject("IKBasicFragment");
                                        container.transform.parent = IKCollection;
                                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                                        cube.transform.parent = container.transform;
                                        cube.transform.Rotate(new Vector3(-90, 0, 0));
                                        cube.transform.localScale = new Vector3(
                                            0.015f,
                                            (convertedTip - convertedBase).magnitude * 0.68f,
                                            0.015f);
                                        cube.transform.GetComponent<CapsuleCollider>().enabled = false;

                                        //Rigidbody gameObjectsRigidBody = cube.AddComponent<Rigidbody>(); // Add the rigidbody.
                                        //gameObjectsRigidBody.isKinematic = true;
                                        //gameObjectsRigidBody.useGravity = false;

                                        IKobjList.Add(container);
                                    }

                                    IKobjList[counter].transform.position = (parent_transform.position + parent_transform.rotation * halfWayVector);
                                    IKobjList[counter].transform.LookAt((parent_transform.position + parent_transform.rotation * convertedTip));

                                    bone_base = bone_tip;
                                    counter++;
                                }

                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
