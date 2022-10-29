using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour{

    public static SpawnManager instance;
    public Transform[] allSpawnPoints;
    private void Awake() {
        instance = this;
    }

    private void Start() {
        HideSpawnPoints();
    }

    private void HideSpawnPoints() {
        foreach (Transform spawnPoint in allSpawnPoints) {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    public Transform GetRandomSpawnPoint() {
        return allSpawnPoints[Random.Range(0,allSpawnPoints.Length)];
    }
}
