using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AutoScale : MonoBehaviour
{
    [SerializeField]
    List<RectTransform> children;
    [SerializeField]
    List<float> xScales;
    [SerializeField]
    List<float> yScales;

    RectTransform me;
    // Start is called before the first frame update
    void Start() {
        me = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScaleChildren() {
        int cCount = children.Count;
        if (cCount != xScales.Count || cCount != yScales.Count) {
            throw new MissingComponentException("The list counts aren't the same");
        }
        for (int i = 0; i < children.Count; i++) {

        }
    }
}
