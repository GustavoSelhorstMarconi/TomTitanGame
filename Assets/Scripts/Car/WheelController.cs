using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public GameObject[] wheelsToRotate;
    public float rotationSpeed;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float verticalAxis = Input.GetAxisRaw("Vertical");
        float horizontalAxis = Input.GetAxisRaw("Horizontal");

        foreach (GameObject wheel in wheelsToRotate)
        {
            wheel.transform.Rotate(Time.deltaTime * verticalAxis * rotationSpeed, 0, 0, Space.Self);
        }

        if (horizontalAxis > 0)
        {
            anim.SetBool("goingLeft", false);
            anim.SetBool("goingRight", true);
        }
        else if (horizontalAxis < 0)
        {
            anim.SetBool("goingRight", false);
            anim.SetBool("goingLeft", true);
        }
        else
        {
            anim.SetBool("goingRight", false);
            anim.SetBool("goingLeft", false);
        }
    }
}