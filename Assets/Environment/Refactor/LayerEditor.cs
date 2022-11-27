using System.Collections;
using Unity.Burst;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Environment.Refactor {
    public class LayerEditor : MonoBehaviour {
        private LayerTicker<float> ticker;

        private float brushVal = -1;
        private Vector2Int editPos = new Vector2Int(-1, -1);
        [SerializeField] private bool paused = true;

        // Use this for initialization
        void Start() {
            ticker = gameObject.GetComponent<LayerTicker<float>>();

            string path = Application.dataPath + "/Layers/test.json";
            ticker.LoadLayer(path);
        }

        // Update is called once per frame
        void Update() {
            if (!paused) {
                ticker.AdvanceLayer();
            }

            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float xScale = transform.localScale.x;
                float yScale = transform.localScale.y;
                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > ticker.layer.w * xScale || mPos.y < 0 || mPos.y > ticker.layer.h * yScale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curEditPos = new Vector2Int((int)(mPos.x / xScale), (int)(mPos.y / yScale));

                // If the mouse has moved
                if (editPos != curEditPos) {
                    editPos = curEditPos;
                    ticker.layer[curEditPos.x, curEditPos.y] = brushVal;                    
                }
            }
        }

        public void TogglePaused() {
            paused = !paused;
        }

        public void SetBrushValue(float val) {
            brushVal = val;
        }
    }
}