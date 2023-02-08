using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Environment {
    public enum CellType { Open, Wall, Stairs }

    [JsonObject(MemberSerialization.OptIn)]
    public class Cell {
        public CellType type;
        public float? val;

        [JsonProperty]
        private float v;

        private SpriteRenderer sr;
        private BoxCollider2D bc;

        [JsonConstructor]
        public Cell(float val) {
            SetVal(val);
        }

        public float? GetValue() {
            switch (type) {
                case CellType.Wall:
                    return null;
                case CellType.Stairs:
                    return null;
                case CellType.Open:
                    return val;
                default:
                    throw new NotImplementedException();
            }
        }
        public void SetVal(float val) {
            switch (val) {
                case -1:
                    type = CellType.Wall;
                    this.val = null;
                    sr.color = Color.black;
                    break;
                case -2:
                    type = CellType.Stairs;
                    this.val = null; 
                    sr.color = Color.green;
                    break;
                default:
                    type = CellType.Open;
                    this.val = Mathf.Clamp(val, 0f, 1f);
                    sr.color = Color.HSVToRGB((1 - val / 4f) - 0.75f, 0.7f, 0.5f);
                    break;
            }
        }
        public bool HasValue() {
            return val is not null;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context) {
            switch (type) {
                case CellType.Wall:
                    v = - 1f;
                    break;
                case CellType.Stairs:
                    v = -2;
                    break;
                case CellType.Open:
                    v = val.Value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
