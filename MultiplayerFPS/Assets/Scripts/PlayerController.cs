using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    [Header("References")]
    [SerializeField] private Transform viewPoint;
    private Camera cam;
    private CharacterController characterController;
    [SerializeField] private Transform groundCheckPoint;
    [Header("Mouse Settings")]
    public bool invertedLook;
    public float mouseSensitivity;
    private Vector2 mouseInput;
    private float verticalRotationStore;
    [Header("Move Settings")]
    public float moveSpeed;
    public float runSpeed;
    private float acitveMoveSpeed;
    private Vector3 moveDirection;
    private Vector3 movement;
    [Header("Jump Settings")]
    public float gravityMod;
    public float jumpForce;
    private bool isGrounded;
    public LayerMask groundLayers;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    private void Update() {
        PlayerRotation();
        PlayerMovement();
    }

    private void LateUpdate() {
        SetCameraPosition();
        SetCameraRotation();
    }

    #region Translation
    private void PlayerMovement() {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        acitveMoveSpeed = GetSpeed();
        float yVelocity = movement.y;
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * acitveMoveSpeed;
        movement.y = yVelocity;
        if (characterController.isGrounded) {
            movement.y = 0f;
        }

        GroundCheck();
        if (Input.GetButtonDown("Jump") && isGrounded) {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        characterController.Move(movement * Time.deltaTime);
    }

    private void GroundCheck() {
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);
    }

    private float GetSpeed() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            return runSpeed;
        }
        return moveSpeed;
    }

    #endregion

    #region Rotation
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
    #endregion

    #region Camera
    private void SetCameraPosition() {
        cam.transform.position = viewPoint.position;
    }
    private void SetCameraRotation() {
        cam.transform.rotation = viewPoint.rotation;
    }
    #endregion

}
