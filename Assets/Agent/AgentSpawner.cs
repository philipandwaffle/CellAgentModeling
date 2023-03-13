using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using Assets.CASMTransmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
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
            InitAgents();
            ticker.SetAgents(sensors, stateMachines);
        }

        private void InitAgents() {
            stateMachines = new IStateMachine[] {
                //GetHCPC()
                GetNavAgent()
            };

            sensors = new Sensor[][] {
                CreateNavSensors(agentCount[0]),
            };

            Sensor.peers = sensors.SelectMany(a => a).ToArray();
            //LayerSensor.gb = GetComponent<Gearbox>();
            NavLayerSensor.gb = GetComponent<Gearbox>();
        }

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
                MultiLayerSensor sensor = go.AddComponent<MultiLayerSensor>();
                sensor.MoveLayer(4);
                go.name = sensor.id.ToString();

                sensor.SetConRadius(1f);
                sensor.SetColRadius(0.5f);

                sensors[i] = sensor;
            }
            return sensors;
        }

        private Sensor[] CreateNavSensors(int agentCount) {
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
                NavLayerSensor sensor = go.AddComponent<NavLayerSensor>();
                //sensor.MoveLayer(4);
                go.name = sensor.id.ToString();

                sensor.SetConRadius(1f);
                sensor.SetColRadius(0.5f);

                sensors[i] = sensor;
            }
            return sensors;
        }

        // Generates and gets the hot cold panic calm state machine
        private IStateMachine<MultiLayerSensor> GetHCPC() {
            float dirModifier = 10f;
            MultiLayerSensor.maxZ = 5;
            State<MultiLayerSensor> hotPanic = new(
                (s) => {
                    int peerIndex = s.GetClosestPeer();

                    Vector2 dir = s.DirectionOfLowest();
                    dir *= dirModifier * 2;
                    s.ApplyForce(dir);

                    s.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                }
            );
            State<MultiLayerSensor> hotCalm = new(
                (s) => {
                    int peerIndex = s.GetClosestPeer();

                    Vector2 dir = s.DirectionOfLowest();
                    dir *= dirModifier;
                    s.ApplyForce(dir);

                    s.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
                }
            );
            State<MultiLayerSensor> coldPanic = new(
                (s) => {
                    int peerIndex = s.GetClosestPeer();

                    Vector2 dir = new Vector2();
                    if (peerIndex == -1) {
                        dir = new(Random.Range(-dirModifier, dirModifier), Random.Range(-dirModifier, dirModifier));
                    } else {
                        dir = s.transform.position - Sensor.peers[peerIndex].transform.position;
                        dir.Normalize();
                        dir *= dirModifier * 2;
                    }
                    s.ApplyForce(dir);

                    s.gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
                }
            );
            State<MultiLayerSensor> coldCalm = new(
                (s) => {
                    s.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
                }
            );
            State<MultiLayerSensor> TakeStairs = new(
                (s) => {
                    s.MoveLayer(-1);
                }
            );

            Input<MultiLayerSensor> isCold = new(
                (s) => {
                    return s.ReadValue() < 0.3f;
                }
            );
            Input<MultiLayerSensor> isHot = new(
                (s) => {
                    return s.ReadValue() >= 0.3f;
                }
            );
            Input<MultiLayerSensor> isFar = new(
                (s) => {
                    return s.contacts.Count == 0;
                }
            );
            Input<MultiLayerSensor> isNear = new(
                (s) => {
                    return s.contacts.Count != 0;
                }
            );
            Input<MultiLayerSensor> isOnSteps = new(
                (s) => {
                    return s.ReadValue() == -2;
                }
            );


            return new StateMachine<MultiLayerSensor>(
                new IState<MultiLayerSensor>[] { hotPanic, hotCalm, coldPanic, coldCalm, TakeStairs },
                new IInput<MultiLayerSensor>[] {isCold, isHot, isFar, isNear, isOnSteps },
                new Dictionary<int, (int, int)[]>() {
                    { 0, new (int, int)[] { (2, 1), (0, 2), (4, 4) } },
                    { 1, new (int, int)[] { (0, 3), (3, 0), (4, 4) } },
                    { 2, new (int, int)[] { (1, 0), (2, 3), (4, 4) } },
                    { 3, new (int, int)[] { (3, 2), (1, 1), (4, 4) } },
                    { 4, new (int, int)[] { (0, 3), (1, 1), (2, 3), (3, 2) } },
                }
            );
        }

        private IStateMachine<NavLayerSensor> GetNavAgent() {
            float dirModifier = 10f;
            State<NavLayerSensor> findPath = new(
                (s) => {
                    Debug.Log("find path");
                    s.UpdatePath();
                }
            );
            State<NavLayerSensor> escape = new(
                (s) => {
                    //Debug.Log("moving");
                    s.ApplyForce(dirModifier * s.GetDir());
                }
            );

            Input<NavLayerSensor> finishSetup = new((s) => { return s.HasPath(); });

            return new StateMachine<NavLayerSensor>(
                new IState<NavLayerSensor>[] { findPath, escape },
                new IInput<NavLayerSensor>[] { finishSetup },
                new Dictionary<int, (int, int)[]>() {
                    { 0, new (int, int)[] { (0, 1) } },
                }
            );
        }
    }
}