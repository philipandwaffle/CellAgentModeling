using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Environment {
    /// <summary>
    /// Implement this to allow the class to be used as a nav graph for a layer
    /// </summary>
    public interface INavGraph {
        /// <summary>
        /// Updates the edges' weight of the graph based on the layer it belongs to
        /// </summary>
        /// <param name="layer"></param>
        public void UpdateEdges(ref float[,] layer);
        /// <summary>
        /// Update paths that the agents will use
        /// </summary>
        public void UpdatePaths();

        /// <summary>
        /// Gets the coordinate of each node on a layer
        /// </summary>
        /// <returns>Array of node coords</returns>
        public Vector2Int[] GetNodeCoords();
        /// <summary>
        /// Returns the paths used to reach each node
        /// </summary>
        /// <returns>An array of paths with each path being an array of nodes making up that path</returns>
        public int[][] Getpaths();
    }
}
