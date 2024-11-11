
using TMPro;
using UnityEngine;
using Transport.Messaging;
using Transport.Universal;

public class TextChatClient : MonoBehaviour {

    [SerializeField] private TMP_InputField nameInput;
    public string ownName; // TODO remake this.

    [SerializeField] private TextChatUI chatUI;

    // 2 different chat panels. One - for PC version, another - for VR version (included to VR tablet).
    [SerializeField] private TextChatUI chatUiPC;
    [SerializeField] private TextChatUI chatUiVR;

    private HostInfo _host;

    void Start() {
        /*Multihost.OnHostConnectionOpen += OnHostConnectionOpen;
        Multihost.OnHostConnectionClosed += OnHostConnectionClosed;

        // Select chat panel.*/
        if (Application.platform == RuntimePlatform.Android) {
            chatUI = chatUiVR;
            chatUiPC.gameObject.SetActive(false);
        }
        else {
            chatUI = chatUiPC;
            chatUiVR.gameObject.SetActive(false);
        }

        var chatting = Game.GetManager<Chatting>();
        chatting.OnChatMsgReceived += ReceiveMessage;
    }

    private void ReceiveMessage(string username, string message)
    {
        chatUiPC.ShowNewMessage(username + ": " + message);
    }

    /*[ContextMenu("Initialize")]
    public void Initialize() {
        Multihost.GetHost("BattleServer", out _host);
        Multihost.MsgReader.AddHandler(MsgType.ChatMessage, OnTextMsgReceived);

        Debug.Log("Text chat is Initialized", gameObject);

    }
    
    private void OnHostConnectionOpen(int connectid, int hostid, HostInfo host) {
        Debug.Log($"OnHostConnectionOpen. connectid={connectid}, hostid={hostid}, host.HostName={host.HostName} ", gameObject);
        if (host.HostName == "BattleServer") {
            Debug.Log($"It is battle server!", gameObject);
            Initialize();
            chatUI.ShowChatPanel(true);
        }

        // Remember own name to add it to messages. Temporary. // TODO Delete this.
        if (nameInput != null && string.IsNullOrEmpty(nameInput.text) == false)
            ownName = nameInput.text;
        else
            ownName = "SomePlayer";
    }

    private void OnHostConnectionClosed(int connectid, int hostid, HostInfo host) {
        chatUI.ShowChatPanel(false);
    }

    private void OnTextMsgReceived(NetworkMsg msg) {
        var line = msg.reader.ReadString();
        Debug.Log(string.Format("New chat msg: {0}", line));
        chatUI.ShowNewMessage(line);
    }*/

    /// <summary>
    /// Send message to battle server. Depending on the server settings, it will be sent to all users by default.
    /// </summary>
    public void SendTextMsg(string line) {
        Debug.Log("Send message");
       Game.GetManager<Chatting>().SendChatMsg(line);
    }

    #region Test

    [Header("Tests")]
    [SerializeField] private string testText;

    [ContextMenu("Test send message to server")]
    public void TestSendToServer() {
        SendTextMsg(testText);

        Debug.Log("Sent", gameObject);
    }

    #endregion
}
