using UnityEngine;
using UnityEngine.EventSystems;

public class TerraformingCamera : MonoBehaviour
{
    Vector3 hitPoint;
    Camera cam;
    public float brushSize = 2f;
    public TerrainManager terrainManager;

    private void Awake() {
        cam = Camera.main;
    }

    private void LateUpdate() {
        if (Input.GetMouseButtonDown(0) && terrainManager.allowTerraforming)
        {
            Terraform(true);
        }
        else if (Input.GetMouseButtonDown(1) && terrainManager.allowTerraforming)
        {
            Terraform(false);
        }
    }

    private void Terraform(bool add) {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Terrain");

        // only terraform if raycast hit "terrain" layer and pointer is NOT over UI element
        if (Physics.Raycast(ray, out hit, 1000, layerMask) && !EventSystem.current.IsPointerOverGameObject()) {
            hitPoint = hit.point;
            terrainManager.ModifyTerrain(hitPoint, brushSize, add);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(hitPoint, Vector3.one * brushSize);
    }

}
