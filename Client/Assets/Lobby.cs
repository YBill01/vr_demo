using TMPro;
using Transport.Client;
using Transport.Universal;
using UnityEngine;

public class Lobby : MonoBehaviour {
    [SerializeField] private AvatarApi _avatarApi;
    [SerializeField] private Lobby _joinScreen;
    [SerializeField] private TextMeshProUGUI _userCountLabel;
    [SerializeField] private bool _isBot;

    [SerializeField] private Vector3 _worldPosition;
    [SerializeField] private Quaternion _worldRotation;
    [SerializeField] private Vector2 _worldSize;
    [SerializeField] private Canvas _canvas;

    // Special input fields must be used for VR.
    [SerializeField] private GameObject[] pcInputFields;
    [SerializeField] private GameObject[] vrInputFields;

    private int _userCount;

    private void Awake() {
        if (_isBot)
            CoroutineBehavior.Delay(1f, SetUsernameAndJoin);


#if UNITY_STANDALONEs
            SetupForPC();
#endif
#if UNITY_ANDROID
        SetupForVR();
#endif
    }

    [ContextMenu("Setup PC canvas")]
    public void SetupForPC() {
        // Switch on PC input fields, switch off VR ones.
        for (int i = 0; i < pcInputFields.Length; i++)
            pcInputFields[i].SetActive(true);
        for (int i = 0; i < vrInputFields.Length; i++)
            vrInputFields[i].SetActive(false);

        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.transform.localScale = new Vector3(0.287f, 0.287f, 0.287f);
    }

    [ContextMenu("Setup VR canvas")]
    public void SetupForVR() {
        // Switch on VR input fields, switch off PC ones.
        for (int i = 0; i < pcInputFields.Length; i++)
            pcInputFields[i].SetActive(false);
        for (int i = 0; i < vrInputFields.Length; i++)
            vrInputFields[i].SetActive(true);

        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
        _canvas.transform.localPosition = _worldPosition;
        _canvas.transform.localRotation = _worldRotation;
        _canvas.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _worldSize.x);
        _canvas.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _worldSize.y);
    }

    public void InitWorldCanvasCamera(Camera camera) {
        print("Init " + camera);
        _canvas.worldCamera = camera;
        print(_canvas.worldCamera);
    }
    public void SetUsernameAndJoin() {

        var client = Game.GetManager<BattleClientCore>();
        
        Debug.Log($"client==null: {client==null}",gameObject);
        if (_isBot)
            client.ChangeUserInfo("Guest" + UnityEngine.Random.Range(0, 200), "Guest");
        else
            client.ChangeUserInfo(_avatarApi.LoginText + UnityEngine.Random.Range(0, 20), "UserID");

        client.SkinsName = AvatarSkinHolder.Instance.GetActiveSkins();
        client.OpenConnection(OnConnected, OnConnectionFailed, "BattleServer");

        void OnConnected(HostInfo hostInfo) 
        {

            Debug.Log("Success connected to server!");

            var netObjects = Game.AddManager(new Client_NetworkObjects());
            netObjects.Final();
            netObjects.OnNetObj_Created += OnNetObjectCreated;
            netObjects.OnNetObj_Destroyed += OnNetObjectDestroyed;

            Game.AddManager(new LobbyManager(client));

            _joinScreen.gameObject.SetActive(false);
            _avatarApi.gameObject.SetActive(false);
            ConnectToChatServer();
        }
        void OnConnectionFailed(string obj) {
            Debug.Log(string.Format("Connection failed! Reason: {0}", obj));

        }

    }

    private void ConnectToChatServer()
    {
        var chatClient = Game.GetManager<Chatting>();
        chatClient.ConnectToChatServer(OnConnected,OnConnectionFailed);
        
        void OnConnected() 
        {
            Debug.Log("Success connected to chat server!");
        }
        void OnConnectionFailed(string obj) 
        {
            Debug.Log(string.Format("Connection failed! Reason: {0}", obj));
        }
    }

    // Test label | TODO - remove
    private void OnNetObjectCreated(NetworkObject obj) {
        // if (obj is PlayerNetworkObject) {
        // _userCount++;
        //_userCountLabel.text = string.Format("Users: {0}", _userCount);
        // }
    }
    private void OnNetObjectDestroyed(NetworkObject obj) {
        //if (obj is PlayerNetworkObject) {
        //_userCount--;
        //_userCountLabel.text = string.Format("Users: {0}", _userCount);
        // }
    }
}
