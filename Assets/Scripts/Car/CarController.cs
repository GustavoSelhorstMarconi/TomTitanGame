using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float moveInput;
    private float turnInput;
    private bool isCarGrounded;
    private float groundCar;
    private Transform mainCamera;
    public float airDrag;
    public float forwardSpeed;
    public float revSpeed;
    public float turnSpeed;
    public float alignToGroundTime;
    public LayerMask groundLayer;
    public Rigidbody sphereRigidBody;
    public Rigidbody carRigidBody;
    public Vector3 offset;
    public float carHeight;

    void Start()
    {
        sphereRigidBody.transform.parent = null;
        carRigidBody.transform.parent = null;

        groundCar = sphereRigidBody.drag;
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        float newRotation = turnInput * turnSpeed * Time.deltaTime * moveInput;

        transform.position = sphereRigidBody.transform.position + offset;

        if (isCarGrounded)
        {
            transform.Rotate(0, newRotation, 0, Space.World);
        }

        //Raycast
        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, carHeight * 0.5f + 0.3f, groundLayer);

        Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);

        moveInput *= moveInput > 0 ? forwardSpeed : revSpeed;

        sphereRigidBody.drag = isCarGrounded ? groundCar : airDrag;
    }

    private void FixedUpdate()
    {
        if (isCarGrounded)
        {
            sphereRigidBody.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        }
        else
        {
            sphereRigidBody.AddForce(transform.up * -30f);
        }

        carRigidBody.MoveRotation(transform.rotation);
    }
}
