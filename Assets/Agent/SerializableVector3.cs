using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Agent {
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class SerializableVector3 {
        [JsonProperty]
        public float x;
        [JsonProperty]
        public float y;
        [JsonProperty]
        public int z;

        public SerializableVector3(float x, float y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
