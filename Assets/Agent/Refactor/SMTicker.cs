using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public class SMTicker : MonoBehaviour {
        private SMSensor[][] sensors;
        private SM<SMSensor>[] sms;

        [SerializeField] Sprite agentSprite;
        [SerializeField] int agentCount = 100;
        [SerializeField] float tickRate = 0.01f;

        State<SMSensor> panic = new(
            (s) => {
                float dist = 1f;        
                Vector3 closest = s.GetClosestPos();
                Vector3 dir = s.transform.position + closest;
                dir = dir.normalized;
                s.transform.Translate(dir * dist);
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
                if (Vector3.Distance(s.GetClosestPos(), s.gameObject.transform.position) >= 2) {
                    return false;
                }
                return true;
            }
        );
        Input<SMSensor> close = new(
            (s) => {
                if (Vector3.Distance(s.GetClosestPos(), s.gameObject.transform.position) < 2) {
                    return true;
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

            sms = new SM<SMSensor>[] { sm };
            sensors = new SMSensor[1][];
            sensors[0] = new SMSensor[agentCount];

            for (int i = 0; i < agentCount; i++) {
                GameObject go = new GameObject();
                go.AddComponent<SpriteRenderer>().sprite = agentSprite;
                sensors[0][i] = go.AddComponent<SMSensor>();
            }

            SMSensor.SetPeers();
            if (sensors.Length != sms.Length) {
                throw new Exception("sensors types don't match state machines count");
            }


            InvokeRepeating(nameof(AdvanceSensors), 0, tickRate);
        }

        private void AdvanceSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                SM<SMSensor> curSM = sms[i];
                for (int j = 0; j < sensors[i].Length; j++) {
                    curSM.AdvanceSensor(sensors[i][j]);
                }
            }
        }
    }
}