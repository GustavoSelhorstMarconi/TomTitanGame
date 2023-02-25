using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunnig;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    public CameraControl cam;
    private PlayerMovement playerMovement;
    private LedgeGrabbing playerLedgeGrabbing;
    private Rigidbody playerRigidBody;

    private void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        playerLedgeGrabbing = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    void FixedUpdate()
    {
        if (playerMovement.wallRunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        //Get inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunnig = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        //State 1 WallRunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!playerMovement.wallRunning)
            {
                StartWallRun();
            }

            //WallRun timer
            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            if (wallRunTimer <= 0 && playerMovement.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            //Wall jump
            if (Input.GetKeyDown(jumpKey))
            {
                WallJump();
            }
        }

        //State 2 Exiting
        else if (exitingWall)
        {
            if (playerMovement.wallRunning)
            {
                StopWallRun();
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

        else
        {
            if (playerMovement.wallRunning)
            {
                StopWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        playerMovement.wallRunning = true;

        wallRunTimer = maxWallRunTime;
        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);
    }

    private void WallRunningMovement()
    {
        playerRigidBody.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        //Forward force
        playerRigidBody.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //Upwards / downwards force
        if (upwardsRunnig)
        {
            playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, wallClimbSpeed, playerRigidBody.velocity.z);
        }
        if (downwardsRunning)
        {
            playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, -wallClimbSpeed, playerRigidBody.velocity.z);
        }

        //Push wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            playerRigidBody.AddForce(-wallNormal * 100f, ForceMode.Force);
        }

        //Counter gravity
        if (useGravity)
        {
            playerRigidBody.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        playerMovement.wallRunning = false;
    }

    private void WallJump()
    {
        if (playerLedgeGrabbing.holding || playerLedgeGrabbing.exitingLedge)
        {
            return;
        }

        //Enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        //Add force
        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);
        playerRigidBody.AddForce(forceToApply, ForceMode.Impulse);
    }
}