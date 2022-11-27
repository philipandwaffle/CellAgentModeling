using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Unity.Mathematics;

namespace Assets.Environment.Refactor {
    public class Layer<T> {
        public T[,] data;
        public T[,] mask;
        
        private Func<T, T, T> agg;
        private Func<T, T, T> sum;
        private Func<T, Color> display;
        private Func<T, T> constrain;

        public byte[] aggBytes, sumBytes, displayBytes, constrainBytes;

        public int w, h;
        public int mW, mH;

        private int xOffset;
        private int yOffset;

        public T this[int x, int y] {
            get {
                x = Math.Abs(x);
                y = Math.Abs(y);

                x = x >= w ? x % w : x;
                y = y >= w ? y % h : y;

                return data[x,y];
            }
            set {
                x = Math.Abs(x);
                y = Math.Abs(y);

                x = x >= w ? x % w : x;
                y = y >= w ? y % h : y;

                data[x, y] = value;
            }
        }

        public Layer(T[,] data, T[,] mask, Func<T, T, T> aggFunc, Func<T, T, T> sumFunc, Func<T, Color> display, Func<T, T> constrain) {
            // Setup dimensions
            w = data.GetLength(0);
            h = data.GetLength(1);

            mW = mask.GetLength(0);
            mH = mask.GetLength(1);

            xOffset = mW / 2;
            yOffset = mH / 2;

            this.data = data;
            this.mask = mask;
            this.agg = aggFunc;
            this.sum = sumFunc;
            this.display = display;
            this.constrain = constrain;
        }

        public static Layer<T> LoadLayer(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Layer<T>>(json);
            }
        }

        public Color GetDisplayData(int x, int y) {
            return display(data[x, y]);
        }
        public void InsertValue(int x, int y, T value) {
            data[x, y] = constrain(value);
        }

        public void Advance() {
            T[,] newData = new T[w, h];

            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    newData[i, j] = DotProduct(i, j);
                }
            }

            data = newData;
        }
        private T DotProduct(int x, int y) {
            T res = default(T);

            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    T agg = this.agg(this[x - xOffset, y - yOffset], mask[i, j]);
                    res = sum(res, agg);
                }
            }
            return res;
        }

        public void Save(string path, Formatting format = Formatting.None) {
            string json = JsonConvert.SerializeObject(this, format);
            
            using (StreamWriter sr = new StreamWriter(path)) { 
                sr.WriteLine(json);
            }
        }

        [OnSerializing]
        internal void Serializing(StreamingContext context) {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()){
                formatter.Serialize(ms, agg);
                aggBytes = ms.ToArray();
            }
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, sum);
                sumBytes = ms.ToArray();
            }
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, display);
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
            
            MemoryStream ms = new MemoryStream(sumBytes);
            sum = (Func<T, T, T>)formatter.Deserialize(ms);

            ms = new MemoryStream(aggBytes);
            agg = (Func<T, T, T>)formatter.Deserialize(ms);

            ms = new MemoryStream(displayBytes);
            display = (Func<T, Color>)formatter.Deserialize(ms);

            ms = new MemoryStream(constrainBytes);
            constrain = (Func<T, T>)formatter.Deserialize(ms);

            ms.Dispose();

            xOffset = mW / 2;
            yOffset = mH / 2;
        }
    }
}