using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    public CameraThirdPerson cameraControl;
    private Rigidbody moveRigidBody;
    private PlayerMovement playerMovement;
    private Transform cameraTransform;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        moveRigidBody = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && !playerMovement.wallRunning)
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && playerMovement.sliding)
        {
            StopSlide();
        }
    }

    void FixedUpdate()
    {
        if (playerMovement.sliding)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        playerMovement.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        moveRigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection;
        if (cameraControl.currentStyle == CameraThirdPerson.CameraStyle.basic || cameraControl.currentStyle == CameraThirdPerson.CameraStyle.combat)
        {
            inputDirection = cameraTransform.forward * verticalInput + cameraTransform.right * horizontalInput;
            inputDirection.y = 0f;
        }
        else
        {
            inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        }

        if (!playerMovement.OnSlope() || moveRigidBody.velocity.y > -0.1f)
        {
            moveRigidBody.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        //Sliding a slope
        else
        {
            moveRigidBody.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        playerMovement.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}