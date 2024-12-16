using UnityEngine;

public class TerraformingCamera : MonoBehaviour
{
    Vector3 hitPoint;
    Camera cam;
    public float brushSize = 2f;

    private void Awake() {
        cam = Camera.main;
    }

    private void LateUpdate() {
        if (Input.GetMouseButton(0))
        {
            Terraform(true);
        }
        else if (Input.GetMouseButton(1))
        {
            Terraform(false);
        }
    }

    private void Terraform(bool add) {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(ray, out hit, 1000, layerMask)) {
            Transform objectHit = hit.transform;
            hitPoint = hit.point;

            objectHit.GetComponent<ChunkManager>().OnRaycastHit(hitPoint, brushSize, add);
        } else {
            Debug.Log("miss");
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hitPoint, brushSize);
    }

}
