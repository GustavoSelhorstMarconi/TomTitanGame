using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappeable;
    public LineRenderer lineRenderer;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overShootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCoolDown;
    private float grapplingCoolDownTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    public bool grappling;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }

        if (grapplingCoolDownTimer > 0)
        {
            grapplingCoolDownTimer -= Time.deltaTime;
        }
    }

    // private void LateUpdate()
    // {
    //     if (grappling)
    //     {
    //         lineRenderer.SetPosition(0, gunTip.position);
    //     }
    // }

    private void StartGrapple()
    {
        if (grapplingCoolDownTimer > 0)
        {
            return;
        }

        GetComponent<Swinging>().StopSwing();

        grappling = true;

        playerMovement.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappeable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.enabled = true;
        // lineRenderer.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        playerMovement.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overShootYAxis;

        if (grapplePointRelativeYPos < 0)
        {
            highestPointOnArc = overShootYAxis;
        }

        playerMovement.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        playerMovement.freeze = false;

        grappling = false;

        grapplingCoolDownTimer = grapplingCoolDown;

        lineRenderer.enabled = false;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}