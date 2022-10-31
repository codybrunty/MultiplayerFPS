using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour{

    public float lifetime;

    private void OnEnable() {
        StartCoroutine(DestroyObject());
    }

    private IEnumerator DestroyObject() {
        yield return new WaitForSeconds(lifetime);
        Destroy(this.gameObject);
    }

}
