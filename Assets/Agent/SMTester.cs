using System.Collections.Generic;
using UnityEngine;

namespace Assets.Agent {
    public class SMTester : MonoBehaviour {
        GameObject go;
        StateMachine turnstile;
        StateMachineSensor sms;

        // Use this for initialization
        void Start() {
            Dictionary<int, (int, int)[]> trans = new Dictionary<int, (int, int)[]>();
            trans.Add(0, new (int, int)[] { (0, 1) });

            turnstile = new StateMachine(
                new State[] { locked, open },
                new Input[] { coin, push },                
                new Dictionary<int, (int, int)[]>() {
                    { 0, new (int, int)[] { (0, 1) } },
                    { 1, new (int, int)[] { (1, 0) } }
                }
            );

            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sms = go.AddComponent<StateMachineSensor>();
            InvokeRepeating(nameof(Advance), 0, 4f);
        }

        private void Advance() {
            turnstile.Advance(go);
        }

        // Update is called once per frame
        void Update() {            
            if (UnityEngine.Input.GetMouseButtonDown(0)) {
                Debug.Log("pushing");
                sms.Push();
            }
            if (UnityEngine.Input.GetMouseButtonDown(1)) {
                Debug.Log("coining");
                sms.Coin();
            }
            /*if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > w || mPos.y < 0 || mPos.y > h) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curMouse = new Vector2Int((int)mPos.x, (int)mPos.y);

                // If the mouse has moved
                if (prevMouse != curMouse) {
                    prevMouse = curMouse;
                    SetLayerValue(curMouse.x, curMouse.y);
                }
            }*/
        }        

        State locked = new State("locked", () => Debug.Log("I'm locked"));
        State open = new State("locked", () => Debug.Log("I'm open"));

        Input coin = new Input(
            "coin", 
            (go) => { 
                StateMachineSensor sms = go.GetComponent<StateMachineSensor>();                
                Debug.Log("checking coin");
                return sms.coin;
            } 
        );
        Input push = new Input(
            "push",
            (go) => {
                StateMachineSensor sms = go.GetComponent<StateMachineSensor>();
                Debug.Log("checking push");
                return sms.push;
            }
        );
    }
}