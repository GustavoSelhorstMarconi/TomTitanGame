using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car2 : MonoBehaviour
{
    [Header("Suspension Informations")]
    public float suspensionForce;
    public float springDumper; //Force to stop suspension
    public float suspensionRestDist;

    [Header("Steering Informations")]
    [Range(0f, 1f)]
    public float tireGripFactor;
    public float tireMass;

    [Header("Acceleration Informations")]
    public float carTopSpeed;
    public AnimationCurve powerCurve;

    [Header("Tires")]
    public Transform[] tires;
    private Rigidbody carRigidBody;

    private void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
    }
    void Update()
    {
        float accelInput = Input.GetAxisRaw("Vertical");
        foreach (Transform tireTransform in tires)
        {
            //Suspension force
            RaycastHit hitSuspension;
            bool raycastSuspension = Physics.Raycast(tireTransform.position, Vector3.down, out hitSuspension);
            if (raycastSuspension)
            {
                Debug.DrawLine(tireTransform.position, hitSuspension.point, Color.red);
                //Space direction of the spring force
                Vector3 springDir = tireTransform.up;

                //Space velocity of this tire
                Vector3 tireWorldVel = carRigidBody.GetPointVelocity(tireTransform.position);

                //Calculate offset value
                float offset = (suspensionRestDist - hitSuspension.distance);

                //Calculate velocity along the spring direction
                float vel = Vector3.Dot(springDir, tireWorldVel);

                //Calculate magnitude of the dampened spring force
                float force = (offset * suspensionForce) - (vel * springDumper);
                Debug.Log("Offset: " + offset);

                //Apply the force at the location of this tire, in the direction of the suspension
                Debug.DrawLine(tireTransform.position, Vector3.zero + (springDir * force), Color.red);
                carRigidBody.AddForceAtPosition(springDir * force, tireTransform.position);
            }

            //Steering force
            if (raycastSuspension)
            {
                Vector3 steeringDir = tireTransform.right;

                Vector3 tireWorldVel = carRigidBody.GetPointVelocity(tireTransform.position);

                float sterringVel = Vector3.Dot(steeringDir, tireWorldVel);

                float desireVelChange = -sterringVel * tireGripFactor;

                float desiredAccel = desireVelChange / Time.fixedDeltaTime;

                carRigidBody.AddForceAtPosition(steeringDir * tireMass * desiredAccel, tireTransform.position);
                Debug.DrawLine(tireTransform.position, Vector3.zero + steeringDir * tireMass * desiredAccel, Color.blue);
            }

            // Acceleration / breaking
            if (raycastSuspension)
            {
                Vector3 accelDir = tireTransform.forward;

                if (accelInput > 0.0f)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRigidBody.velocity);

                    float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);

                    float availableTorque = powerCurve.Evaluate(normalizedSpeed) * accelInput;

                    carRigidBody.AddForceAtPosition(accelDir * availableTorque, tireTransform.position);
                    Debug.DrawLine(tireTransform.position, Vector3.zero + accelDir * availableTorque, Color.green);
                }
            }
        }
    }
}