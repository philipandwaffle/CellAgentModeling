using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

namespace Assets.Agent {
    public class SMTester : MonoBehaviour {        
        [SerializeField] Sprite displaySprite;
        [SerializeField] int count = 100;
        [SerializeField] float tickRate = 0.001f;

        private StateMachine[] runners;
        private GameObject[] runnerSensors;

        // Use this for initialization
        void Start() {
            runners = new StateMachine[count];
            runnerSensors = new GameObject[count];

            for (int i = 0; i < runners.Length; i++) {
                StateMachine runner = new StateMachine(
                    new State[] { panic, calm },
                    new Input[] { far, close },
                    new Dictionary<int, (int, int)[]>() {
                        { 0, new (int, int)[] { (0, 1) } },
                        { 1, new (int, int)[] { (1, 0) } }
                    }
                );
                GameObject runnerSensor = new GameObject();
                runnerSensor.AddComponent<StateMachineSensor>();
                runnerSensor.AddComponent<SpriteRenderer>().sprite = displaySprite;

                runners[i] = runner;
                runnerSensors[i] = runnerSensor;
            }
            StateMachineSensor.sensors = FindObjectsOfType<StateMachineSensor>();

            InvokeRepeating(nameof(Advance), 0, tickRate);
        }

        private void Advance() {
            //turnstile.Advance(agent);

            for (int i = 0; i < runners.Length; i++) {
                runners[i].Advance(runnerSensors[i]);
            }
        }

        State panic = new State(
            "panic", 
            (agent) => {
                agent.GetComponent<SpriteRenderer>().color = Color.red;
                float range = 0.5f;
                Vector2 translation = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
                agent.transform.Translate(translation);
            }
        );
        State calm = new State(
            "calm", 
            (agent) => {
                agent.GetComponent<SpriteRenderer>().color = Color.green;
                /*float range = 0.1f;
                Vector2 translation = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
                agent.transform.Translate(translation);*/
            }
        );

        Input close = new Input(
            "close",
            (agent) => {
                StateMachineSensor[] agents = StateMachineSensor.sensors;
                for (int i = 0; i < agents.Length; i++) {
                    if (agent.GetComponent<StateMachineSensor>().id == agents[i].id) {
                        continue;
                    }
                    if (Vector2.Distance(agent.transform.position, agents[i].transform.position) < 2f) {
                        return true;
                    }
                }
                return false;
            }
        );
        Input far = new Input(
            "far",
            (agent) => {
                StateMachineSensor[] agents = StateMachineSensor.sensors;
                for (int i = 0; i < agents.Length; i++) {
                    if (agent.GetComponent<StateMachineSensor>().id == agents[i].id) {
                        continue;
                    }
                    if (Vector2.Distance(agent.transform.position, agents[i].transform.position) <= 2f) {
                        return false;
                    }
                }
                return true;
            }
        );
    }
}