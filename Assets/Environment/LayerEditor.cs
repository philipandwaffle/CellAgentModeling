using Assets.Environment;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayerEditor : MonoBehaviour {
    [SerializeField] Sprite displaySprite;    
    [SerializeField] TMP_InputField inputField;

    private Dictionary<Vector2, (GameObject, float)> points;
    private int width = 0;
    private int height = 0;
    private Vector2 prevMouse = Vector2.zero;
    private float value = 0f;    

    private void Start() {
        points = new Dictionary<Vector2, (GameObject, float)>();
    }

    private void Update() {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
            Vector2 curMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            curMouse = new Vector2((int)curMouse.x + .5f, (int)curMouse.y + .5f);
            if (prevMouse != curMouse) {
                prevMouse = curMouse;
                AddPoint(new Vector2(curMouse.x, curMouse.y), value);
            }
        }        
    }
    

    public void SetValue(string value) {
        Debug.Log(value);
        float val;
        if (float.TryParse(value, out val)) {
            this.value = val;
        }
    }

    public void AddPoint(Vector2 p, float value) {
        if (points.ContainsKey(p)) {
            (GameObject, float) point = points[p];
            point.Item1.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(value, 1, 1);
            point.Item2 = value;
            points[p] = point;
        } else {
            if (p.x > width) {
                width = (int)p.x;
            }
            if (p.y > height) {
                height = (int)p.y;
            }
            points.Add(p, (SpawnSprite(p, value), value));
        }        
    }
    private void RemoveValue(Vector2 p) {
        Destroy(points[p].Item1);
        points.Remove(p);
    }

    private GameObject SpawnSprite(Vector2 p, float value) {
        GameObject go = new GameObject();
        go.transform.position = p;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = displaySprite;
        sr.color = Color.HSVToRGB(value, 1, 1);
        return go;
    }

    private Layer GenerateLayer() {
        float[,] values = new float[width, height];
        float[,] m = {
            { 1f, 1f, 1f },
            { 1f, 1.05f, 1f },
            { 1f, 1f, 1f }
        };

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                values[x, y] = 0f;
            }
        }
        foreach (Vector2 key in points.Keys) {
            int x = (int)key.x;
            int y = (int)key.y;
            values[x, y] = points[key].Item2;
        }

        return new Layer(width, height, m, values);
    }

    private class IVector2 {
        public int x, y;
        public IVector2(int x, int y) {
            this.x = x; this.y = y;
        }
    }
}
