using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Assets.Environment.Refactor {
    [Serializable]
    public class Layer {
        public float[,] data;
        public float[,] mask;

        private Func<float, Color> display;
        private Func<float, float> constrain;

        public byte[] aggBytes, sumBytes, displayBytes, constrainBytes;

        public int w, h;
        public int mW, mH;
        private float mCount;

        private int xOffset;
        private int yOffset;

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

//        [JsonConstructor]
        public Layer(float[,] data, float[,] mask, Func<float, float, float> agg, Func<float, float, float> sum, Func<float, Color> display, Func<float, float> constrain) {
            InitVariables(mask, agg, sum, display, constrain);

            w = data.GetLength(0);
            h = data.GetLength(1);
            this.data = data;
        }
        public Layer(int w, int h, float[,] mask, Func<float, float, float> agg, Func<float, float, float> sum, Func<float, Color> display, Func<float, float> constrain) {            
            InitVariables(mask, agg, sum, display, constrain);

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

        private void InitVariables(float[,] mask, Func<float, float, float> agg, Func<float, float, float> sum, Func<float, Color> display, Func<float, float> constrain) {
            mW = mask.GetLength(0);
            mH = mask.GetLength(1);
            mCount = mW * mH;

            xOffset = mW / 2;
            yOffset = mH / 2;

            this.mask = mask;
            this.display = display;
            this.constrain = constrain;
        }

        public static Layer LoadLayer(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Layer>(json);
            }
        }

        public void Fill(float value) {
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    data[i, j] = value;
                }
            }
        }        

        public Color GetDisplayData(int x, int y) {
            return display(data[x, y]);
        }
        public void InsertValue(int x, int y, float value) {
            data[x, y] = constrain(value);
        }

        public void Advance() {
            float[,] newData = new float[w, h];

            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    newData[i, j] = DotProduct(i, j);
                }
            }

            data = newData;
        }
        private float DotProduct(int x, int y) {
            if (this[x,y].Equals(-1)) {
                return -1;
            }

            float tempMCount = mCount;
            float total = 0f;

            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    float curVal = this[x + i - xOffset, y + j - yOffset];
                    float maskVal = mask[i, j];

                    if (curVal == -1) {
                        tempMCount -= 1f;
                    } else {
                        total += curVal * maskVal;
                    }
                }
            }

            return total / tempMCount;
        }

        public void Save(string path, Formatting format = Formatting.None) {
            string json = JsonConvert.SerializeObject(this, format);
            
            using (StreamWriter sr = new StreamWriter(path,false)) { 
                sr.WriteLine(json);
            }
        }

        [OnSerializing]
        internal void Serializing(StreamingContext context) {
            BinaryFormatter formatter = new BinaryFormatter();
            /*using (MemoryStream ms = new MemoryStream()){
                formatter.Serialize(ms, agg);
                aggBytes = ms.ToArray();
            }
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, sum);
                sumBytes = ms.ToArray();
            }*/

            using (FileStream stream = new(Application.dataPath + "/Layers/deleteme.txt", FileMode.Create, FileAccess.Write, FileShare.None))
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(stream, display);
                displayBytes = ms.ToArray();
            }
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, constrain);
                constrainBytes = ms.ToArray();
            }
        }
        [OnDeserialized]
        internal void Deserialized(StreamingContext context) {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms;
            //ms = new MemoryStream(sumBytes);
            /*sum = (Func<float, float, float>)formatter.Deserialize(ms);

            ms = new MemoryStream(aggBytes);
            agg = (Func<float, float, float>)formatter.Deserialize(ms);*/

            ms = new MemoryStream(displayBytes);
            display = (Func<float, Color>)formatter.Deserialize(ms);

            ms = new MemoryStream(constrainBytes);
            constrain = (Func<float, float>)formatter.Deserialize(ms);

            ms.Dispose();

            xOffset = mW / 2;
            yOffset = mH / 2;
        }
    }
}