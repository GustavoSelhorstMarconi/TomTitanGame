using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class SpringCode : MonoBehaviour
// {
//     public float downHeight = -10f;
//     public float upHeight = 0f;
//     public bool wasPressed = false;
//     public Transform moveHandle = null;
//     private float goalPosition;
//     public void SetButtonPressed(bool isPressed)
//     {
//         if (wasPressed != isPressed)
//         {
//             wasPressed = isPressed;

//             if (isPressed)
//             {
//                 goalPosition = downHeight;
//             }
//             else
//             {
//                 goalPosition = upHeight;
//             }
//         }
//     }
//     private void OnSprintUpdate(float springValue)
//     {
//         Vector3 offset = Vector3.up * springValue;
//         moveHandle.localPosition = offset;
//     }

//     public static float Spring(float from, float to, float time)
//     {
//         time = Mathf.Clamp01(time);
//         time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
//         return from + (to - from) * time;
//     }

//     public static Vector3 SpringVector(Vector3 from, Vector3 to, float time)
//     {
//         return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
//     }

//     void Update()
//     {
//         Vector3 nextPos = SpringVector(transform.position, transform.position * goalPosition, Time.deltaTime);
//         transform.position = nextPos;
//     }
// }

public class SpringCode
{
    public float damping = 26f;
    public float mass = 1f;
    public float stiffness = 169f;
    public float startValue;
    public float endValue;
    public float initialVelocity;

    protected float currentValue;
    protected float currentVelocity;

    float stepSize = 1f / 60f; // stable if < 1/51
    bool isFirstEvaluate = true;

    public void Reset()
    {
        currentValue = startValue;
        currentVelocity = initialVelocity;
    }

    public void UpdateEndValue(float value, float velocity)
    {
        endValue = value;
        currentVelocity = velocity;
    }

    public float Evaluate(float deltaTime)
    {
        if (isFirstEvaluate)
        {
            Reset();
            isFirstEvaluate = false;
        }

        var c = damping;
        var m = mass;
        var k = stiffness;

        var x = currentValue;
        var v = currentVelocity;

        var steps = Mathf.Ceil(deltaTime / stepSize);
        for (var i = 0; i < steps; i++)
        {
            var dt = i == steps - 1 ? deltaTime - i * stepSize : stepSize;

            // springForce = -k * (x - endValue)
            // dampingForce = -c * v
            var a = (-k * (x - endValue) + -c * v) / m;
            v += a * dt;
            x += v * dt;
        }

        currentValue = x;
        currentVelocity = v;

        return currentValue;
    }
}