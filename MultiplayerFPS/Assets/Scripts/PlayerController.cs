using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    [Header("References")]
    [SerializeField] private Transform viewPoint;
    [Header("Mouse Settings")]
    public bool invertedLook;
    public float mouseSensitivity;
    private Vector2 mouseInput;
    private float verticalRotationStore;
    [Header("Move Settings")]
    public float moveSpeed;
    private Vector3 moveDirection;
    private Vector3 movement;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        PlayerRotation();
        PlayerMovement();
    }

    private void PlayerMovement() {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"),0f,Input.GetAxisRaw("Vertical"));
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void PlayerRotation() {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z));
        verticalRotationStore += mouseInput.y;
        verticalRotationStore = Mathf.Clamp(verticalRotationStore, -60f, 60f);
        if (invertedLook) {
            viewPoint.rotation = Quaternion.Euler(new Vector3(verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z));
        }
        else {
            viewPoint.rotation = Quaternion.Euler(new Vector3(-verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z));
        }
    }
}
