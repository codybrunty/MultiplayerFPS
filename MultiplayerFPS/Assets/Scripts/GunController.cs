using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunController : MonoBehaviour {

    [Header("Gun Settings")]
    private bool isReloading;
    private float shotCounter;
    private Camera cam;
    private ObjectPoolingManager pool;
    private TextMeshProUGUI ammoText;
    private TextMeshProUGUI gunNameText;
    public Gun[] allGuns;
    private int currentGun;
    private Coroutine reloadCo;
    private Coroutine reloadAnimationCo;

    private void Start() {
        cam = Camera.main;
        pool = ObjectPoolingManager.instance;
        ammoText = UIManager.instance.ammoText;
        gunNameText = UIManager.instance.gunNameText;
        shotCounter = 0f;
        allGuns[currentGun].currentAmmo = allGuns[currentGun].ammo;
        UpdateUI();
        UpdateGunDisplay();
    }

    private void Update() {
        CheckIsShooting();

        if (shotCounter > 0) {
            shotCounter-= Time.deltaTime;
        }
    }

    #region Shoot
    private void CheckIsShooting() {
        if (Input.GetMouseButtonDown(0)) {
            Shoot();
        }
        if (Input.GetMouseButton(0) && allGuns[currentGun].isAutomatic) {
            Shoot();
        }
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                PreviousWeapon();
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                NextWeapon();
            }
        }


    }

    public void Shoot() {
        if(shotCounter > 0) { return; }
        if(isReloading) { return; }
        if(allGuns[currentGun].currentAmmo <= 0) { ReloadWeapon(); return; }
        allGuns[currentGun].GunFlash();
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            pool.SpawnFromPool("BulletHole", hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
        }
        shotCounter = allGuns[currentGun].timeBetweenShots;
        allGuns[currentGun].ReduceCurrentAmmo();
        UpdateUI();
    }
    #endregion

    #region Reload
    private void ReloadWeapon() {
        isReloading = true;
        if (reloadCo != null) { StopCoroutine(reloadCo); }
        reloadCo = StartCoroutine(ReloadWeaponOverTime());
    }

    IEnumerator ReloadWeaponOverTime() {
        if (reloadAnimationCo != null) { StopCoroutine(reloadAnimationCo); }
        reloadAnimationCo = StartCoroutine(ReloadAmmoUIAnimation());
        yield return new WaitForSeconds(allGuns[currentGun].reloadDuration);
        allGuns[currentGun].ResetCurrentAmmo();
        UpdateUI();
        isReloading = false;
    }

    IEnumerator ReloadAmmoUIAnimation() {
        ammoText.color = Color.red;
        for (float t = 0f; t < allGuns[currentGun].reloadDuration; t+=Time.deltaTime) {
            ammoText.text = "Reload: "+((int)Mathf.Lerp(0, allGuns[currentGun].ammo,t/ allGuns[currentGun].reloadDuration)).ToString();
            yield return null;
        }
    }
    #endregion

    #region UI
    private void UpdateUI() {
        gunNameText.text = allGuns[currentGun].gunName;
        ammoText.color = Color.white;
        ammoText.text = "Ammo: " + allGuns[currentGun].currentAmmo.ToString();
    }
    #endregion

    #region Gun Display
    private void UpdateGunDisplay() {
        for (int i = 0; i < allGuns.Length; i++) {
            allGuns[i].HideModel();
        }
        allGuns[currentGun].ShowModel();
    }
    #endregion

    #region Switch Weapon
    public void NextWeapon() {
        int currentIndex = currentGun;
        currentIndex++;
        if (currentIndex >= allGuns.Length) {
            currentIndex = 0;
        }
        SwitchWeapon(currentIndex);
    }

    public void PreviousWeapon() {
        int currentIndex = currentGun;
        currentIndex--;
        if (currentIndex < 0) {
            currentIndex = allGuns.Length-1;
        }
        SwitchWeapon(currentIndex);
    }

    private void SwitchWeapon(int newGun) {
        if (reloadCo != null) { StopCoroutine(reloadCo); }
        if (reloadAnimationCo != null) { StopCoroutine(reloadAnimationCo); }
        isReloading = false;
        currentGun = newGun;
        UpdateUI();
        UpdateGunDisplay();
    }
    #endregion

}
