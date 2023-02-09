using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Tilemaps;

namespace Assets.Environment.TileMap {
    public class TileLayer : MonoBehaviour{
        Tilemap tm;
        Layer l;
        private void Awake() {
            tm = GetComponent<Tilemap>();
        }
        private void Start() {
            SetLayer(Layer.LoadLayer("C:\\Users\\philipc\\Documents\\Personal\\UnityStuff\\CellAgentModeling\\Assets\\Layers\\demo_1\\4.layer"));
        }

        public void SetLayer(Layer l) {
            this.l = l;

            for (int x = 0; x < l.w; x++) {
                for (int y = 0; y < l.h; y++) {
                    if (x > 20 && x < 30 && y > 20 && y < 30) {
                        l[x, y] = 0.9f;
                    }
                }
            }

            for (int x = 0; x < l.w; x++) {
                for (int y = 0; y < l.h; y++) {
                    float val = l[x, y];

                    Tile t = new Tile();
                    if (val == -2) {
                        t = TileLayerTicker.stair;
                    } else if (val == -1) {
                        t = TileLayerTicker.wall;
                    } else if (val >= 0f && val <= 1f) {
                        t = TileLayerTicker.open;
                        t.color = Color.HSVToRGB((1 - val / 4f) - 0.75f, 0.7f, 0.5f);
                    }
                    tm.SetTile(new Vector3Int(x, y, 0), t);
                }
            }
        }

        private void Update() {
            l.Advance();

            for (int x = 0; x < l.w; x++) {
                for (int y = 0; y < l.h; y++) {
                    float val = l[x, y];
                    if (val >= 0f && val <= 1f) {
                        Vector3Int pos = new Vector3Int(x, y, 0);
                        Tile t = TileLayerTicker.open;
                        Color c = Color.HSVToRGB((1 - val / 4f) - 0.75f, 0.7f, 0.5f);
                        //t.color = c;
                        Color cOld = tm.GetColor(pos);
                        if (cOld.g != c.g) {
                            tm.SetColor(new Vector3Int(x, y, 0), c);
                            Debug.Log(pos + ": " + cOld.g + ", " + c.g);
                            tm.RefreshTile(pos);
                        }
                        //tm.SetTile(new Vector3Int(x, y, 0), t);
                    }
                }
            }

            //tm.RefreshAllTiles();
        }
    }
}
