using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lineRenderer;
    public Transform gunTip;
    public Transform cam;
    public Transform player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement playerMovement;

    [Header("Swinging")]
    public float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("Air Gear")]
    public Transform orientation;
    public Rigidbody playerRigidBody;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Predction")]
    public RaycastHit predictionHit;
    public float predctionSphereCastRadius;
    public Transform predictionPoint;

    // private Vector3 currentGrapplePosition;

    private void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
        }
        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
        }

        CheckForSwingPoints();

        if (joint != null)
        {
            AirGearMovement();
        }
    }

    // private void LateUpdate()
    // {
    //     DrawRope();
    // }

    private void StartSwing()
    {
        if (predictionHit.point == Vector3.zero)
        {
            return;
        }

        GetComponent<Grappling>().StopGrapple();

        playerMovement.ResetRestrictions();

        playerMovement.swinging = true;

        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        //Distance grapple will try to keep from grapple point
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        //Customize values
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        // lineRenderer.positionCount = 2;
        // currentGrapplePosition = gunTip.position;
    }

    public void StopSwing()
    {
        playerMovement.swinging = false;

        // lineRenderer.positionCount = 0;
        Destroy(joint);
    }

    // private void DrawRope()
    // {
    //     if (!joint)
    //     {
    //         return;
    //     }

    //     currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

    //     lineRenderer.SetPosition(0, gunTip.position);
    //     lineRenderer.SetPosition(1, swingPoint);
    // }

    private void AirGearMovement()
    {
        if (Input.GetKey(KeyCode.D))
        {
            playerRigidBody.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerRigidBody.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            playerRigidBody.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            playerRigidBody.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    private void CheckForSwingPoints()
    {
        if (joint != null)
        {
            return;
        }

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predctionSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        //Option 1 direct hit
        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }

        //Option 2 indirect predicted hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }

        //Option 3 miss
        else
        {
            realHitPoint = Vector3.zero;
        }

        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    public Vector3 GetSwingPoint()
    {
        return swingPoint;
    }

    public bool IsGrappling()
    {
        return joint != null;
    }
}