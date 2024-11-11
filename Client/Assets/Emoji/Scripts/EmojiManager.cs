using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Emoji 
{
    /// <summary>
    /// Place 'Emoji Manager' prefab to your scene before using emoji.
    /// Call public method 'CreateEmoji' to instantiate some emoji.
    /// </summary>
    public class EmojiManager : MonoBehaviour 
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private Emoji emojiPrefab;
        [SerializeField] private NetworkEmoji _networkEmoji;
        
        public Emoji.Type CurrentEmojiType;

        /// <summary>
        /// Instantiate and play emoji at given position, parenting it to given transform.
        /// </summary>
        /// <param name="parent">Can be null.</param>

        private void _CreateEmoji(Emoji.Type emojiType, Transform parent, Vector3 pos) {

            Emoji emoji = Instantiate(emojiPrefab, pos, Quaternion.identity, parent);
            emoji.Initialize(emojiType);
        }
        
        /// <summary>
        /// Instantiate and play emoji at given position, parenting it to given transform.
        /// </summary>
        /// <param name="parent">Can be null.</param>
        
        public void CreateEmoji(Emoji.Type emojiType) 
        {
            CurrentEmojiType = emojiType;
            _CreateEmoji(emojiType, transform, transform.position);
        }
        
        public void CreateEmojiNetwork(Emoji.Type emojiType) 
        {
            CurrentEmojiType = emojiType;
            _networkEmoji.CreateEmoji((byte)emojiType);
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
}
