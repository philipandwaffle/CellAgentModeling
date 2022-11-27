using System.Collections;
using UnityEngine;

namespace Assets.Environment.Refactor {
    public class LayerEditor : MonoBehaviour {
        private LayerTicker ticker;

        private float brushVal = -1;
        private Vector2Int prevMouse = new Vector2Int(-1, -1);
        [SerializeField] private bool paused = true;

        // Use this for initialization
        void Start() {
            ticker = gameObject.GetComponent<LayerTicker>();

            string path = Application.dataPath + "/Layers/test.json";
            ticker.LoadLayer(path);
        }

        // Update is called once per frame
        void Update() {
            if (!paused) {
                ticker.AdvanceLayer();
            }
        }

        public void 
    }
}