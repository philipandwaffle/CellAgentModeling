using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Assets.Agent.Refactor {
    public class SMTicker : MonoBehaviour {
        private SMSensor[][] sensors;
        private SM<SMSensor>[] sms;

        [SerializeField] Sprite agentSprite;
        [SerializeField] int agentCount = 100;
        [SerializeField] float tickRate = 0.001f;
        //[SerializeField] int batchSize = 50;

        State<SMSensor> panic = new(
            (s) => {
                float dist = 0.5f;

                /*int peerIndex = s.GetClosestPeer();
                if (peerIndex != -1) {
                    Vector3 dir = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
                } else {

                }


                Vector3 closest = SMSensor.peers[s.GetClosestPeer()].transform.position;
                if (closest.magnitude == 0f) {
                    closest = new(Random.Range(-1, 1), Random.Range(-1, 1));
                }
                Vector3 dir = s.transform.position - closest;
                dir = dir.normalized;
                s.transform.Translate(dir * dist);*/
                s.transform.Translate(new(Random.Range(-dist, dist), Random.Range(-dist, dist)));
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
                for (int i = 0; i < SMSensor.peers.Length; i++) {
                    SMSensor peer = SMSensor.peers[i];
                    if (s.id == peer.id) {
                        continue;
                    }
                    if (Vector3.Distance(s.transform.position, peer.transform.position) <= 2) {
                        return false;
                    }
                }
                return true;
            }
        );
        Input<SMSensor> close = new(
            (s) => {
                for (int i = 0; i < SMSensor.peers.Length; i++) {
                    SMSensor peer = SMSensor.peers[i];
                    if (s.id == peer.id) {
                        continue;
                    }
                    if (Vector3.Distance(s.transform.position, peer.transform.position) < 2) {
                        return true;
                    }
                }
                return false;
            }
        );


        // Use this for initialization
        void Start() {
            SM<SMSensor> sm = new(
                new State<SMSensor>[] { panic, calm },
                new Input<SMSensor>[] {far, close},
                new Dictionary<int, (int, int)[]>() {
                        { 0, new (int, int)[] { (0, 1) } },
                        { 1, new (int, int)[] { (1, 0) } }
                }
            );

            //sms = new SM<SMSensor>[] { sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm, sm };
            sms = new SM<SMSensor>[] { sm };
            sensors = new SMSensor[sms.Length][];            

            for (int i = 0; i < sensors.Length; i++) {
                sensors[i] = new SMSensor[agentCount];
                for (int j = 0; j < agentCount; j++) {
                    GameObject go = new GameObject();
                    go.AddComponent<SpriteRenderer>().sprite = agentSprite;
                    sensors[i][j] = go.AddComponent<SMSensor>();
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
            List<SensorBatch<SMSensor>> batches = new List<SensorBatch<SMSensor>>();
            for (int i = 0; i < sensors.Length; i++) {
                batches.Add(new SensorBatch<SMSensor>(ref sms[i], ref sensors[i]));
                /*SM<SMSensor> curSM = sms[i];

                int start = 0;
                do {
                    int curBatchSize = batchSize;
                    curBatchSize = curBatchSize + start > sensors[i].Length ? sensors[i].Length - start : curBatchSize;

                    int end = start + curBatchSize;


                    
                    start += batchSize;
                } while (start < sensors[i].Length);*/
            }
            
            for (int i = 0; i < batches.Count; i++) {
                batches[i].Execute();
            }
        }

        private void AdvanceSensorBatch(int smIndex, int start, int end) {
            SM<SMSensor> curSM = sms[smIndex];
            for (int i = start; i < end; i++) {
                curSM.AdvanceSensor(sensors[smIndex][i]);
            }
        }
    }
}