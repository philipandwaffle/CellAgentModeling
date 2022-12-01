using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Assets.Environment {
    public class CAController : MonoBehaviour {
        [SerializeField] Sprite displaySprite;
        [SerializeField] public float scale;
        public Layer l;
        public GameObject[,] d;
        public float fps;
        public Vector2 offset = Vector2.zero;

        public void SetLayer(Layer l) {
            this.l = l;
            l.SetHoodFn(HoodFunctions.BoundedAvgSpread);
            SetD(l.w, l.h);
            UpdateDisplay(l.GetValues());
        }
        public void SetPoint(int x, int y, float val) {
            int lX = l.mW / 2 + x;
            int lY = l.mH / 2 + y;

            if (l.values[lX,lY] != val) {
                l.values[lX, lY] = val;
                d[x, y].GetComponent<SpriteRenderer>().color = GenColor(l.values[lX, lY]);
            }
        }

        private void SetD(int w, int h) {
            d = new GameObject[w, h];

            GameObject go = new GameObject();

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = displaySprite;

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    GameObject instance = Instantiate(go, offset + new Vector2(x, y) * scale, Quaternion.identity);
                    instance.transform.localScale = Vector3.one * scale;
                    instance.transform.parent = transform;
                    instance.name = "(" + x + "," + y + ")";
                    d[x, y] = instance;
                }
            }
            Destroy(go);
        }
        public void SetOffset(Vector2 offset) {
            this.offset = offset;
        }

        public void ResumeSim() {
            l.LoopMatrix();
            InvokeRepeating(nameof(ConvolveLayer), 0f, fps);
        }
        public void PauseSim() {
            CancelInvoke();
        }
                
        private void ConvolveLayer() {
            UpdateDisplay(l.Convolute());
        }
        private void UpdateDisplay(float[,] values) {
            for (int y = 0; y < l.h; y++) {
                for (int x = 0; x < l.w; x++) {
                    d[x, y].GetComponent<SpriteRenderer>().color =
                        GenColor(values[x, y]);
                }
            }
        }

        private Color GenColor(float val) {
            if (val == -1) {
                return new Color(0, 0, 0);
            } else {
                return Color.HSVToRGB(Mathf.Min(val, 1), 1, 1);
            }
        }
    }
}