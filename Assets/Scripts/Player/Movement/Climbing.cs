using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody playerRigidBody;
    public PlayerMovement playerMovement;
    public LedgeGrabbing playerLedgeGrabbing;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private void Start()
    {
        playerLedgeGrabbing = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing && !exitingWall)
        {
            ClimbingMovement();
        }
    }

    private void StateMachine()
    {
        //State 0 Ledge grabbing
        if (playerLedgeGrabbing.holding)
        {
            if (climbing)
            {
                StopClimbing();
            }
        }

        //State 1 Climbing
        else if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0)
            {
                StartClimbing();
            }

            if (climbTimer > 0)
            {
                climbTimer -= Time.deltaTime;
            }

            if (climbTimer <= 0)
            {
                StopClimbing();
            }
        }

        //State 2 Exiting
        else if (exitingWall)
        {
            if (climbing)
            {
                StopClimbing();
            }

            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if (exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        //State none
        else
        {
            if (climbing)
            {
                StopClimbing();
            }
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0)
        {
            ClimbJump();
        }
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || playerMovement.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        playerMovement.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, climbSpeed, playerRigidBody.velocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;
        playerMovement.climbing = false;
    }

    private void ClimbJump()
    {
        if (playerMovement.grounded)
        {
            return;
        }

        if (playerLedgeGrabbing.holding || playerLedgeGrabbing.exitingLedge)
        {
            return;
        }

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);
        playerRigidBody.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}