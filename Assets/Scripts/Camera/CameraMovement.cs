using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{
    private Camera cam;

    public float zoomStrength = 10f;
    public float panStrength = 100f;
    public float rotStrength = 0.5f;

    private Vector3 previousPosition;

    public bool allowMove = true;

    private void Awake() {
        cam = Camera.main;
    }

    void Start()
    {
    }


    private void Update()
    {
        if (Input.mouseScrollDelta.y != 0 && allowMove)
        {
            cam.transform.position += Input.mouseScrollDelta.y * cam.transform.forward * zoomStrength;
        }

        if (Input.GetMouseButtonDown(1) && allowMove)
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1) && allowMove)
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;
            direction *= rotStrength;

            float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically

            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

            previousPosition = newPosition;
        }

        if (Input.GetMouseButtonDown(0) && allowMove)
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && allowMove)
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 posDiff = previousPosition - newPosition;

            cam.transform.Translate(posDiff * panStrength);
            previousPosition = newPosition;

        }
    }

    public void SetAllowMove(Toggle toggle)
    {
        allowMove = toggle.isOn;
    }

}
