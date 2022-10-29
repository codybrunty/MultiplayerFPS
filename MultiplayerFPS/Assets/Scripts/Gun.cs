using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour{

    public string gunName;
    public bool isAutomatic;
    public float timeBetweenShots;
    public int ammo;
    public int currentAmmo;
    public float reloadDuration;
    public int damage;
    public GameObject model;
    public GameObject muzzleFlash;
    private Coroutine flashCo;

    private void Start() {
        currentAmmo = ammo;
    }

    public void ShowModel() {
        model.SetActive(true);
    }

    public void HideModel() {
        HideMuzzleFlash();
        model.SetActive(false);
    }

    public void ResetCurrentAmmo() {
        currentAmmo = ammo;
    }

    public void ReduceCurrentAmmo() {
        currentAmmo--;
    }

    public void GunFlash() {
        if (flashCo != null) { StopCoroutine(flashCo); }
        flashCo = StartCoroutine(Flash());
    }
    IEnumerator Flash() {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(.05f);
        muzzleFlash.SetActive(false);

    }
    public void HideMuzzleFlash() {
        if (flashCo != null) { StopCoroutine(flashCo); }
        muzzleFlash.SetActive(false);
    }

}
