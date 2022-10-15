using System;

namespace Assets.Environment {
    public static class HoodFunctions {
        public static Func<float[,], float> BoundedAvgSpread = delegate (float[,] hood) {
            int w = hood.GetLength(0);
            int h = hood.GetLength(1);

            float avg = w * h;
            float total = 0;

            if (hood[w / 2, h / 2] < 0) {
                return -1;
            }

            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    if (hood[x, y] < 0) {
                        avg -= 1f;
                    } else {
                        total += hood[x, y];
                    }
                }
            }

            return total / avg;
        };
    }
}