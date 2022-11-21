using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using Unity.Collections;

namespace Assets.Agent.Refactor {
    public class SMTicker : MonoBehaviour {
        private SMSensor[][] sensors;
        private SM<SMSensor>[] sms;

        [SerializeField] Sprite agentSprite;
        [SerializeField] int agentCount = 100;
        [SerializeField] float tickRate = 0.001f;
        [SerializeField] int batchCount = 10;

        State<SMSensor> panic = new(
            (s) => {
                float dist = 1f;

                int peerIndex = s.GetClosestPeer();

                Vector2 dir = new Vector2();
                if (peerIndex == -1) {
                    dir = new(Random.Range(-dist, dist), Random.Range(-dist, dist));                    
                } else {
                    dir = s.transform.position - SMSensor.peers[peerIndex].transform.position;
                    dir.Normalize();
                    dir *= dist;
                }
                s.transform.Translate(dir);

                s.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
        );
        State<SMSensor> calm = new(
            (s) => {
                s.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
        );
        Input<SMSensor> far = new(
            (s) => {
                return s.colliders.Count == 0;
            }
        );
        Input<SMSensor> close = new(
            (s) => {
                return s.colliders.Count != 0;
            }
        );


        // Use this for initialization
        void Awake() {
            SM<SMSensor> sm = new(
                new State<SMSensor>[] { panic, calm },
                new Input<SMSensor>[] {far, close},
                new Dictionary<int, (int, int)[]>() {
                        { 0, new (int, int)[] { (0, 1) } },
                        { 1, new (int, int)[] { (1, 0) } }
                }
            );
            sms = new SM<SMSensor>[batchCount];
            for (int i = 0; i < batchCount; i++) {
                sms[i] = sm;
            }

            sensors = new SMSensor[batchCount][];

            float spawnRange = 150f;
            for (int i = 0; i < sensors.Length; i++) {
                sensors[i] = new SMSensor[agentCount];
                for (int j = 0; j < agentCount; j++) {
                    
                    GameObject go = new GameObject();
                    go.hideFlags = HideFlags.HideInHierarchy;
                    go.transform.position = new Vector2(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange));
                    go.AddComponent<SpriteRenderer>().sprite = agentSprite;
                    SMSensor sensor = go.AddComponent<SMSensor>();
                    sensor.SetColliderRadius(1f);
                    sensors[i][j] = sensor;
                }
            }
            
            SMSensor.SetPeers();
            if (sensors.Length != sms.Length) {
                throw new Exception("sensors types don't match state machines count");
            }

            //InvokeRepeating(nameof(AdvanceSensorsParallel), 0, tickRate);
        }

        private void Update() {
            //AdvanceSensorsParallel();
            AdvanceSensors();
        }

        private void AdvanceSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                SM<SMSensor> curSM = sms[i];
                for (int j = 0; j < sensors[i].Length; j++) {
                    curSM.AdvanceSensor(sensors[i][j]);
                }
            }
        }

        private void AdvanceSensorsParallel() {
            NativeArray<bar> foo;
            List<SensorBatch<SMSensor>> batches = new List<SensorBatch<SMSensor>>();
            for (int i = 0; i < sensors.Length; i++) {
                batches.Add(new SensorBatch<SMSensor>(ref sms[i], ref sensors[i]));
            }
            Parallel.ForEach(batches, (batch) => { batch.Execute(); });
        }
    }

    public struct bar{
        private void Test() {

        }
    }
}