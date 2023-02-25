using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BulletGun : MonoBehaviour
{
    [Header("References")]
    public GameObject bullet;
    public Camera camFps;
    public Transform attackPoint;
    public Transform orientation;
    public ActionsControl playerActionsControl;
    [SerializeField]
    private Animator animator;

    [Header("Configurations")]
    public float shootForce;
    public float upwardForce;
    public float timeBetweenShooting;
    public float spread;
    public float reloadTime;
    public float timeBetweenShots;
    public int magazineSize;
    public int bulletsPerTap;
    public bool allowButtonHold;
    public int reloadValue;
    public int bulletsLeft;
    private int bulletsShot;
    private bool shooting;
    private bool readyToShoot;
    private bool reloading;

    [Header("Recoil")]
    public Rigidbody playerRigidBody;
    public float recoilForce;

    public bool allowInvoke = true;

    [Header("Graphics")]
    [SerializeField]
    private GameObject muzzleFlash;
    [SerializeField]
    private TextMeshProUGUI ammunitionDisplay;

    private void Start()
    {
        bulletsLeft = reloadValue;
        readyToShoot = true;
    }
    private void Update()
    {
        MyInput();
        if (ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
        }
    }

    private void MyInput()
    {
        if (allowButtonHold)
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (Input.GetKeyDown(KeyCode.R) && magazineSize > 0 && !reloading)
        {
            Reload();
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0 && magazineSize > 0)
        {
            Reload();
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;

            Shoot();
        }

        //Aiming
        // if (Input.GetMouseButtonDown(1) && !playerActionsControl.isAiming)
        // {
        //     playerActionsControl.isAiming = true;
        // }
        // else if (Input.GetMouseButtonDown(1) && playerActionsControl.isAiming)
        // {
        //     playerActionsControl.isAiming = false;
        // }
    }

    private void Shoot()
    {
        readyToShoot = false;

        Vector3 directionForce = camFps.transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(camFps.transform.position, camFps.transform.forward, out hit, 500f))
        {
            directionForce = (hit.point - attackPoint.position).normalized;
        }

        Vector3 directionWithoutSpread = directionForce * shootForce + transform.up * upwardForce;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        currentBullet.transform.forward = directionWithSpread.normalized;

        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread, ForceMode.Impulse);

        if (muzzleFlash != null)
        {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            Invoke("ResetShoot", timeBetweenShooting);
            allowInvoke = false;

            playerRigidBody.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
        if (ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
        }
    }

    private void ResetShoot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        animator.SetBool("Reload", true);
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        animator.SetBool("Reload", false);
        int actualMagazineSize = magazineSize;
        magazineSize -= reloadValue - bulletsLeft;
        bulletsLeft += reloadValue - bulletsLeft;
        if (magazineSize <= 0)
        {
            magazineSize = 0;
        }
        reloading = false;
    }

    public void GetAmmunation(int ammunation)
    {
        magazineSize += ammunation;
    }
}