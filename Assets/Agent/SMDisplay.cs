using System.Collections;
using System.Data;
using UnityEngine;

namespace Assets.Agent {
    public class SMDisplay : MonoBehaviour {
        public Sprite displaySprite;
        private GameObject display;
        public StateMachine sm;

        // Use this for initialization
        void Start() {
            display = new GameObject();
            SpriteRenderer sr = display.AddComponent<SpriteRenderer>();
            sr.sprite = displaySprite;
        }

        // Update is called once per frame
        void Update() {
            if (sm.curState == 0) {
                display.GetComponent<SpriteRenderer>().color = Color.red;
            }else if (sm.curState == 1) {
                display.GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }
}