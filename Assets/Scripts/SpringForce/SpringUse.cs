using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spring = SpringCode;
public class SpringUse : MonoBehaviour
{
    [Header("Values X")]
    public float dampingX = 26f;
    public float massX = 1f;
    public float stiffnessX = 169f;
    public float startValueX;
    public float endValueX;
    public float initialVelocityX;
    [Header("Values Y")]
    public float dampingY = 26f;
    public float massY = 1f;
    public float stiffnessY = 169f;
    public float startValueY;
    public float endValueY;
    public float initialVelocityY;
    [Header("Values Z")]
    public float dampingZ = 26f;
    public float massZ = 1f;
    public float stiffnessZ = 169f;
    public float startValueZ;
    public float endValueZ;
    public float initialVelocityZ;
    Spring springX;
    Spring springY;
    Spring springZ;
    void Start()
    {
        // interpolate from -10f to 10f
        springX = new Spring()
        {
            startValue = startValueX,
            endValue = endValueX,
            damping = dampingX,
            mass = massX,
            stiffness = stiffnessX
        };

        springY = new Spring()
        {
            startValue = startValueY,
            endValue = endValueY,
            damping = dampingY,
            mass = massY,
            stiffness = stiffnessY
        };

        springZ = new Spring()
        {
            startValue = startValueZ,
            endValue = endValueZ,
            damping = dampingZ,
            mass = massZ,
            stiffness = stiffnessZ
        };
    }

    void Update()
    {
        var x = springX.Evaluate(Time.deltaTime);
        var y = springY.Evaluate(Time.deltaTime);
        var z = springZ.Evaluate(Time.deltaTime);
        transform.position = new Vector3(x, 0, 0);
    }
}