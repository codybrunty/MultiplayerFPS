using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnOverTime : MonoBehaviour{

    public string objectPoolName;
    public float lifetime;

    private void OnEnable() {
        StartCoroutine(DespawnObject());
    }

    private IEnumerator DespawnObject() {
        yield return new WaitForSeconds(lifetime);
        ObjectPoolingManager.instance.DespawnToPool(objectPoolName, this.gameObject);
    }

}
