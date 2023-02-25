using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Camera Follow")]
    public Transform cameraTargetCar;
    public Transform cameraTargetPlayer;
    public Transform orientation;
    [Header("Camera Control")]
    public float sensibility;
    [Range(0, 360)]
    public float limitRotation;
    [Header("Camera Smooth")]
    public Vector3 positionCameraCar;
    public Vector3 positionCameraPlayer;
    public Vector3 positionCameraAiming;
    public float speedRotation;
    private ActionsControl actionControlPlayer;
    private Transform mainCamera;

    float rotX;
    float rotY;

    void Start()
    {
        transform.forward = cameraTargetCar.forward;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        actionControlPlayer = cameraTargetPlayer.gameObject.GetComponent<ActionsControl>();
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse Y");
        float mouseY = Input.GetAxis("Mouse X");

        rotX -= mouseX * sensibility * Time.deltaTime;
        rotY += mouseY * sensibility * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -limitRotation, limitRotation);

        transform.rotation = Quaternion.Euler(rotX, rotY, 0);
        orientation.rotation = Quaternion.Euler(0, rotY, 0);
    }

    void LateUpdate()
    {
        //Camera car movement
        if (actionControlPlayer.isCar)
        {
            transform.position = cameraTargetCar.position;
            if (mainCamera.localPosition != positionCameraCar)
            {
                mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, positionCameraCar, Time.deltaTime * speedRotation);
            }
        }
        else
        {
            transform.position = cameraTargetPlayer.position;
            if (mainCamera.localPosition != positionCameraPlayer)
            {
                mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, positionCameraPlayer, Time.deltaTime * speedRotation);
            }
        }

        //Camera aiming movement
        if (actionControlPlayer.isAiming)
        {
            if (mainCamera.localPosition != positionCameraAiming)
            {
                mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, positionCameraAiming, Time.deltaTime * speedRotation);
            }
        }
        else
        {
            if (mainCamera.localPosition != positionCameraPlayer)
            {
                mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, positionCameraPlayer, Time.deltaTime * speedRotation);
            }
        }
    }

    public void DoFov(float endValue)
    {
        mainCamera.GetComponent<Camera>().fieldOfView = endValue;
    }
}