using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunController : MonoBehaviour{

    [Header("Gun Settings")]
    public float timeBetweenShots;
    public int ammo;
    public int reloadDuration;
    private bool isReloading;
    private int currentAmmo;
    private float shotCounter;
    private Camera cam;
    private ObjectPoolingManager pool;
    private TextMeshProUGUI ammoText;

    private void Start() {
        cam = Camera.main;
        pool = ObjectPoolingManager.instance;
        ammoText = UIManager.instance.ammoText;
        shotCounter = 0f;
        currentAmmo = ammo;
        UpdateAmmoUI();
    }

    private void Update() {
        if(shotCounter > 0) {
            shotCounter-= Time.deltaTime;
        }
    }

    public void Shoot() {
        if(shotCounter > 0) { return; }
        if(isReloading) { return; }
        if(currentAmmo <= 0) { ReloadWeapon(); return; }
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            pool.SpawnFromPool("BulletHole", hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
        }
        shotCounter = timeBetweenShots;
        currentAmmo--;
        UpdateAmmoUI();
    }

    private void ReloadWeapon() {
        isReloading = true;
        StartCoroutine(ReloadWeaponOverTime());
    }

    IEnumerator ReloadWeaponOverTime() {
        StartCoroutine(ReloadAmmoUIAnimation());
        yield return new WaitForSeconds(reloadDuration);
        currentAmmo = ammo;
        UpdateAmmoUI();
        isReloading = false;
    }

    IEnumerator ReloadAmmoUIAnimation() {
        ammoText.color = Color.red;
        for (float t = 0f; t < reloadDuration; t+=Time.deltaTime) {
            ammoText.text = "Reload: "+((int)Mathf.Lerp(0,ammo,t/reloadDuration)).ToString();
            yield return null;
        }
    }

    private void UpdateAmmoUI() {
        ammoText.color = Color.white;
        ammoText.text = "Ammo: " + currentAmmo.ToString();
    }

}
