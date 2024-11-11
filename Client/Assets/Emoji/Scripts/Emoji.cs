
using UnityEngine;

namespace Emoji {

    /// <summary>
    /// This script is placed on Emoji prefab. It is little animated particle system effect.
    /// </summary>
    public class Emoji : MonoBehaviour {
        [SerializeField] private float centralIconHeight = 3f;

        [Header("Cached links")]
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private ParticleSystem.MainModule psMain;
        [SerializeField] private ParticleSystem particleSystem2;
        [SerializeField] private ParticleSystemRenderer psRender;
        [SerializeField] private ParticleSystemRenderer psRender2;

        [Header("Materials")] public Material loveEmojiMaterial;
        public Material wonderEmojiMaterial;
        public Material happyEmojiMaterial;
        public Material lolEmojiMaterial;
        public Material approvalEmojiMaterial;
        public Material whimperEmojiMaterial1;
        public Material whimperEmojiMaterial2;

        private const float lifeTime = 4f;

        private float destroyTime;

        private void Update()
        {
            if (Time.time > destroyTime)
                DestroyEmoji();
        }
        
        /// <summary>
        /// Animate emoji of given type.
        /// </summary>
        public void Initialize(Type type) {
            SetParticlesMaterial(type);
            gameObject.SetActive(true);
            destroyTime = Time.time + lifeTime;
        }

        /// <summary>
        /// Destroy emoji game object.
        /// </summary>
        public void DestroyEmoji() {
            Destroy(gameObject);
        }

        private void SetParticlesMaterial(Type type) {
            switch (type) {
                case Type.Approval:
                    psRender.sharedMaterial = approvalEmojiMaterial;
                    psRender2.sharedMaterial = approvalEmojiMaterial;
                    SetupForJenericEmoji();
                    break;
                case Type.Happy:
                    psRender.sharedMaterial = happyEmojiMaterial;
                    psRender2.sharedMaterial = happyEmojiMaterial;
                    SetupForJenericEmoji();
                    break;
                case Type.LOL:
                    psRender.sharedMaterial = lolEmojiMaterial;
                    psRender2.sharedMaterial = lolEmojiMaterial;
                    SetupForJenericEmoji();
                    break;
                case Type.Love:
                    psRender.sharedMaterial = loveEmojiMaterial;
                    psRender2.sharedMaterial = loveEmojiMaterial;
                    SetupForJenericEmoji();
                    break;
                case Type.Wonder:
                    psRender.sharedMaterial = wonderEmojiMaterial;
                    psRender2.sharedMaterial = wonderEmojiMaterial;
                    SetupForJenericEmoji();
                    break;
                case Type.Whimper:
                    psRender.sharedMaterial = whimperEmojiMaterial1;
                    psRender2.sharedMaterial = whimperEmojiMaterial2;
                    SetupForWhimper();
                    break;
            }
        }

        private void SetupForJenericEmoji() {
            psMain = particleSystem.main;
            psMain.gravityModifier = 0f;
            particleSystem2.transform.localPosition = new Vector3(0f, centralIconHeight, 0f);
        }

        // Whimper must be higher than default emoji.
        private void SetupForWhimper() {
            transform.position = transform.position + Vector3.up;
            psMain = particleSystem.main;
            psMain.gravityModifier = 0.3f;
            particleSystem2.transform.localPosition = new Vector3(0f, centralIconHeight - 1f, 0f);
        }

        /// <remarks>
        /// In alphabetical order.
        /// </remarks>
        public enum Type : byte {
            Approval = 0,
            Happy = 1,
            LOL = 2,
            Love = 3,
            Wonder = 4,
            Whimper = 5, // Cry
        }
    }
}