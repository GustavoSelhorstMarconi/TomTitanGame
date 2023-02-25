using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControl : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField]
    private Transform targetRightHand;
    [SerializeField]
    private Transform targetLeftHand;

    private Vector3 startRightHandPosition;
    private Vector3 startLeftHandPosition;

    private void Start() {
        startRightHandPosition = targetRightHand.position;
        startLeftHandPosition = targetLeftHand.position;
    }
}