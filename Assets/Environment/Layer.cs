using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Assets.Environment {
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class Layer {
        [JsonProperty]
        public float[,] data;

        public NavGraph navGraph;

        public int w { get; private set; }
        public int h {get; private set;}

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

        public float this[int y, int x] {
            get {
                x = x <= 0 ? (x % w) + (w) : x % w;
                x = x == w ? 0 : x;

                y = y <= 0 ? (y % h) + (h) : y % h;
                y = y == h ? 0 : y;

                return data[y, x];
            }
            set {
                x = x <= 0 ? (x % w) + (w) : x % w;
                x = x == w ? 0 : x;

                y = y <= 0 ? (y % h) + (h) : y % h;
                y = y == h ? 0 : y;

                data[y, x] = value;
            }
        }

        [JsonConstructor]
        public Layer(float[,] data, float[,] mask) {
            w = data.GetLength(0);
            h = data.GetLength(1);
            this.data = data;
        }

        public Layer(int w, int h, float[,] mask) {
            this.w = w;
            this.h = h;
            data = new float[h, w];
        }

        public void SetBorder(float val) {
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (y == 0 || y == w - 1 || x == 0 || x == h - 1) {
                        this[y, x] = val;
                    }
                }
            }
        }

        public static Layer LoadLayer(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Layer>(json);
            }
        }        

        public Color GetDisplayData(int y, int x) {
            return Display(data[y, x]);
        }
        public void InsertValue(int y, int x, float value) {
            data[y, x] = Constrain(value);
        }

        public float[] AsPaddedFlattened() {
            int paddedCount = (this.w + 2) * (this.h + 2);
            float[] padded = new float[paddedCount];
            int i = 0;

            int h = this.h - 1;
            int w = this.w - 1;

            // first row of padded
            padded[i] = data[h, w];
            i++;
            for (int x = 0; x < this.w; x++) {
                padded[i] = data[h, x];
                i++;
            }
            padded[i] = data[h, 0];
            i++;

            // internal rows in padded
            for (int y = 0; y < this.h; y++) {
                padded[i] = data[y, w];
                i++;
                for (int x = 0; x < this.w; x++) {
                    padded[i] = data[y, x];
                    i++;
                }
                padded[i] = data[y, 0];
                i++;
            }

            // last row of padded
            padded[i] = data[0, w];
            i++;
            for (int x = 0; x < this.w; x++) {
                padded[i] = data[0, x];
                i++;
            }
            padded[i] = data[0, 0];
            i++;

            if (i != paddedCount) {
                Debug.LogError("Impropper padding/flattening of layer");
            }
            return padded;
        }

        public Bleed[] AdvanceGPU(ComputeShader cs, float[] padLayerData) {
            // Get a padded flattened layer and create a buffer for it
            ComputeBuffer padLayerBuf = new ComputeBuffer(padLayerData.Length, sizeof(float));
            padLayerBuf.SetData(padLayerData);

            // Allocate a buffer for the advanced layer
            int layerLen = w * h;
            float[] newLayerData = new float[layerLen];
            ComputeBuffer newLayerBuf = new ComputeBuffer(layerLen, sizeof(float));

            // Allocate a buffer for the advanced layer
            int bleedCount = 10;
            Bleed[] bleed = Bleed.GetDataHolder(bleedCount);
            ComputeBuffer bleedBuf = new ComputeBuffer(bleedCount, sizeof(float) + (2 * sizeof(int)));
            bleedBuf.SetData(bleed);

            // Set the buffer
            cs.SetBuffer(0, "paddedLayer", padLayerBuf);
            cs.SetBuffer(0, "newLayer", newLayerBuf);
            cs.SetBuffer(0, "layerBleed", bleedBuf);

            // Set modifier
            cs.SetFloat("bleedModifier", .5f);

            // Set the width and height
            cs.SetInt("w", w);
            cs.SetInt("h", h);

            // Set the padded width and height
            cs.SetInt("pw", w + 2);
            cs.SetInt("ph", h + 2);

            //cs.SetInt("bleedIndex", 0);

            cs.Dispatch(0, padLayerData.Length / 64, 1, 1);

            // Get the new layer data
            newLayerBuf.GetData(newLayerData);
            bleedBuf.GetData(bleed);

            //Dispose
            padLayerBuf.Dispose();
            newLayerBuf.Dispose();
            bleedBuf.Dispose();

            for (int i = 0; i < newLayerData.Length; i++) {
                float val = newLayerData[i];

                int y = i / w;
                int x = i - (y * h);

                data[y, x] = val;
            }

            return bleed;
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