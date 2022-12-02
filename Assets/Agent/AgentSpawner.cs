using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using Assets.CASMTransmission;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Agent {
    public class AgentSpawner : MonoBehaviour {
        AgentTicker ticker;

        [SerializeField] Sprite agentSprite;
        [SerializeField] private int[] agentCount = new int[] { 100, 100 };
        [SerializeField] float spawnPoint = 50f;
        [SerializeField] float spawnRange = 10f;

        private IStateMachine[] stateMachines;
        private Sensor[][] sensors;

        private void Start() {
            ticker = GetComponent<AgentTicker>();

            stateMachines = new IStateMachine[] {
                new StateMachine<Sensor>(
                    new IState<Sensor>[] { calm, panic },
                    new IInput<Sensor>[] { close, far },
                    new Dictionary<int, (int, int)[]>() {
                            { 0, new (int, int)[] { (0, 1) } },
                            { 1, new (int, int)[] { (1, 0) } }
                    }
                ),
                new StateMachine<LayerSensor>(
                    new IState<LayerSensor>[] { hot, cold },
                    new IInput<LayerSensor>[] { isCold, isHot },
                    new Dictionary<int, (int, int)[]>() {
                            { 0, new (int, int)[] { (0, 1) } },
                            { 1, new (int, int)[] { (1, 0) } }
                    }
                )
            };
            InitAgents();
            ticker.SetAgents(sensors, stateMachines);
        }

        private void InitAgents() {
            sensors = new Sensor[][] {
                CreateSensors(agentCount[0]),
                CreateLayerSensors(agentCount[1]),
            };

            Sensor.peers = sensors.SelectMany(a => a).ToArray();
            LayerSensor.gb = GetComponent<Gearbox>();
        }

        State<Sensor> panic = new(
            (s) => {
                float dist = 0.5f;

                int peerIndex = s.GetClosestPeer();

                Vector2 dir = new Vector2();
                if (peerIndex == -1) {
                    dir = new(Random.Range(-dist, dist), Random.Range(-dist, dist));
                } else {
                    dir = s.transform.position - Sensor.peers[peerIndex].transform.position;
                    dir.Normalize();
                    dir *= dist;
                }
                s.transform.Translate(dir);

                s.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
        );
        State<Sensor> calm = new(
            (s) => {
                s.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
        );
        Input<Sensor> far = new(
            (s) => {
                return s.colliders.Count == 0;
            }
        );
        Input<Sensor> close = new(
            (s) => {
                return s.colliders.Count != 0;
            }
        );
        private Sensor[] CreateSensors(int agentCount) {
            Sensor[] sensors = new Sensor[agentCount];

            for (int i = 0; i < agentCount; i++) {
                GameObject go = new GameObject();
                go.transform.parent = transform;
                go.transform.position = new Vector3(
                    spawnPoint + Random.Range(-spawnRange, spawnRange),
                    spawnPoint + Random.Range(-spawnRange, spawnRange),
                    transform.position.z
                );
                go.AddComponent<SpriteRenderer>().sprite = agentSprite;
                Sensor sensor = go.AddComponent<Sensor>();
                go.name = sensor.id.ToString();
                sensor.SetColliderRadius(1f);

                sensors[i] = sensor;
            }
            return sensors;
        }

        State<LayerSensor> hot = new(
            (s) => {
                float dist = 0.5f;

                int peerIndex = s.GetClosestPeer();

                Vector2 dir = s.DirectionOfLowest();
                /*dir *= new Vector2(Random.Range(-dist, dist), Random.Range(-dist, dist));
                dir *= dist;*/
                s.transform.Translate(dir * dist);

                s.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
        );
        State<LayerSensor> cold = new(
            (s) => {
                s.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
        );
        Input<LayerSensor> isCold = new(
            (s) => {
                float val = s.ReadValue();
                if (val == -1) {
                    return false;
                }
                return val < 0.1f;
                //return s.colliders.Count == 0;
            }
        );
        Input<LayerSensor> isHot = new(
            (s) => {
                return s.ReadValue() > 0.2f;
                //return s.colliders.Count != 0;
            }
        );
        private Sensor[] CreateLayerSensors(int agentCount) {
            Sensor[] sensors = new Sensor[agentCount];
            for (int i = 0; i < agentCount; i++) {

                GameObject go = new GameObject();
                go.transform.parent = transform;
                go.transform.position = new Vector3(
                    spawnPoint + Random.Range(-spawnRange, spawnRange),
                    spawnPoint + Random.Range(-spawnRange, spawnRange),
                    transform.position.z
                );
                go.AddComponent<SpriteRenderer>().sprite = agentSprite;
                Sensor sensor = go.AddComponent<LayerSensor>();
                //sensor.SetTrigger(false);
                go.name = sensor.id.ToString();
                sensor.SetColliderRadius(1f);

                sensors[i] = sensor;
            }
            return sensors;
        }
    }
}