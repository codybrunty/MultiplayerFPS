using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class GunController : MonoBehaviourPunCallbacks {

    [Header("Gun Settings")]
    private bool isReloading;
    private float shotCounter;
    private Camera cam;
    private ObjectPoolingManager pool;
    private TextMeshProUGUI ammoText;
    public Gun[] allGuns;
    private int currentGun;
    private Coroutine reloadCo;
    private Coroutine reloadAnimationCo;
    private float targetFOV;
    public float zoomSpeed;
    public string bulletHoleResourcesName;
    public string bulletPlayerImpactResourcesName;
    private PlayerController playerController;
    [SerializeField] Transform gunHolder;
    [SerializeField] Transform gunPoint;

    private void Start() {
        cam = Camera.main;
        pool = ObjectPoolingManager.instance;
        playerController = gameObject.GetComponent<PlayerController>();
        UpdateGunDisplay();
        if (photonView.IsMine) {
            ammoText = UIManager.instance.ammoText;
            shotCounter = 0f;
            allGuns[currentGun].currentAmmo = allGuns[currentGun].ammo;
            ZoomOut();
            UpdateUI();
        }
        else {
            gunHolder.parent = gunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }

    private void Update() {
        if (!photonView.IsMine) { return; }
        CheckForShot();
        CheckForReload();
        CheckForWeaponSwitch();
        CheckForZoom();
        CountdownTimeBetweenShots();
    }

    #region Shoot
    private void CheckForShot() {
        if (Input.GetMouseButtonDown(0)) {
            Shoot();
        }
        if (Input.GetMouseButton(0) && allGuns[currentGun].isAutomatic) {
            Shoot();
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
            if(hit.collider.tag == "Player") {
                PhotonNetwork.Instantiate(bulletPlayerImpactResourcesName, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("TakeDamage",RpcTarget.All, allGuns[currentGun].damage, photonView.Owner.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else {
                PhotonNetwork.Instantiate(bulletHoleResourcesName, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                //pool.SpawnFromPool("BulletHole", hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            }
        }
        shotCounter = allGuns[currentGun].timeBetweenShots;
        allGuns[currentGun].ReduceCurrentAmmo();
        UpdateUI();
    }

    private void CountdownTimeBetweenShots() {
        if (shotCounter > 0) {
            shotCounter -= Time.deltaTime;
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, string damager, int actor) {
        playerController.TakeDamage(damage, damager, actor);
    }

    #endregion

    #region Reload

    private void CheckForReload() {
        if (isReloading) { return; }
        if (allGuns[currentGun].currentAmmo == allGuns[currentGun].ammo) { return; }
        if (Input.GetKeyDown(KeyCode.R)) {
            ReloadWeapon();
        }
    }

    private void ReloadWeapon() {
        isReloading = true;
        if (reloadCo != null) { StopCoroutine(reloadCo); }
        reloadCo = StartCoroutine(ReloadWeaponOverTime());
    }

    IEnumerator ReloadWeaponOverTime() {
        if (reloadAnimationCo != null) { StopCoroutine(reloadAnimationCo); }
        //Less bullets to reload less time it takes
        float totalReloadTime = (allGuns[currentGun].reloadDuration / allGuns[currentGun].ammo) * (allGuns[currentGun].ammo - allGuns[currentGun].currentAmmo);
        reloadAnimationCo = StartCoroutine(ReloadAmmoUIAnimation(totalReloadTime));
        yield return new WaitForSeconds(totalReloadTime);
        allGuns[currentGun].ResetCurrentAmmo();
        UpdateUI();
        isReloading = false;
    }

    IEnumerator ReloadAmmoUIAnimation(float duration) {
        ammoText.color = Color.red;
        int startAmmo = allGuns[currentGun].currentAmmo;
        for (float t = 0f; t < duration; t+=Time.deltaTime) {
            ammoText.text = ((int)Mathf.Lerp(startAmmo, allGuns[currentGun].ammo,t/ duration)).ToString() + " / " + allGuns[currentGun].ammo.ToString();
            yield return null;
        }
    }
    #endregion

    #region UI
    private void UpdateUI() {
        ammoText.color = Color.white;
        ammoText.text = allGuns[currentGun].currentAmmo.ToString() + " / " + allGuns[currentGun].ammo.ToString();
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
    private void CheckForWeaponSwitch() {
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
        if (Input.GetKeyDown("1")) {
            if (currentGun != 0) {
                SwitchWeapon(0);
            }
        }
        if (Input.GetKeyDown("2")) {
            if (currentGun != 1) {
                SwitchWeapon(1);
            }
        }
        if (Input.GetKeyDown("3")) {
            if (currentGun != 2) {
                SwitchWeapon(2);
            }
        }
    }

    private void NextWeapon() {
        int currentIndex = currentGun;
        currentIndex++;
        if (currentIndex >= allGuns.Length) {
            currentIndex = 0;
        }
        SwitchWeapon(currentIndex);
    }

    private void PreviousWeapon() {
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
        photonView.RPC("SetGunOnNetwork",RpcTarget.All,currentGun);
    }

    [PunRPC]
    public void SetGunOnNetwork(int gun) {
        currentGun = gun;
        UpdateGunDisplay();
    }
    #endregion

    #region Zoom
    private void CheckForZoom() {
        if (Input.GetMouseButtonDown(1)) {
            ZoomIn();
        }
        if (Input.GetMouseButtonUp(1)) {
            ZoomOut();
        }
        LerpToZoomPosition();
    }
    private void ZoomIn() {
        targetFOV = allGuns[currentGun].zoomedFOV;
    }
    private void ZoomOut() {
        targetFOV = 60;
    }

    private void LerpToZoomPosition() {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,targetFOV,Time.deltaTime*zoomSpeed);
    }

    #endregion

}
