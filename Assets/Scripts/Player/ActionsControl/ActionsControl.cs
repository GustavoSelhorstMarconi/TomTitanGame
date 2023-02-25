using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsControl : MonoBehaviour
{
    [Header("References")]
    public Transform car;
    public CarController carController;
    public WheelController wheelController;
    public PickUpGun gunPickUpController;
    public PlayerMovement playerMovement;
    private Transform mainCamera;

    [Header("Configurations")]
    public float speedRotation;
    public float distanceOutCar;
    public float distanceToEnterCar;
    public KeyCode enterCarKey = KeyCode.F;
    public bool isCar { get; private set; } = false;
    public bool isAiming = false;

    void Update()
    {
        Vector3 distanceToCar = transform.position - car.position;
        if (Input.GetKeyDown(enterCarKey) && distanceToCar.magnitude < distanceToEnterCar)
        {
            if (!isCar)
            {
                isCar = true;
                carController.enabled = true;
                wheelController.enabled = true;
            }
            else
            {
                isCar = false;
                transform.position = transform.position + Vector3.left * distanceOutCar;
                carController.enabled = false;
                wheelController.enabled = false;
            }
        }

        playerMovement.aiming = isAiming;
    }
}