using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PickUpGun : MonoBehaviour
{
    public BulletGun gunControl;
    public Transform player, gunContainer;
    public Rigidbody gunRigidbody;
    public BoxCollider colliderBox;
    public GameObject aim;
    public KeyCode pickUpKey = KeyCode.F;
    [SerializeField]
    private UnityEvent<bool> OnTakeLeftGun;
    private Transform cam;

    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    public bool equipped = false;
    public static bool slotFull;

    private void Start()
    {
        gunControl.enabled = false;
        gunRigidbody.isKinematic = false;
        slotFull = false;
        // aim.SetActive(equipped);
        cam = Camera.main.transform;
    }

    private void Update()
    {
        Vector3 distanceToPlayer = player.position - transform.position;
        if (!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(pickUpKey) && !slotFull)
        {
            PickUp();
            // aim.SetActive(true);
        }
        if (equipped && Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
            // aim.SetActive(false);
        }
        if (equipped)
        {
            transform.localPosition = Vector3.zero;
        }
    }

    private void PickUp()
    {
        equipped = true;
        slotFull = true;

        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        gunRigidbody.isKinematic = true;
        colliderBox.isTrigger = true;

        gunControl.enabled = true;
        player.GetComponent<PlayerMovement>().gunEquipped = true;
        OnTakeLeftGun?.Invoke(true);
    }

    private void Drop()
    {
        equipped = false;
        slotFull = false;

        transform.SetParent(null);

        gunRigidbody.isKinematic = false;
        colliderBox.isTrigger = false;
        gunRigidbody.velocity = player.GetComponent<Rigidbody>().velocity;

        gunRigidbody.AddForce(cam.forward * dropForwardForce, ForceMode.Impulse);
        gunRigidbody.AddForce(player.up * dropUpwardForce, ForceMode.Impulse);

        float randomRotation = Random.Range(-1f, 1f);
        gunRigidbody.AddTorque(new Vector3(randomRotation, randomRotation, randomRotation) * 10);

        gunControl.enabled = false;
        player.GetComponent<PlayerMovement>().gunEquipped = false;
        OnTakeLeftGun?.Invoke(false);
    }
}
