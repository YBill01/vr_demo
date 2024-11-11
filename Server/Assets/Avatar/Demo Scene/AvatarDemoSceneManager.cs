using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AvatarDemoSceneManager : MonoBehaviour {
    [SerializeField] private Avatar avatar1;
    [SerializeField] private Avatar[] otherAvatars;

    public Avatar.EmotionType testEmotion;

    [SerializeField] private TMP_Dropdown emotionsList;
    [SerializeField] private TMP_Dropdown animationList;

    void Awake() {

        // Emotions.
        emotionsList.ClearOptions();
        string[] emotions = Enum.GetNames(typeof(Avatar.EmotionType));
        emotionsList.AddOptions(emotions.ToList());

        // Animations.
        animationList.ClearOptions();
        string[] anims = Enum.GetNames(typeof(Avatar.AnimationType));
        animationList.AddOptions(anims.ToList());
    }

    [ContextMenu("Change emotion")]
    private void ChangeEmotion() {
        avatar1.ChangeEmotion(testEmotion, false);
    }

    [ContextMenu("Change emotion smoothly")]
    private void ChangeEmotionSmoothly() {
        avatar1.ChangeEmotion(testEmotion, true);
    }

    [ContextMenu("Transfer avatar skins")]
    public void TransferAvatarSkins() {
        foreach (Avatar otherAvatar in otherAvatars)
            otherAvatar.SetSkins(avatar1.GetSkinsInformation());
    }

    public void ChangeEmotionFFromDropDown() {
        avatar1.ChangeEmotion((Avatar.EmotionType)emotionsList.value, true);
        foreach (Avatar otherAvatar in otherAvatars)
            otherAvatar.ChangeEmotion((Avatar.EmotionType)emotionsList.value, true);
    }

    public void PlayAnimationFromDropDown() {
        avatar1.PlayAnimation((Avatar.AnimationType)animationList.value);
        foreach (Avatar otherAvatar in otherAvatars)
            otherAvatar.PlayAnimation((Avatar.AnimationType)animationList.value);
    }

    #region Editor
#if UNITY_EDITOR
    [ContextMenu("Editor: Set Dirty")]
    private void Editor_SetDirty() {
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
    }
#endif
    #endregion Editor
}
