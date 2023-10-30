using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Controls controls;
    CharacterController charCon;
    
    
    Vector3 ray;
    public GameObject bullet;
    public GameObject head;
    Camera cam;
    public float speed;
    float rotationX, rotationY;
    private readonly static float gravity = 9.8f;
    private float vForce = 0; 
    private float jumpForce = 7;
    Queue<GameObject> pool = new();
    public int limit;

    private void Awake() {
        charCon = GetComponent<CharacterController>();
        cam = head.GetComponent<Camera>();
        controls = new();
        controls.FPSPlayer.Jump.performed += Jump;
    }

    private void OnEnable() {
        controls.FPSPlayer.Enable();
    }

    private void OnDisable() {
        controls.FPSPlayer.Disable();
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        Rotation();
    }

    void FixedUpdate()
    {
        Move();
        OffGroundMove();
        Shoot();
    }

    private void Move() {
        Vector2 movement = controls.FPSPlayer.Movement.ReadValue<Vector2>();
        Vector3 direction = (transform.forward * movement.y + transform.right * movement.x).normalized;
        charCon.Move(direction * speed * Time.fixedDeltaTime);
    }

    private void Rotation() {
        Vector2 aim = controls.FPSPlayer.Aim.ReadValue<Vector2>();
        rotationX += aim.x;
        rotationY += aim.y;
        rotationY = Math.Clamp(rotationY, -70f, 70f);
        head.transform.localRotation = Quaternion.Euler(-rotationY, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, rotationX, 0f);
    }

    private void OffGroundMove() {

        vForce -= gravity * Time.deltaTime;
        charCon.Move(new(0, vForce * Time.deltaTime, 0));
        
    }

    private void Jump(InputAction.CallbackContext context) {
        if (charCon.isGrounded) {
            vForce = Mathf.Sqrt(jumpForce * -2 * -gravity);
        }
    }

    private void Shoot() {
        bool isShooting = controls.FPSPlayer.Shoot.IsPressed();

        if (isShooting) {
            CastRay();
        }

    }

    private void CastRay() {
        ray = cam.ViewportToWorldPoint(new(0.5f,0.5f,0));
        RaycastHit hit;

        if (Physics.Raycast(ray, cam.transform.forward, out hit, 100f))
        {
            InstantiateNewBullet(hit);
        }
    }

    public void InstantiateNewBullet(RaycastHit hit) {

        if (HasFreeSpace()) {
            pool.Enqueue(Instantiate(bullet, hit.point, Quaternion.identity));
        } else {
            GameObject go = pool.Dequeue();
            go.transform.position = hit.point;
            pool.Enqueue(go);
        }
    }

    private bool HasFreeSpace() { return pool.Count >= limit ? false : true;}
}
