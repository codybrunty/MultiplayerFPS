using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour{

    public static ObjectPoolingManager instance;
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> dictPool;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        CreatePools();
    }

    private void CreatePools() {
        dictPool = new Dictionary<string, Queue<GameObject>>();
        foreach (Pool pool in pools) {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.startSize; i++) {
                GameObject obj = Instantiate(pool.prefab, gameObject.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            dictPool.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) {
        if (!dictPool.ContainsKey(tag)) { Debug.Log("Pool tag(" + tag + ") doesn't exist in Pool Dictionary"); return null; }
        GameObject objToSpawn;
        if (dictPool[tag].Count < 1 || dictPool[tag].Peek().activeInHierarchy) {
            objToSpawn = Instantiate(GetPrefabFromPool(tag), gameObject.transform);
            objToSpawn.SetActive(false);
        }
        else {
            objToSpawn = dictPool[tag].Dequeue();
        }
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);
        return objToSpawn;
    }
    public void DespawnToPool(string tag, GameObject obj) {
        obj.SetActive(false);
        dictPool[tag].Enqueue(obj);
    }

    private GameObject GetPrefabFromPool(string tag) {
        foreach (Pool pool in pools) {
            if (pool.tag == tag) {
                return pool.prefab;
            }
        }
        return null;
    }

}

[System.Serializable]
public class Pool{
    public string tag;
    public GameObject prefab;
    public int startSize;
}
