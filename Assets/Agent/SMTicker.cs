using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using Assets.CASMTransmission;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Agent {
    public class SMTicker : MonoBehaviour {
        private Sensor[][] sensors;
        //private (IStateMachine<SMSensor>, IStateMachine<LayerSensor>, SMSensor[])[] horror;
        private IStateMachine[] sms;

        [SerializeField] Sprite agentSprite;
        [SerializeField] int agentCount = 100;
        [SerializeField] float spawnPoint = 50f;
        [SerializeField] float spawnRange = 10f;
        [SerializeField] int batchCount = 10;

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


        // Use this for initialization
        void Start() {
            IStateMachine<Sensor> PanicClose = new StateMachine<Sensor>(
                new IState<Sensor>[] { calm, panic },
                new IInput<Sensor>[] { close, far },
                new Dictionary<int, (int, int)[]>() {
                        { 0, new (int, int)[] { (0, 1) } },
                        { 1, new (int, int)[] { (1, 0) } }
                }
            );
            IStateMachine<LayerSensor> sm = new StateMachine<LayerSensor>(
                new IState<LayerSensor>[] { hot, cold },
                new IInput<LayerSensor>[] { isCold, isHot },
                new Dictionary<int, (int, int)[]>() {
                        { 0, new (int, int)[] { (0, 1) } },
                        { 1, new (int, int)[] { (1, 0) } }
                }
            );
            sms = new IStateMachine[2];
            
            sms[1] = PanicClose;
            sms[0] = sm;

            sensors = new Sensor[2][];

            sensors[0] = new Sensor[agentCount];
            for (int j = 0; j < agentCount; j++) {
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

                sensors[0][j] = sensor;
            }

            sensors[1] = new Sensor[agentCount];
            for (int j = 0; j < agentCount; j++) {
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

                sensors[1][j] = sensor;
            }
            
            Sensor.SetPeers();
            LayerSensor.gb = GetComponent<Gearbox>();

            if (sensors.Length != sms.Length) {
                throw new Exception("sensors types don't match state machines count");
            }
        }

        private void Update() {
            AdvanceSensors();
        }

        private void AdvanceSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                switch (sms[i]) {
                    case IStateMachine<LayerSensor> sm:
                        for (int j = 0; j < sensors[i].Length; j++) {
                            sm.AdvanceSensor((LayerSensor)sensors[i][j]);
                        }
                    break;
                    case IStateMachine<Sensor> sm:
                        for (int j = 0; j < sensors[i].Length; j++) {
                            sm.AdvanceSensor(sensors[i][j]);
                        }
                    break;
                }
            }
        }
    }
}