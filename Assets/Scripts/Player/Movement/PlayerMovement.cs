using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Car Informations")]
    [SerializeField]
    private Transform car;
    [SerializeField]
    private float distanceInCar;

    [Header("Movement Informations")]
    public Transform orientation;
    private float speedMovement;
    public float walkSpeed;
    public float sprintSpeed;
    public float rotationSpeed;
    public float playerHeight;
    public float slideSpeed;
    private float desiredMoveSpeed;
    private float lastDesireModeSpeed;
    public float dashSpeed;
    public float dashSpeedChangeFactor;
    public float swingSpeed;
    public float maxYSpeed;
    public float groundDrag;
    public float wallRunSpeed;
    public float climbSpeed;
    public float aimingMoveSpeed;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    public LayerMask whatIsGround;
    private float horizontalInput;
    private float verticalInput;

    [Header("References")]
    [SerializeField]
    private Climbing climbingController;
    [SerializeField]
    private CameraThirdPerson cameraControl;

    [Header("Jump Informations")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool grounded;
    private bool readyToJump = true;
    private Vector3 movementDirection;
    public ActionsControl actionControl { get; private set; }
    private Rigidbody characterRigidBody;
    private Transform cameraTransform;

    [SerializeField]
    private LayerMask aimColliderLayerMask = new LayerMask();
    public Vector3 mousePosition;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keys")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        swinging,
        walking,
        sprinting,
        wallRunning,
        climbing,
        crouching,
        sliding,
        dashing,
        aiming,
        air
    }

    public bool sliding;
    public bool wallRunning;
    public bool climbing;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    public bool dashing;
    public bool swinging;
    public bool aiming;
    public bool activeGrapple;
    public bool gunEquipped;

    private bool keepMomentum;
    private MovementState lastState;
    private Vector3 velocityToSet;
    private bool enableMovementOnNextTouch;
    private float startSpeedIncreseMultiplier;
    void Start()
    {
        actionControl = GetComponent<ActionsControl>();
        characterRigidBody = GetComponent<Rigidbody>();

        characterRigidBody.freezeRotation = true;
        cameraTransform = Camera.main.transform;

        startYScale = transform.localScale.y;
        startSpeedIncreseMultiplier = speedIncreaseMultiplier;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        CheckInputs();
        SpeedControl();
        StateHandler();
        Aiming();
        characterRigidBody.drag = (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching) && !activeGrapple ? groundDrag : 0;
    }

    void FixedUpdate()
    {
        if (!actionControl.isCar)
        {
            if (activeGrapple)
            {
                return;
            }

            if (swinging)
            {
                return;
            }

            if (restricted)
            {
                return;
            }

            if (climbingController.exitingWall)
            {
                return;
            }

            if (state == MovementState.dashing)
            {
                return;
            }

            movementDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if ((cameraControl.currentStyle == CameraThirdPerson.CameraStyle.basic || cameraControl.currentStyle == CameraThirdPerson.CameraStyle.combat) && !climbing && !wallRunning)
            {
                movementDirection = cameraTransform.forward * verticalInput + cameraTransform.right * horizontalInput;
                movementDirection.y = 0f;
            }

            // On slope
            if (OnSlope() && !exitingSlope)
            {
                characterRigidBody.AddForce(GetSlopeMoveDirection(movementDirection) * speedMovement * 20f, ForceMode.Force);

                if (characterRigidBody.velocity.y > 0)
                {
                    characterRigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }

            else if (grounded)
            {
                characterRigidBody.AddForce(movementDirection.normalized * speedMovement * 10f, ForceMode.Force);
            }

            else if (!grounded)
            {
                characterRigidBody.AddForce(movementDirection.normalized * speedMovement * 10f * airMultiplier, ForceMode.Force);
                characterRigidBody.AddForce(Vector3.down * 10f, ForceMode.Force);
            }

            // Turn off gravity on slope
            if (!wallRunning)
            {
                characterRigidBody.useGravity = !OnSlope();
            }

            if (movementDirection != Vector3.zero && !aiming)
            {
                movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
                movementDirection.Normalize();
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void LateUpdate()
    {
        if (actionControl.isCar)
        {
            transform.position = car.position + Vector3.up * distanceInCar;
        }
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    void OnCollisionEnter(Collision other)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    private void SpeedControl()
    {
        if (activeGrapple)
        {
            return;
        }

        if (OnSlope() && !exitingSlope)
        {
            if (characterRigidBody.velocity.magnitude > speedMovement)
            {
                characterRigidBody.velocity = characterRigidBody.velocity.normalized * speedMovement;
            }
        }

        else
        {
            Vector3 flatVel = new Vector3(characterRigidBody.velocity.x, 0f, characterRigidBody.velocity.z);

            if (flatVel.magnitude > speedMovement)
            {
                Vector3 limitedVel = flatVel.normalized * speedMovement;
                characterRigidBody.velocity = new Vector3(limitedVel.x, characterRigidBody.velocity.y, limitedVel.z);
            }
        }

        // Limit y vel
        if (maxYSpeed != 0 && characterRigidBody.velocity.y > maxYSpeed)
        {
            characterRigidBody.velocity = new Vector3(characterRigidBody.velocity.x, maxYSpeed, characterRigidBody.velocity.z);
        }
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // Smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - speedMovement);
        float startValue = speedMovement;

        while (time < difference)
        {
            speedMovement = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }
            yield return null;
        }

        speedMovement = desiredMoveSpeed;
    }

    private void Jump()
    {
        exitingSlope = true;
        characterRigidBody.velocity = new Vector3(characterRigidBody.velocity.x, 0f, characterRigidBody.velocity.z);

        characterRigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private void StateHandler()
    {
        // Mode aiming
        if (aiming)
        {
            state = MovementState.aiming;
            desiredMoveSpeed = aimingMoveSpeed;
        }

        // Mode dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedIncreaseMultiplier = dashSpeedChangeFactor;
        }

        // Mode freeze
        else if (freeze)
        {
            state = MovementState.freeze;
            characterRigidBody.velocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }

        // Mode unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
            return;
        }

        // Mode climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        // Mode wallRunning
        else if (wallRunning)
        {
            state = MovementState.wallRunning;
            desiredMoveSpeed = wallRunSpeed;
        }

        // Mode sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && characterRigidBody.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }

            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        // Mode swinging
        else if (swinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }

        // Mode crouching
        else if (Input.GetKeyDown(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode air
        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
            {
                desiredMoveSpeed = walkSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesireModeSpeed;

        if (lastState == MovementState.dashing)
        {
            keepMomentum = true;
        }

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                speedMovement = desiredMoveSpeed;
            }
        }

        lastDesireModeSpeed = desiredMoveSpeed;
        lastState = state;

        if (Mathf.Abs(desiredMoveSpeed - speedMovement) < 0.1f)
        {
            keepMomentum = false;
        }
    }

    private void CheckInputs()
    {
        if (!actionControl.isCar)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            if (Input.GetKey(jumpKey) && readyToJump && grounded)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }

            if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                characterRigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            if (Input.GetKeyUp(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        characterRigidBody.velocity = velocityToSet;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void Aiming()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        // Aim shoot
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = raycastHit.point;
            mousePosition = raycastHit.point;
        }

        if (aiming)
        {
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}