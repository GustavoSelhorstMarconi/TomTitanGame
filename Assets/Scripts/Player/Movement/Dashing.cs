using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody playerRigidBody;
    private PlayerMovement playerMovement;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;

    [Header("CameraEffects")]
    public CameraControl cam;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCoolDown;
    private float dashCoolDownTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;

    private Vector3 delayedForceToApply;

    private void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            Dash();
        }

        if (dashCoolDownTimer > 0)
        {
            dashCoolDownTimer -= Time.deltaTime;
        }
    }

    private void Dash()
    {
        if (dashCoolDownTimer > 0)
        {
            return;
        }
        else
        {
            dashCoolDownTimer = dashCoolDown;
        }

        playerMovement.dashing = true;
        playerMovement.maxYSpeed = maxDashYSpeed;

        Transform forwardT;

        if (useCameraForward)
        {
            forwardT = playerCam;
        }
        else
        {
            forwardT = orientation;
        }

        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
        {
            playerRigidBody.useGravity = false;
        }

        delayedForceToApply = new Vector3(forceToApply.x, 0f, forceToApply.z);
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        if (resetVel)
        {
            playerRigidBody.velocity = Vector3.zero;
        }

        playerRigidBody.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        playerMovement.dashing = false;
        playerMovement.maxYSpeed = 0;

        if (disableGravity)
        {
            playerRigidBody.useGravity = true;
        }
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirections)
        {
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        }
        else
        {
            direction = forwardT.forward;
        }

        if (verticalInput == 0 && horizontalInput == 0)
        {
            direction = forwardT.forward;
        }

        return direction.normalized;
    }
}