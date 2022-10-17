using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector2 prevMouse;
    private new Transform transform;
    private new Camera camera;

    // Start is called before the first frame update
    void Start() {
        transform = gameObject.GetComponent<Transform>();
        camera = gameObject.GetComponent<Camera>();
        prevMouse = Input.mousePosition;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButton(1)) {
            Vector2 curMouse = camera.ScreenToViewportPoint(Input.mousePosition);
            Vector3 translation = (Vector2)camera.ViewportToWorldPoint(prevMouse) - (Vector2)camera.ViewportToWorldPoint(curMouse);
            transform.transform.position += translation;
            prevMouse = curMouse;
        } else {
            prevMouse = camera.ScreenToViewportPoint(Input.mousePosition);
        }

        camera.orthographicSize = Mathf.Max(0.1f, camera.orthographicSize - Input.mouseScrollDelta.y * .25f);
    }
}
