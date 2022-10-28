using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHole : MonoBehaviour{

    private void OnEnable() {
        StartCoroutine(DespawnBullet());
    }

    private IEnumerator DespawnBullet() {
        yield return new WaitForSeconds(5f);
        ObjectPoolingManager.instance.DespawnToPool("BulletHole", this.gameObject);
    }

}
