using Emoji;
using UnityEngine;
using UnityEngine.UI;

public class EmojiButtonInitializer : MonoBehaviour
{
   public static EmojiButtonInitializer Instance;
   
   [SerializeField] private Button _likeButton;
   [SerializeField] private Button _smileButton;
   [SerializeField] private Button _laughtButton;
   [SerializeField] private Button _heartButton;
   [SerializeField] private Button _cryButton;
   [SerializeField] private Button _wonderButton;
   
   [SerializeField] private EmojiManager _emojiManager;

   private void Awake() => Instance = this;

   private void OnEnable()
   {
      _likeButton.onClick.AddListener(LikeButtonOnClick);
      _smileButton.onClick.AddListener(SmileButtonOnClick);
      _laughtButton.onClick.AddListener(LaughtButtonOnClick);
      _heartButton.onClick.AddListener(HeartButtonOnClick);
      _cryButton.onClick.AddListener(CryButtonOnClick);
      _wonderButton.onClick.AddListener(WonderButtonOnClick);
   }

   private void LikeButtonOnClick()
   {
      _emojiManager.CreateEmojiNetwork(Emoji.Emoji.Type.Approval);
   }
   private void SmileButtonOnClick()
   {
      _emojiManager.CreateEmojiNetwork(Emoji.Emoji.Type.Happy);
   }
   private void LaughtButtonOnClick()
   {
      _emojiManager.CreateEmojiNetwork(Emoji.Emoji.Type.LOL);
   }
   private void HeartButtonOnClick()
   {
      _emojiManager.CreateEmojiNetwork(Emoji.Emoji.Type.Love);
   }
   private void CryButtonOnClick()
   {
      _emojiManager.CreateEmojiNetwork(Emoji.Emoji.Type.Whimper);
   }
   private void WonderButtonOnClick()
   {
      _emojiManager.CreateEmojiNetwork(Emoji.Emoji.Type.Wonder);
   }
   
}
