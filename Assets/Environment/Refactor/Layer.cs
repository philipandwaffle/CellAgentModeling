using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Assets.Environment.Refactor {
    public class Layer<T> {
        public T[,] data;
        public T[,] mask;
        
        public T stopVal;
        //public T maxVal;

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
        public Layer(T[,] data, T[,] mask, T stopVal, Func<T, T, T> agg, Func<T, T, T> sum, Func<T, Color> display, Func<T, T> constrain) {
            // Setup dimensions
            w = data.GetLength(0);
            h = data.GetLength(1);

            mW = mask.GetLength(0);
            mH = mask.GetLength(1);

            this.stopVal = stopVal;

            /*maxVal = default(T);
            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    maxVal = sum(maxVal, mask[i, j]);
                }
            }*/

            xOffset = mW / 2;
            yOffset = mH / 2;

            this.data = data;
            this.mask = mask;
            this.agg = agg;
            this.sum = sum;
            this.display = display;
            this.constrain = constrain;
        }

        public Layer(int w, int h, T[,] mask, T stopVal, Func<T, T, T> agg, Func<T, T, T> sum, Func<T, Color> display, Func<T, T> constrain) {
            this.w = w;
            this.h = h;
            data = new T[w, h];
            Fill(default(T));

            mW = mask.GetLength(0);
            mH = mask.GetLength(1);

            this.stopVal = stopVal;
            /*maxVal = default(T);
            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    maxVal = sum(maxVal, mask[i, j]);
                }
            }*/

            xOffset = mW / 2;
            yOffset = mH / 2;
            
            this.mask = mask;
            this.agg = agg;
            this.sum = sum;
            this.display = display;
            this.constrain = constrain;
        }

        public static Layer<T> LoadLayer(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Layer<T>>(json);
            }
        }

        public void Fill(T value) {
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    data[i, j] = value;
                }
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
            if (this[x,y].Equals(stopVal)) {
                return stopVal;
            }

            T res = default(T);
            T stopValSum = default(T);

            for (int i = 0; i < mW; i++) {
                for (int j = 0; j < mH; j++) {
                    T curVal = this[x + i - xOffset, y + j - yOffset];
                    T maskVal = mask[i, j];
                    T agg = this.agg(curVal, maskVal);

                    if (curVal.Equals(stopVal)) {
                        stopValSum = sum(stopValSum, agg);
                    } else {                        
                        res = sum(res, agg);
                    }
                }
            }

            //return sum(agg(stopValSum, agg(res, maxVal)), res);
            return res;
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