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
    private List<GameObject> allPlayers = new List<GameObject>();
    public float respawnWaitDuration;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        HideSpawnPoints();
        if (PhotonNetwork.IsConnected) {
            StartCoroutine(SpawnOnStart());
        }
    }

    #region Spawn Points
    private void HideSpawnPoints() {
        foreach (Transform spawnPoint in allSpawnPoints) {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    public Transform GetRandomSpawnPoint() {
        Transform randomSpawnPoint = allSpawnPoints[Random.Range(0, allSpawnPoints.Length)];
        GameObject closestPlayer = GetClosestPlayer(randomSpawnPoint);
        if (closestPlayer != null) {
            while(Vector3.Distance(randomSpawnPoint.position, closestPlayer.transform.position) < 5f) {
                randomSpawnPoint = allSpawnPoints[Random.Range(0, allSpawnPoints.Length)];
                closestPlayer = GetClosestPlayer(randomSpawnPoint);
                //Debug.Log("Choose new spawn someone close by");
            }
        }
        return randomSpawnPoint;
    }
    #endregion
    IEnumerator SpawnOnStart() {
        yield return new WaitForSeconds(1f);
        SpawnPlayer();
    }
    public void SpawnPlayer() {
        Transform spawnPoint = GetRandomSpawnPoint();
        player = PhotonNetwork.Instantiate(playerResourcesName,spawnPoint.position,spawnPoint.rotation);
        UIManager.instance.HideJoinGamePanel();
    }

    public void Die(string damager,int actor) {
        MatchManager.instance.UpdateStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 0, 1);
        MatchManager.instance.UpdateStatSend(actor, 1, 1);

        PhotonNetwork.Instantiate(deathEffectResourcesName, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);

        UIManager.instance.ShowDeathPanel(damager, respawnWaitDuration);
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn() {
        yield return new WaitForSeconds(respawnWaitDuration);
        if(MatchManager.instance.gameState != MatchManager.GameState.Ending) {
            SpawnPlayer();
        }
    }

    private GameObject GetClosestPlayer(Transform point) {
        float smallestDistance = int.MaxValue;
        GameObject closestPlayer = null;

        GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in playersArray) {
            float distance = Vector3.Distance(p.transform.position, point.position);
            if(distance < smallestDistance) {
                smallestDistance = distance;
                closestPlayer = p;
            }
        }
        return closestPlayer;
    }
}
