using System;

namespace Assets.Environment {
    public struct Bleed {
        public int x;
        public int y;
        public float val;

        private Bleed(int x, int y, float val) {
            this.y = y;
            this.x = x;
            this.val = val;
        }
        private static Bleed Default => new Bleed(-1, -1, -1);
        public static Bleed[] GetDataHolder(int count) {
            Bleed[] data = new Bleed[count];
            Array.Fill(data, Default);
            return data;
        }
    }
}
