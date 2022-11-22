using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;

namespace Assets.Environment.Refactor {

    [Serializable]
    public class Layer<T> {
        public T[,] data;
        public T[,] mask;
        public Func<T, T, T> aggFunc;

        public int w, h;
        public int mW, mH;
        public int tW, tH;

        public Layer(T[,] data, T[,] mask, Func<T, T, T> aggFunc) {
            // Setup dimensions
            h = data.GetLength(0);
            w = data.GetLength(1);

            mH = mask.GetLength(0);
            mW = mask.GetLength(1);

            tW = w + 2 * (mW / 2);
            tH = h + 2 * (mH / 2);

            this.data = data;
            this.mask = mask;
            this.aggFunc = aggFunc;
        }

        public static This LoadLayer(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<This>(json);
            }
        }

        private void UpdatePadding() {

        }

        public void Save(string path) {
            string json = JsonConvert.SerializeObject(this, Formatting.None);
            Debug.Log(json);
            using (StreamWriter sr = new StreamWriter(path)) {
                sr.WriteLine(json);
            }
        }
    }
}