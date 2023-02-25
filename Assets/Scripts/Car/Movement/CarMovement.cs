using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float speed;
    public Transform frontLeft;
    public Transform frontRight;

    public float speedRotation;

    private Transform cameraMain;

    public void ApplyLocalPositionToVisuals(WheelCollider collider, Transform wheel)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        wheel.position = position;
        wheel.localRotation = rotation;
    }

    void Awake()
    {
        cameraMain = Camera.main.transform;
    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        Vector3 cameraDirection = new Vector3(cameraMain.forward.x, transform.forward.y, cameraMain.forward.z);
        transform.forward = Vector3.Lerp(transform.forward, cameraDirection, Time.deltaTime * speedRotation);

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
                ApplyLocalPositionToVisuals(axleInfo.leftWheel, frontLeft);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel, frontRight);
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor * speed;
                axleInfo.rightWheel.motorTorque = motor * speed;
            }
        }
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}