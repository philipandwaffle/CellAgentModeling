using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Environment {
    public class Cell {
        public enum CellType { Open, Wall, Stairs }
        
        public CellType type { private set; get; }
        public float val { private set; get; }

        public Cell(CellType type = CellType.Open, float val = 0) {
            this.type = type;
            this.val = val;
        }

        public Color GetDisplay() {
            switch (type) {
                case CellType.Open:
                return Color.HSVToRGB((1 - val / 4f) - 0.75f, 0.7f, 0.5f);
                case CellType.Wall:
                return Color.black;
                case CellType.Stairs:
                return Color.green;
                default:
                return Color.magenta;
            }
        }

        public void SetVal(float val) {
            this.val = val;
        }
        public void SetType(CellType type) {
            this.type = type;
        }

        public static Cell[,] ValuesToCells(float[,] data) {
            int w = data.GetLength(0);
            int h = data.GetLength(1);
            Cell[,] cells = new Cell[w,h];

            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    float val = data[x, y];
                    CellType type = CellType.Open;

                    if (val == -1){
                        type = CellType.Wall;
                    }else if(val == -2) {
                        type = CellType.Stairs;
                    }

                    cells[w, h] = new Cell(type, val);
                }
            }

            return cells;
        }
    }
}
