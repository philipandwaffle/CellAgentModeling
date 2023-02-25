using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Threading.Tasks;

namespace Assets.Environment {
    [Serializable]
    public class Layer {
        public float[,] data;
        public float[,] mask;

        public int w { get; private set; }
        public int h {get; private set;}
        private int mW, mH;
        private float mCount;

        private int xOffset;
        private int yOffset;
        private Color Display(float val) {
            if (val == -1) {
                return Color.black;
            }
            else if (val == -2) {
                return Color.green;
            }
            return Color.HSVToRGB((1-val /4f) - 0.75f, 0.7f, 0.5f);
        }
        private float Constrain(float val) {
            if (val == -1) {
                return -1;
            } else if (val == -2) {
                return -2;
            }
            return Mathf.Clamp(val, 0f, 1f);
        }

        public float this[int x, int y] {
            get {
                x = x <= 0 ? (x % w) + (w) : x % w;
                x = x == w ? 0 : x;

                y = y <= 0 ? (y % h) + (h) : y % h;
                y = y == h ? 0 : y;

                return data[x, y];
            }
            set {
                x = x <= 0 ? (x % w) + (w) : x % w;
                x = x == w ? 0 : x;

                y = y <= 0 ? (y % h) + (h) : y % h;
                y = y == h ? 0 : y;

                data[x, y] = value;
            }
        }

        [JsonConstructor]
        public Layer(float[,] data, float[,] mask) {
            InitVariables(mask);

            w = data.GetLength(0);
            h = data.GetLength(1);
            this.data = data;
        }

        public Layer(int w, int h, float[,] mask) {            
            InitVariables(mask);

            this.w = w;
            this.h = h;
            data = new float[w, h];
        }

        public void SetBorder(float val) {
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    if (i == 0 || i == w - 1 || j == 0 || j == h - 1) {
                        this[i, j] = val;
                    }
                }
            }
        }

        private void InitVariables(float[,] mask) {
            mW = mask.GetLength(0);
            mH = mask.GetLength(1);
            mCount = mW * mH;

            xOffset = mW / 2;
            yOffset = mH / 2;

            this.mask = mask;
        }

        public static Layer LoadLayer(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Layer>(json);
            }
        }        

        public Color GetDisplayData(int x, int y) {
            return Display(data[x, y]);
        }
        public void InsertValue(int x, int y, float value) {
            data[x, y] = Constrain(value);
        }

        public List<(int, int, float)> Advance() {
            float[,] newData = new float[w, h];
            List<(int, int, float)> layerBleed = new List<(int, int, float)>();
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    if (this[x, y] == -1) {
                        newData[x, y] = -1;
                    } else if (this[x, y] == -2) {
                        newData[x, y] = -2;
                        layerBleed.Add((x, y, DotProduct(x, y)));
                    } else {
                        newData[x, y] = Constrain(DotProduct(x, y));
                    }
                }
            }

            data = newData;
            return layerBleed;
        }
        private float DotProduct(int x, int y) { 
            float tempMCount = mCount;
            float total = 0f;

            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    float curVal = this[x + i - xOffset, y + j - yOffset];
                    float maskVal = mask[i, j];

                    if (curVal == -1 || curVal == -2) {
                        tempMCount -= 1f;
                    } else {
                        if (maskVal != 1) {
                            total += curVal * maskVal;
                        } else {
                            total += curVal;
                        }
                    }
                }
            }
            if (tempMCount == 0) {
                return -2;
            }
            return total / tempMCount;
        }

        private float[] AsPaddedFlattened() {

            float[] padded = new float[(w + 2) * (h + 2)];
            int i = 0;
            for (int x = -1; x <= w; x++) {
                for (int y = -1; y <= h; y++) {
                    int newX = x;
                    int newY = y;

                    LoopIndex(ref newX, w);
                    LoopIndex(ref newY, h);

                    padded[i] = data[newX, newY];
                    i++;
                }
            }

            return padded;
        }
        private void LoopIndex(ref int index, int max) {
            if (index == -1) {
                index = max - 1;
            } else if (index == max) {
                index = 0;
            }
        }

        public List<(int, int, float)> AdvanceGPU(ComputeShader cs) {
            // Get a padded flattened layer and create a buffer for it
            float[] padLayerData = AsPaddedFlattened();
            ComputeBuffer padLayerBuf = new ComputeBuffer(padLayerData.Length, sizeof(float));
            padLayerBuf.SetData(padLayerData);

            // Allocate a buffer for the advanced layer
            int layerLen = w * h;
            float[] newLayerData = new float[layerLen];
            ComputeBuffer newLayerBuf = new ComputeBuffer(layerLen, sizeof(float));

            // Set the buffer
            cs.SetBuffer(0, "paddedLayer", padLayerBuf);
            cs.SetBuffer(0, "newLayer", newLayerBuf);

            // Set the width and height
            cs.SetInt("w", w);
            cs.SetInt("h", h);

            // Set the padded width and height
            cs.SetInt("pw", w + 2);
            cs.SetInt("ph", h + 2);

            cs.Dispatch(0, padLayerData.Length / 64, 1, 1);

            // Get the new layer data
            newLayerBuf.GetData(newLayerData);

            //Dispose
            padLayerBuf.Dispose();
            newLayerBuf.Dispose();

            //Debug.Log()
            // Map the new layer array to the 2d data array
            /*Parallel.ForEach(newLayerData,(val,state,i) => {
                long y = i / w;
                long x = i - (y * h);

                data[y, x] = val;
            });*/
            for (int i = 0; i < newLayerData.Length; i++) {
                float val = newLayerData[i];

                int y = i / w;
                int x = i - (y * h);

                data[y, x] = val;
            }

            return new List<(int, int, float)> { };
        }

        public void Save(string path, Formatting format = Formatting.None) {
            string json = JsonConvert.SerializeObject(this, format);            
            using (StreamWriter sr = new StreamWriter(path, false)) { 
                sr.WriteLine(json);
            }
        }

        public Layer DeepClone() {
            using (var ms = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;

                return (Layer)formatter.Deserialize(ms);
            }
        }
    }
}