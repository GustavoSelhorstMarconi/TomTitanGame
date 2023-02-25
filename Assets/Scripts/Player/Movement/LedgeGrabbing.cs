using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public Transform orientation;
    public Transform cam;
    public Rigidbody playerRigidBody;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float timeOnLedge;

    public bool holding;

    [Header("Ledge Jumping")]
    public KeyCode jumpKey = KeyCode.Space;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer;

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }

    private void SubStateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool anyInputKeyPressed = horizontalInput != 0 || verticalInput != 0;

        //SubState 1 Holding ledge
        if (holding)
        {
            FreezeRigidBodyOnLedge();

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnLedge && anyInputKeyPressed)
            {
                ExitLedgeHold();
            }

            if (Input.GetKeyDown(jumpKey))
            {
                LedgeJump();
            }
        }

        //SubState 2 Exiting ledge
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0)
            {
                exitLedgeTimer -= Time.deltaTime;
            }
            else
            {
                exitingLedge = false;
            }
        }
    }

    private void LedgeDetection()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (!ledgeDetected)
        {
            return;
        }

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge)
        {
            return;
        }

        if (distanceToLedge < maxLedgeGrabDistance && !holding)
        {
            EnterLedgeHold();
        }
    }

    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(DelayedJumpForce), 0.05f);
    }

    private void DelayedJumpForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        playerRigidBody.velocity = Vector3.zero;
        playerRigidBody.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        holding = true;

        // playerMovement.unlimited = true;
        playerMovement.restricted = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        playerRigidBody.useGravity = false;
        playerRigidBody.velocity = Vector3.zero;
    }

    private void FreezeRigidBodyOnLedge()
    {
        playerRigidBody.useGravity = false;

        Vector3 directionToLedge = currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        //Move player towards ledge
        if (distanceToLedge > 1f)
        {
            if (playerRigidBody.velocity.magnitude < moveToLedgeSpeed)
            {
                playerRigidBody.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
            }
        }

        //Hold onto ledge
        else
        {
            if (!playerMovement.freeze)
            {
                playerMovement.freeze = true;
            }
            if (playerMovement.unlimited)
            {
                playerMovement.unlimited = false;
            }
        }

        //Exiting if something goes wrong
        if (distanceToLedge > maxLedgeGrabDistance)
        {
            ExitLedgeHold();
        }
    }

    private void ExitLedgeHold()
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timeOnLedge = 0f;

        playerMovement.restricted = false;
        playerMovement.freeze = false;

        playerRigidBody.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }
}