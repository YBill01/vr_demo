using TMPro;
using UnityEngine;

/// <summary> Input field for Oculus VR. </summary>
public class OculusInputField : MonoBehaviour {
    [SerializeField] public string defaultText;
    [SerializeField] public TMP_Text textLabel;

    private TouchScreenKeyboard overlayKeyboard;
    private static OculusInputField currentInputField;

    void Start() {
        textLabel.text = defaultText;
    }

    void Update() {
        if (currentInputField == this && overlayKeyboard != null)
            textLabel.text = overlayKeyboard.text;
    }

    public void StartEditing() {
        currentInputField = this;
        overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        if (overlayKeyboard != null)
            overlayKeyboard.text = textLabel.text == defaultText ? "" : textLabel.text;
    }
}
