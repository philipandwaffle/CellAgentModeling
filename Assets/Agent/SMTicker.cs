using Assets.Agent.Sensors;
using Assets.CASMTransmission;
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

namespace Assets.Agent {
    public class SMTicker : MonoBehaviour {
        private SMSensor[][] sensors;
        //private (IStateMachine<SMSensor>, IStateMachine<LayerSensor>, SMSensor[])[] horror;
        private IStateMachine<LayerSensor>[] sms;

        [SerializeField] Sprite agentSprite;
        [SerializeField] int agentCount = 100;
        [SerializeField] float spawnPoint = 50f;
        [SerializeField] float spawnRange = 10f;
        [SerializeField] int batchCount = 10;
        
        /*        State<SMSensor> panic = new(
                    (s) => {
                        float dist = 0.5f;

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
                );*/

        State<LayerSensor> panic = new(
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
        State<LayerSensor> calm = new(
            (s) => {
                s.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
        );
        Input<LayerSensor> cold = new(
            (s) => {
                float val = s.ReadValue();
                if (val == -1) {
                    return false;
                }
                return val < 0.1f;
                //return s.colliders.Count == 0;
            }
        );
        Input<LayerSensor> hot = new(
            (s) => {
                return s.ReadValue() > 0.2f;
                //return s.colliders.Count != 0;
            }
        );


        // Use this for initialization
        void Start() {            
            IStateMachine<LayerSensor> sm = new SM<LayerSensor>(
                new IState<LayerSensor>[] { panic, calm },
                new IInput<LayerSensor>[] { cold, hot },
                new Dictionary<int, (int, int)[]>() {
                        { 0, new (int, int)[] { (0, 1) } },
                        { 1, new (int, int)[] { (1, 0) } }
                }
            );
            //IStateMachine<SMSensor> foo = (IStateMachine<SMSensor>)sm;
            sms = new IStateMachine<LayerSensor>[batchCount];
            for (int i = 0; i < batchCount; i++) {
                sms[i] = sm;
            }

            sensors = new SMSensor[batchCount][];

            for (int i = 0; i < sensors.Length; i++) {
                sensors[i] = new SMSensor[agentCount];
                for (int j = 0; j < agentCount; j++) {
                    GameObject go = new GameObject();
                    go.transform.parent = transform;
                    go.transform.position = new Vector3(
                        spawnPoint + Random.Range(-spawnRange, spawnRange),
                        spawnPoint + Random.Range(-spawnRange, spawnRange), 
                        transform.position.z
                    );
                    go.AddComponent<SpriteRenderer>().sprite = agentSprite;
                    SMSensor sensor = go.AddComponent<LayerSensor>();
                    sensor.SetTrigger(false);
                    go.name = sensor.id.ToString();
                    sensor.SetColliderRadius(1f);

                    sensors[i][j] = sensor;
                }
            }
            
            SMSensor.SetPeers();
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
                IStateMachine<LayerSensor> curSM = sms[i];
                for (int j = 0; j < sensors[i].Length; j++) {
                    curSM.AdvanceSensor((LayerSensor)sensors[i][j]);
                }
            }
        }
    }
}