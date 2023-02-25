using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingRope : MonoBehaviour
{
    public LineRenderer lineRendererSwing;
    public LineRenderer lineRendererGrapple;
    public Swinging swinging;
    public Grappling grappling;
    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve curve;
    private SpringRope springSwing;
    private SpringRope springGrapple;
    private Vector3 currentSwingPosition;
    private Vector3 currentGrapplePosition;

    private void Start()
    {
        springSwing = new SpringRope();
        springSwing.SetTarget(0);

        springGrapple = new SpringRope();
        springGrapple.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();

        DrawRopeGrappling();
    }

    private void DrawRope()
    {
        if (!swinging.IsGrappling())
        {
            currentSwingPosition = swinging.gunTip.position;
            springSwing.Reset();
            if (lineRendererSwing.positionCount > 0)
            {
                lineRendererSwing.positionCount = 0;
            }
            return;
        }

        if (lineRendererSwing.positionCount == 0)
        {
            springSwing.SetVelocity(velocity);
            lineRendererSwing.positionCount = quality + 1;
        }

        springSwing.SetDamper(damper);
        springSwing.SetStrength(strength);
        springSwing.Update(Time.deltaTime);

        Vector3 grapplePoint = swinging.GetSwingPoint();
        Vector3 gunTipPosition = swinging.gunTip.position;
        Vector3 up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentSwingPosition = Vector3.Lerp(currentSwingPosition, grapplePoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * springSwing.Value * curve.Evaluate(delta);

            lineRendererSwing.SetPosition(i, Vector3.Lerp(gunTipPosition, currentSwingPosition, delta) + offset);
        }
    }

    private void DrawRopeGrappling()
    {
        if (!grappling.grappling)
        {
            currentGrapplePosition = grappling.gunTip.position;
            springGrapple.Reset();
            if (lineRendererGrapple.positionCount > 0)
            {
                lineRendererGrapple.positionCount = 0;
            }
            return;
        }

        if (lineRendererGrapple.positionCount == 0)
        {
            springGrapple.SetVelocity(velocity);
            lineRendererGrapple.positionCount = quality + 1;
        }

        springGrapple.SetDamper(damper);
        springGrapple.SetStrength(strength);
        springGrapple.Update(Time.deltaTime);

        Vector3 grapplePoint = grappling.GetGrapplePoint();
        Vector3 gunTipPosition = grappling.gunTip.position;
        Vector3 up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * springGrapple.Value * curve.Evaluate(delta);

            lineRendererGrapple.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}