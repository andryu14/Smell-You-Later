using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridTest : MonoBehaviour {
    private Pathfinder pathfinder;

    void Start() {
        pathfinder = new Pathfinder(18, 10);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinder.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            List<PathNode> path = pathfinder.FindPath(3, 7, x, y);
            /* if (path != null) {
                for (int i = 0; i < path.Count - 1; i++) {
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y), new Vector3(path[i + 1].x, path[i + 1].y), Color.green, 5f);
                }
            } */
        }
    }

    public static Vector3 GetMouseWorldPosition() {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public static Vector3 GetMouseWorldPositionWithZ() {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera) {
        return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera) {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}