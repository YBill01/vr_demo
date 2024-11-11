using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using static System.Int32;

public class AvatarApi : MonoBehaviour
{
	public static AvatarApi instance;


	public OnlineState onlineState;
    
    public int userID = -1;

    private string _userName;
    
    public string UserName
    {
        get
        {
            if (_userName == null)
                return "Empty";
            return _userName;
        }
    }

    public int[] friendsIDs;
    public string token;
    public string oldApiURL = "https://xenera.aestar.com.ua"; // https://xenera.aestar.com.ua
    public string registerApiURL = "https://xenera.aestar.com.ua/api/user/register?";

    [Header("API 2.0 (by Max Zubko)")]
    public string apiURL;
    [Tooltip("Minutes.")]
    public int tokenRelevanceTime = 30; // Minutes.

    [Header("Test variables")]
    [SerializeField] private TMP_InputField _loginInputField;
    [SerializeField] private OculusInputField _loginInputFieldVR;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private OculusInputField _passwordInputFieldVR;
    [SerializeField] private TextMeshProUGUI _loginResultText;
    public string LoginText
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
                return _loginInputFieldVR.textLabel.text;
            else
                return _loginInputField.text;
        }
    }

    public string PasswordText
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
                return _passwordInputFieldVR.textLabel.text;
            else
                return _passwordInputField.text;
        }
    }


    /*public bool FriendsList;
    public bool FriendsAdd;
    public string someUserName;
    public delegate void FriendsListChangeDelegate();
    /// <summary> It is invoked when list of friends is changed. </summary>
    public FriendsListChangeDelegate OnFriendsListChange;*/

    private void Awake()
    {
        instance = this;
    }
    public bool UserIsLogged()
    {
        return userID > 0 && token != "";
    }
    
    public void Login()
    {
        if (LoginText == string.Empty || PasswordText == string.Empty)
        {
            Debug.Log($"Enter name and password.");
            _loginResultText.text = $"Enter name and password.";
            return;
        }

        StartCoroutine(ProcessLogin());
    }
    public void Register()
    {
        if (PasswordText == string.Empty || LoginText == string.Empty)
        {
            Debug.Log($"Enter name and password.");
            _loginResultText.text = $"Enter name and password.";
            return;
        }

        StartCoroutine(ProcessRegister());
    }

    protected IEnumerator ProcessRegister()
    {
        Debug.Log($"Start registering process.");
        onlineState = OnlineState.TryingLogin;

        float interruptTime = Time.time + 10f;
        WWWForm form = new WWWForm();
        form.AddField("username", LoginText);
        form.AddField("password", PasswordText);
        using (UnityWebRequest request = UnityWebRequest.Post("https://xenera.aestar.com.ua/api/user/register?", form))
        {
            yield return request.SendWebRequest();

            // Wait.
            while (request.downloadHandler.isDone == false && Time.time < interruptTime)
            {
                Debug.Log($"Pending... downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
                yield return new WaitForSeconds(1f);
            }

            // Result.
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request answer is received. downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
                Debug.Log(request.downloadHandler.text);
                if (request.downloadHandler.text.Contains("201"))
                {
                    Debug.Log($"User Registered Successfully.");
                    _loginResultText.text = "User Registered Successfully.";
                    //SoundManager.instance.PlayConfirmSound();
                    StartCoroutine(ProcessLogin());
                }
                else if (request.downloadHandler.text.Contains("400"))
                {
                    Debug.Log($"Username already registered.");
                    _loginResultText.text = "Username already registered.";
                }
            }
            else
            {
                onlineState = OnlineState.None;
                //SoundManager.instance.PlayErrorSound();
                Debug.Log($"Request.result!=Success. Error={request.error}, downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
            }
        }
    }

    protected IEnumerator ProcessLogin()
    {
        Debug.Log($"Start logging process.");

        onlineState = OnlineState.TryingLogin;
        //RefreshLoginButton();

        float interruptTime = Time.time + 10f;
        WWWForm form = new WWWForm();
        form.AddField("username", LoginText);
        form.AddField("password", PasswordText);
        using (UnityWebRequest request = UnityWebRequest.Post("https://xenera.aestar.com.ua/api/user/login?", form))
        {
            yield return request.SendWebRequest();

            // Wait.
            while (request.downloadHandler.isDone == false && Time.time < interruptTime)
            {
                Debug.Log($"Pending... downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
                yield return new WaitForSeconds(1f);
            }

            // Result.
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request answer is received. downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}, text={request.downloadHandler.text}");

                if (request.downloadHandler.text.Contains("code\":\"200"))
                {
                    // 200 - Logged successfully. Parse token and try to get avatar.
                    AvatarStruct avStruct = JsonUtility.FromJson<AvatarStruct>(request.downloadHandler.text);
                    //SoundManager.instance.PlayConfirmSound();
                    token = avStruct.data.access_token;
                    _userName = LoginText;
                    //SearchForOwnID();
                    StartCoroutine(ProcessGetAvatarData(avStruct.data.access_token));
                    _loginResultText.text = "Login success";
                }
                else if (request.downloadHandler.text.Contains("code\":\"400"))
                {
                    // 400 - Incorrect username or password.
                    onlineState = OnlineState.None;
                    //SoundManager.instance.PlayErrorSound();
                    userID = -1;
                    //RefreshLoginButton();
                }
            }
            else
            {
                onlineState = OnlineState.None;
                //RefreshLoginButton();
                //SoundManager.instance.PlayErrorSound();
                userID = -1;
                Debug.Log($"Request.result!=Success. Error={request.error}, downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
            }
        }
    }

    protected IEnumerator ProcessGetAvatarData(string token)
    {
        Debug.Log($"Start getting Avatar's Data with token {token}");
        float interruptTime = Time.time + 10f;

        using (UnityWebRequest request = UnityWebRequest.Post("https://xenera.aestar.com.ua/api/avatar/get?", ""))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            // Wait.
            while (request.downloadHandler.isDone == false && Time.time < interruptTime)
            {
                Debug.Log($"Pending... downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
                yield return new WaitForSeconds(1f);
            }

            // Result.
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request answer is received. downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}, text={request.downloadHandler.text}");

                // 200 - Avatar is received.
                if (request.downloadHandler.text.Contains("code\":\"200"))
                {
                    //SoundManager.instance.PlayConfirmSound();
                    SetDownloadedAvatarSkin(request.downloadHandler.text);
                }
                // 404 - ???
                else if (request.downloadHandler.text.Contains("code\":\"404"))
                {
                    //SoundManager.instance.PlayErrorSound();
                }
                // TODO 'No avatar'.

                onlineState = OnlineState.Logged;
                //RefreshLoginButton();
            }
            else
            {
                onlineState = OnlineState.None;
                //RefreshLoginButton();
                //SoundManager.instance.PlayErrorSound();
                Debug.Log($"Request.result!=Success. Error={request.error}, downloadHandler.isDone={request.downloadHandler.isDone}, request.isDone={request.isDone}");
            }
        }
    }

    protected void SetDownloadedAvatarSkin(string rawString)
    {
        //GameManager.instance.demoAvatar.SetSkin(SkinNames(rawString));
    }

    protected string[] SkinNames(string rawString)
    {
        List<string> skinNames = new List<string>();
        int charIndex = 0;
        int counter = 0;

        // Find all 6-digit numbers.
        while (charIndex >= 0 && charIndex + 6 < rawString.Length && counter < 100)
        {
            charIndex = rawString.IndexOf("\"1", charIndex);
            if (charIndex >= 0 && charIndex + 6 < rawString.Length)
            {
                skinNames.Add(rawString.Substring(charIndex + 1, 6));
                charIndex += 7;
                counter++;
            }
            else
                break;
        }

        return skinNames.ToArray();
    }


    public struct AvatarStruct
    {
        public string status;
        public string code;
        public string message;
        public Data data;

        [System.Serializable]
        public struct Data
        {
            public string access_token;
            public string refresh_token;
        }
    }
    
    public enum OnlineState : byte
    {
        None,
        TryingLogin,
        Logged,
    }
}