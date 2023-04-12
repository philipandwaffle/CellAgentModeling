using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Environment {
    public interface INavGraph {
        public void UpdateEdges(ref float[,] layer);
        public void UpdatePaths();

        public Vector2Int[] GetNodeCoords();
        public int[][] Getpaths();
    }
}
