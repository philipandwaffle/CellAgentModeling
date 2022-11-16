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
        }        

        State locked = new State("locked", () => Debug.Log("I'm locked"));
        State open = new State("open", () => Debug.Log("I'm open"));

        Input coin = new Input(
            "coin", 
            (go) => {
                StateMachineSensor sms = go.GetComponent<StateMachineSensor>();                
                return sms.coin;
            } 
        );
        Input push = new Input(
            "push",
            (go) => {
                StateMachineSensor sms = go.GetComponent<StateMachineSensor>();                
                return sms.push;
            }
        );
    }
}