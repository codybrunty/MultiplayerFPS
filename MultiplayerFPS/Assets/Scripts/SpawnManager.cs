using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour{

    public static SpawnManager instance;
    public Transform[] allSpawnPoints;
    public string playerResourcesName;
    public string deathEffectResourcesName;
    private GameObject player;
    public float respawnWaitDuration;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        HideSpawnPoints();
        if (PhotonNetwork.IsConnected) {
            SpawnPlayer();
        }
    }

    #region Spawn Points
    private void HideSpawnPoints() {
        foreach (Transform spawnPoint in allSpawnPoints) {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    public Transform GetRandomSpawnPoint() {
        return allSpawnPoints[Random.Range(0,allSpawnPoints.Length)];
    }
    #endregion

    public void SpawnPlayer() {
        Transform spawnPoint = GetRandomSpawnPoint();
        player = PhotonNetwork.Instantiate(playerResourcesName,spawnPoint.position,spawnPoint.rotation);
    }

    public void Die(string damager) {
        PhotonNetwork.Instantiate(deathEffectResourcesName, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIManager.instance.ShowDeathPanel(damager,respawnWaitDuration);
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn() {
        yield return new WaitForSeconds(respawnWaitDuration);
        SpawnPlayer();
    }

}
