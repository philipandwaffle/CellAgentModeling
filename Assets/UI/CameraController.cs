using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private float scrollSpeed = 0.25f;
    private Vector2 prevMouse;

    // Start is called before the first frame update
    void Start() {

        prevMouse = Input.mousePosition;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButton(1)) {
            Vector2 curMouse = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
            Vector3 translation = (Vector2)GetComponent<Camera>().ViewportToWorldPoint(prevMouse) - (Vector2)GetComponent<Camera>().ViewportToWorldPoint(curMouse);
            transform.transform.position += translation;
            prevMouse = curMouse;
        } else {
            prevMouse = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
        }

        GetComponent<Camera>().orthographicSize = Mathf.Max(0.1f, GetComponent<Camera>().orthographicSize - Input.mouseScrollDelta.y * scrollSpeed);
    }
}
