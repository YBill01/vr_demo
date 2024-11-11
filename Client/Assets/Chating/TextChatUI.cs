
using TMPro;
using Transport.Universal;
using UnityEngine;

public class TextChatUI : MonoBehaviour {

    [SerializeField] private int maxChatSymbols = 1000;

    [Header("Links")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_Text chatLabel;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private OculusInputField oculusInputField;
    [SerializeField] private TextChatClient textChatManager;

    void Start() {
        ClearChat();

        // Select input field.
        if (Application.platform == RuntimePlatform.Android) {
            chatInput?.gameObject.SetActive(false);
            oculusInputField?.gameObject.SetActive(true);
        }
        else {
            chatInput?.gameObject.SetActive(true);
            oculusInputField?.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by UI button.
    /// </summary>
    public void SendText() {

        if (Application.platform == RuntimePlatform.Android) {

            if (oculusInputField == null)
                return;
            if (!string.IsNullOrEmpty(oculusInputField.textLabel.text)) {
                textChatManager.SendTextMsg(oculusInputField.textLabel.text);
                oculusInputField.textLabel.text = "";
            }
        }
        else {
            if (chatInput == null)
                return;
            if (!string.IsNullOrEmpty(chatInput.text)) {
                textChatManager.SendTextMsg(chatInput.text);
                chatInput.text = "";
            }
        }
    }

    public void ClearChat() {
        chatLabel.text = "";
        chatInput.text = "";
    }

    /// <summary>
    /// Show new line in chat panel.
    /// </summary>
    public void ShowNewMessage(string message) {
        Debug.Log($"Current text length: {chatLabel.text.Length}", gameObject);

        // Cut chat if it is too long.
        if (chatLabel.text.Length > maxChatSymbols) {
            chatLabel.text = chatLabel.text.Substring(0, maxChatSymbols - 100);
        }

        chatLabel.text = chatLabel.text + "\n" + message;
    }

    public void ShowChatPanel(bool enabled) {
        chatPanel.SetActive(enabled);
    }

    [ContextMenu("Test show all info")]
    private void TestShowInfo() {
        Debug.Log($"Total connections: {Multihost.ConnectionsManager.GetAllConnections.Count}", gameObject);

        int c = 0;
        foreach (var conn in Multihost.ConnectionsManager.GetAllConnections.Values) {
            c++;
            Debug.Log($"{c} NetworkId: {conn.NetworkId}, ConnectId: {conn.ConnectId}, HostId: {conn.HostId}, MasterNetworkId: {conn.MasterNetworkId}", gameObject);

            Multihost.UsersManager.GetUser(conn.NetworkId, out User user);
            if (user != null) {
                Debug.Log($"UserId: {user.UserId}, Username: {user.Username}", gameObject);
            }
        }
    }
}
