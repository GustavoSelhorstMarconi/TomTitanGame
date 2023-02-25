using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraThirdPerson : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody playerRigidBody;
    public PlayerMovement playerMovement;

    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject camNormal;
    public GameObject camCombat;
    public GameObject camTopDown;
    public GameObject camAiming;

    public CameraStyle currentStyle;

    public enum CameraStyle
    {
        basic,
        combat,
        topdown,
        aiming
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (playerMovement.actionControl.isCar)
        {
            SwitchCameraStyle(CameraStyle.topdown);
        }
        // if (Input.GetKeyDown(KeyCode.Keypad1))
        // {
        //     SwitchCameraStyle(CameraStyle.basic);
        // }
        else if (!playerMovement.gunEquipped)
        {
            SwitchCameraStyle(CameraStyle.basic);
        }
        else if (!playerMovement.aiming)
        {
            SwitchCameraStyle(CameraStyle.combat);
        }
        // if (Input.GetKeyDown(KeyCode.Keypad3))
        // {
        //     SwitchCameraStyle(CameraStyle.topdown);
        // }
        else if (playerMovement.aiming)
        {
            SwitchCameraStyle(CameraStyle.aiming);
        }

        if (currentStyle == CameraStyle.topdown)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDirection != Vector3.zero)
            {
                playerObject.forward = Vector3.Slerp(playerObject.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        else if (currentStyle == CameraStyle.basic || currentStyle == CameraStyle.combat || currentStyle == CameraStyle.aiming)
        {
            Vector3 directionToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = directionToCombatLookAt.normalized;

            playerObject.forward = directionToCombatLookAt.normalized;
        }

        Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDirection.normalized;
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        camCombat.SetActive(false);
        camNormal.SetActive(false);
        camTopDown.SetActive(false);
        camAiming.SetActive(false);

        if (newStyle == CameraStyle.basic)
        {
            camNormal.SetActive(true);
        }
        if (newStyle == CameraStyle.combat)
        {
            camCombat.SetActive(true);
        }
        if (newStyle == CameraStyle.topdown)
        {
            camTopDown.SetActive(true);
        }
        if (newStyle == CameraStyle.aiming)
        {
            camAiming.SetActive(true);
        }

        currentStyle = newStyle;
    }
}