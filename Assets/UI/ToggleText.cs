using Assets.UI;
using TMPro;
using UnityEngine;

public class ToggleText : MonoBehaviour {
    [SerializeField] private MonoBehaviour target;
    [SerializeField] private TMP_Text text;
    [SerializeField] private string trueString;
    [SerializeField] private string falseString;
    private IToggleable targetToggle;

    private void Start() {
        targetToggle = (IToggleable)target;
    }

    public void Toggle() {
        bool state = targetToggle.GetState();
        if (state) {
            text.text = trueString;
        } else {
            text.text = falseString;
        }
    }
}
