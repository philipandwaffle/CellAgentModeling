using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Agent {
    public class AgentSpawner : MonoBehaviour {
        [SerializeField] Sprite agentSprite;
        [SerializeField] private int[] agentCounts;
        [SerializeField] float spawnPoint = 50f;
        [SerializeField] float spawnRange = 10f;
        private int numLayerSpawns;

        public void InitAgents(ref IStateMachine sm, ref BaseSensor[] baseSensors, Queue<Vector2>[] spawnLocations) {
            DestroySensors(ref baseSensors);

            Debug.Log("Spawning agents");
            bool spawnable = true;

            numLayerSpawns = math.min(spawnLocations.Length, agentCounts.Length);
            for (int z = 0;z < numLayerSpawns; z++) {
                if (agentCounts[z] > spawnLocations[z].Count) {
                    Debug.LogError("Insufficent spawn locations on layer " + z);
                    spawnable = false;
                }
            }
            if (!spawnable) { 
                Debug.Log("Not spawning agents");
                return; 
            }

            sm = GetNavSM();

            baseSensors = SpawnAgents(agentCounts, spawnLocations);
        }
        public void InitAgents(ref IStateMachine sm, ref BaseSensor[] baseSensors, SerializableVector3[] spawnLocations) {
            DestroySensors(ref baseSensors);

            Debug.Log("Spawning agents");
            sm = GetNavSM();

            baseSensors = SpawnAgents(spawnLocations);
        }

        // Destroy currently loaded sensors
        private void DestroySensors(ref BaseSensor[] baseSensors) {
            BaseSensor.nextId = 0;
            if (baseSensors is not null) {
                for (int i = 0; i < baseSensors.Length; i++) {
                    Destroy(baseSensors[i].gameObject);
                }
            }
        }

        // Spawn agents from randomised queue
        private BaseSensor[] SpawnAgents(int[] agentCounts, Queue<Vector2>[] spawnLocations) {
            List<BaseSensor> baseSensors = new List<BaseSensor>();

            GameObject prefab = new GameObject();
            SpriteRenderer sr =  prefab.AddComponent<SpriteRenderer>();
            sr.sprite = agentSprite;

            for (int z = 0; z < numLayerSpawns; z++) {
                for (int i = 0; i < agentCounts[z]; i++) {
                    GameObject instance = Instantiate(prefab, spawnLocations[z].Dequeue(), Quaternion.identity, transform);
                    NavLayerSensor sen = instance.AddComponent<NavLayerSensor>();
                    instance.transform.localScale = new Vector3(.5f, .5f, .5f);
                    sen.MoveLayer(z);

                    baseSensors.Add(sen);
                }
            }
            Destroy(prefab);

            return baseSensors.ToArray();
        }
        // Spawn agents from saved locations
        private BaseSensor[] SpawnAgents(SerializableVector3[] spawnLocations) {
            List<BaseSensor> baseSensors = new List<BaseSensor>();

            GameObject prefab = new GameObject();
            SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = agentSprite;

            for (int i = 0; i < spawnLocations.Length; i++) {
                SerializableVector3 pos = spawnLocations[i];

                GameObject instance = Instantiate(prefab, new Vector2(pos.x, pos.y), Quaternion.identity, transform);
                NavLayerSensor sen = instance.AddComponent<NavLayerSensor>();   
                instance.transform.localScale = new Vector3(.5f, .5f, .5f);
                sen.MoveLayer(pos.z);

                baseSensors.Add(sen);
            }
            Destroy(prefab);

            return baseSensors.ToArray();
        }

        private BaseSensor[] CreateLayerSensors(int agentCount) {
            BaseSensor[] sensors = new BaseSensor[agentCount];
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

        private BaseSensor[] CreateNavSensors(int agentCount) {
            BaseSensor[] sensors = new BaseSensor[agentCount];
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
                new IInput<MultiLayerSensor>[] { isCold, isHot, isFar, isNear, isOnSteps },
                new Dictionary<int, (int, int)[]>() {
                    { 0, new (int, int)[] { (2, 1), (0, 2), (4, 4) } },
                    { 1, new (int, int)[] { (0, 3), (3, 0), (4, 4) } },
                    { 2, new (int, int)[] { (1, 0), (2, 3), (4, 4) } },
                    { 3, new (int, int)[] { (3, 2), (1, 1), (4, 4) } },
                    { 4, new (int, int)[] { (0, 3), (1, 1), (2, 3), (3, 2) } },
                }
            );
        }

        private IStateMachine<NavLayerSensor> GetNavSM() {
            float dirModifier = 0.5f;
            State<NavLayerSensor> initPath = new(
                (s) => {
                    s.InitPath();
                }
            );
            State<NavLayerSensor> updatePath = new(
                (s) => {
                    s.UpdatePath();
                }
            );
            State<NavLayerSensor> escape = new(
                (s) => {
                    s.moveCounter++;
                    s.ApplyForce(dirModifier * s.GetDir());
                }
            );
            State<NavLayerSensor> takeSteps = new(
                (s) => {
                    s.moveCounter++;
                    s.MoveLayer(-1);
                }    
            );

            Input<NavLayerSensor> hasPath = new((s) => { return s.HasPath(); });
            Input<NavLayerSensor> onSteps = new((s) => { return s.ReadValue() == -2; });
            Input<NavLayerSensor> offSteps = new((s) => { return s.ReadValue() != -2; });
            Input<NavLayerSensor> needsUpdate = new((s) => {
                if (s.moveCounter % 300 == 0) {
                    s.moveCounter = 1;
                    return true;
                } else {
                    return false;
                }
            });

            return new StateMachine<NavLayerSensor>(
                new IState<NavLayerSensor>[] { initPath, updatePath, escape, takeSteps },
                new IInput<NavLayerSensor>[] { hasPath, needsUpdate, onSteps, offSteps },
                new Dictionary<int, (int, int)[]>() {
                    { 0, new (int, int)[] { (0, 2)} },
                    { 1, new (int, int)[] { (0, 2) } },
                    { 2, new (int, int)[] { (1, 1), (2, 3) } },
                    { 3, new (int, int)[] { (2, 3), (3, 0) } },
                },
                true
            );
        }
    }
}