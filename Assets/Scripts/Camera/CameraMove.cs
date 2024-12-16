using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private Camera cam;

    private void Awake() {
        cam = GetComponent<Camera>();
    }

    void Start()
    {

    }


    void Update()
    {
        RotateCameraTowardsCursor();
        MoveCamera();
    }

    void RotateCameraTowardsCursor()
    {
        
    }


    void MoveCamera()
    {
        float input = Input.GetAxis("Vertical");
        transform.position += transform.forward * input * moveSpeed * Time.deltaTime;
    }
}
