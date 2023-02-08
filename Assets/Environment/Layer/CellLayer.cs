using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Assets.Environment {
    [Serializable]
    public class CellLayer {
        public Cell[,] cells;
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

        public float? this[int x, int y] {
            get {
                x = x <= 0 ? (x % w) + (w) : x % w;
                x = x == w ? 0 : x;

                y = y <= 0 ? (y % h) + (h) : y % h;
                y = y == h ? 0 : y;

                return cells[x, y].GetValue();
            }
            set {
                x = x <= 0 ? (x % w) + (w) : x % w;
                x = x == w ? 0 : x;

                y = y <= 0 ? (y % h) + (h) : y % h;
                y = y == h ? 0 : y;

                cells[x, y].SetVal(value.Value);
            }
        }

        [JsonConstructor]
        public CellLayer(Cell[,] cells, float[,] mask) {
            InitVariables(mask);

            w = cells.GetLength(0);
            h = cells.GetLength(1);
            this.cells = cells;
        }

        private void SpawnCells() {
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    
                }
            }
        }

        public CellLayer(int w, int h, float[,] mask) {            
            InitVariables(mask);

            this.w = w;
            this.h = h;
            cells = new Cell[w, h];
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

        public void InsertValue(int x, int y, float value) {
            this[x, y] = value;
        }

        public List<(int, int, float)> Advance() {
            Cell[,] newData = new Cell[w, h];
            List<(int, int, float)> layerBleed = new List<(int, int, float)>();
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    if (this[x, y] == -1) {
                        newData[x, y].SetVal(-1);
                    } else if (this[x, y] == -2) {
                        newData[x, y].SetVal(-2);
                        layerBleed.Add((x, y, DotProduct(x, y)));
                    } else {
                        newData[x, y].SetVal(DotProduct(x, y));
                    }
                }
            }

            cells = newData;
            return layerBleed;
        }
        private float DotProduct(int x, int y) {
            float tempMCount = mCount;
            float total = 0f;

            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    float? cellVal = this[x + i - xOffset, y + j - yOffset];
                    if (cellVal.HasValue) {
                        total += cellVal.Value * mask[i, j];
                    } else {
                        tempMCount -= 1f;
                    }
                }
            }
            if (tempMCount == 0) {
                return -2;
            }
            return total / tempMCount;
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