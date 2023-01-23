using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Environment {
    public class TileLayerTicker : MonoBehaviour {
        private static Tile wall, open, stair;
        private static bool loadedTiles = false;

        private CellLayer layer;

        private void Awake() {
            if (!loadedTiles) {
                wall = Resources.Load<Tile>("Tiles/WallTile");
                open = Resources.Load<Tile>("Tiles/OpenTile");
                stair = Resources.Load<Tile>("Tiles/StairTile");

                loadedTiles = true;
            }
        }


    }
}
