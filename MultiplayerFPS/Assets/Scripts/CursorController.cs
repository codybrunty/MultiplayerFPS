using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour{

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
        }
        if(Cursor.lockState == CursorLockMode.None) {
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
