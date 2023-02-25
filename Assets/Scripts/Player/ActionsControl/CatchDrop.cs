using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CatchDrop : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform camTransform;
    [SerializeField]
    private GameObject text;
    [SerializeField]
    private Transform itemHolder;

    [Header("Values")]
    [SerializeField]
    private float dropForceForward;
    [SerializeField]
    private float dropForceUpward;

    [Header("Configurations")]
    [SerializeField]
    private float radiusSphereCast;
    [SerializeField]
    private float distanceCast;
    [SerializeField]
    private LayerMask whatIsCatchable;
    [SerializeField]
    private KeyCode catchKey = KeyCode.F;
    [SerializeField]
    private KeyCode dropKey = KeyCode.Q;

    private bool canCatch;
    private bool equipped;
    private RaycastHit hitItemInfo;
    private Transform item;

    private void Update()
    {
        CheckForCatchableItems();
        if (Input.GetKeyDown(catchKey) && canCatch && !equipped)
        {
            Catch();
        }
        if (Input.GetKeyDown(dropKey) && equipped)
        {
            Drop();
        }
    }

    private void CheckForCatchableItems()
    {
        if (Physics.SphereCast(camTransform.position, radiusSphereCast, camTransform.forward, out hitItemInfo, distanceCast, whatIsCatchable) && !equipped)
        {
            canCatch = true;
            text.SetActive(true);
        }
        else
        {
            canCatch = false;
            text.SetActive(false);
        }
    }

    private void Catch()
    {
        equipped = true;
        canCatch = false;

        item = hitItemInfo.transform;
        item.SetParent(itemHolder);
        item.localPosition = Vector3.zero;
        item.forward = itemHolder.forward;
        item.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void Drop()
    {
        equipped = false;

        item.GetComponent<Rigidbody>().isKinematic = false;
        item.SetParent(null);
        Vector3 forceToAdd = camTransform.forward * dropForceForward + transform.up * dropForceUpward;
        item.GetComponent<Rigidbody>().AddForce(forceToAdd, ForceMode.Impulse);
    }
}