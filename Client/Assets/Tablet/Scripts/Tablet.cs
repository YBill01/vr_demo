using UnityEngine;

public class Tablet : MonoBehaviour {

    [Header("Links")]
    [SerializeField] private GameObject emojiScreen;
    [SerializeField] private GameObject animationScreen;
    [SerializeField] private GameObject settingScreen;
    [SerializeField] private GameObject chatScreen;

    #region Screens manipulation

    /// <summary>
    /// Hide all tablet screen. Call this before show some screen.
    /// </summary>
    private void CloseAllScreens() {
        emojiScreen.SetActive(false);
        animationScreen.SetActive(false);
        settingScreen.SetActive(false);
        chatScreen.SetActive(false);
    }

    // Linked to Inspector button.
    public void SwitchToSettingScreen() {
        CloseAllScreens();
        settingScreen.SetActive(true);
    }

    // Linked to Inspector button.
    public void SwitchToEmojiScreen() {
        CloseAllScreens();
        emojiScreen.SetActive(true);
    }

    // Linked to Inspector button.
    public void SwitchToAnimationScreen() {
        CloseAllScreens();
        animationScreen.SetActive(true);
    }

    // Linked to Inspector button.
    public void SwitchToChatScreen() {
        CloseAllScreens();
        chatScreen.SetActive(true);
    }

    #endregion
}
