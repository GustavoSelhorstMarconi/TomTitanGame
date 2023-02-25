using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammunation : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField]
    private int ammunation;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.parent.GetComponent<PlayerMovement>())
        {
            return;
        }
        if (other.transform.Find("GunContainer").childCount > 0)
        {
            other.transform.Find("GunContainer").GetChild(0).GetComponent<BulletGun>().GetAmmunation(ammunation);
            Destroy(gameObject);
        }
    }
}