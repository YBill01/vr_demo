using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Cat change clothes, can play animations.
/// </summary>
public class Avatar : MonoBehaviour {
    [Tooltip("Used to play emotions.")]
    [SerializeField] private SkinnedMeshRenderer face;

    [Header("Skins")]
    [SerializeField] private Transform rootTransform;
    [SerializeField] private LODGroup lodGroup;
    [SerializeField] private BodyLodRenders[] bodyRenders;
    public static readonly float[] lodTransitionLevels = { 0.6f, 0.3f, 0.1f };
    private List<GameObject> instantiatedSkinObjs;
    [SerializeField] private List<SkinnedMeshRenderer>[] skinsLods;
    [SerializeField] private GameObject[] bones;
    [SerializeField, ReadOnly]private List<SkinPrefabInfo> skins;

    [Header("Other links")]
    public Animator animator;

    [Header("Debug & Tests")]
    [Tooltip("Automatically play 'Walk' animation if delta position is greater than zero.")]
    [SerializeField] private bool autoWalkAnimation;

    // Time for change emotion if using 'smooth changing'.
    private const float emotionChangeTime = 0.6f;
    private Vector3 prevPosition;
    private Coroutine emotionChangingRoutine;

    private void Awake() 
    {
        Init();
    }
    
    private void Update()
    {
        if (autoWalkAnimation)
            CheckAnimation();
    }

    private void Init() {
        CheckLists();
    }

    private void CheckLists() {
        instantiatedSkinObjs ??= new List<GameObject>();
        if (skinsLods == null) {
            skinsLods = new List<SkinnedMeshRenderer>[3];
            for (int i = 0; i < skinsLods.Length; i++)
                skinsLods[i] = new List<SkinnedMeshRenderer>();
        }
        skins ??= new List<SkinPrefabInfo>(8);
    }

    private void CheckAnimation() {
        if (Vector3.Distance(transform.position, prevPosition) > 0.02f) {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                animator.SetTrigger("Walk");
        }
        else {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                animator.SetTrigger("Idle");
        }

        prevPosition = transform.position;
    }

    #region Skins

    /// <summary>
    /// Randomize avatar's skins.
    /// </summary>
    [ContextMenu("Randomize")]
    public void Randomize() {

        ClearInstantiatedSkins();

        if (SkinManager.instance != null) {
            if (Random.Range(1, 101) > 50)
                ApplySkin(SkinManager.instance.GetRandomSkinPrefab(SkinType.Hair));
            else
                ApplySkin(SkinManager.instance.GetRandomSkinPrefab(SkinType.Hat));

            ApplySkin(SkinManager.instance.GetRandomSkinPrefab(SkinType.Pants));
            ApplySkin(SkinManager.instance.GetRandomSkinPrefab(SkinType.TShirt));

            ApplyLodGroup();
            AvatarSkinHolder.Instance.ApplyActiveSkins(GetSkinsInformation());
        }
        else {
            Debug.Log($"Can't access any skin manager.", gameObject);
        }
    }

    /// <summary>
    /// Use this method to get an information about current avatar skins. Then transfer it via network to remote avatar.
    /// </summary>
    public string GetSkinsInformation() {
        if (skins != null) {
            string[] skinNames = new string[skins.Count];
            for (int i = 0; i < skins.Count; i++)
                skinNames[i] = skins[i].name;
            return string.Join(";", skinNames);

        }
        else {
            return string.Empty;
        }
    }

    /// <summary>
    /// Set the actual avatar skins. 
    /// </summary>
    /// <param name="skinsInfo">String received via network.</param>
    public void SetSkins(string skinsInfo) {

        if (SkinManager.instance == null) {
            Debug.Log($"SkinManager is not available. Can't set skins. Check your usage.", gameObject);
            return;
        }

        ClearInstantiatedSkins();

        string[] skinNames = skinsInfo.Split(";");
        if (skinNames != null && skinNames.Length > 0) {
            foreach (string skinName in skinNames) {
                SkinPrefabInfo skinPrefab = SkinManager.instance.GetSkinPrefab(skinName);
                ApplySkin(skinPrefab);
            }

            ApplyLodGroup();
            AvatarSkinHolder.Instance.ApplyActiveSkins(string.Join(";", skinNames));
        }
        else {
            Debug.Log($"Can't set skins for avatar. Incoming string is corrupted: {skinsInfo}", gameObject);
        }
    }

    private void ClearInstantiatedSkins() {
        if (instantiatedSkinObjs != null)
            for (int i = instantiatedSkinObjs.Count - 1; i >= 0; i--)
                if (instantiatedSkinObjs[i] != null)
                    Destroy(instantiatedSkinObjs[i].gameObject);

        if (skins != null)
            skins.Clear();

        if (skinsLods != null)
            for (int i = 0; i < skinsLods.Length; i++)
                if (skinsLods[i] != null)
                    skinsLods[i].Clear();
    }

    private void ApplySkin(SkinPrefabInfo skinPrefab) {

        // Just to be sure.
        CheckLists();

        // Prepare bones-array for renders.
        Transform[] bonesForRender = new Transform[skinPrefab.boneNames.Length];
        for (int i = 0; i < bonesForRender.Length; i++)
            bonesForRender[i] = GetBoneByName(skinPrefab.boneNames[i]);

        // Instantiate LOD-objects for skin.
        for (int i = 0; i < skinPrefab.lodModels.Length; i++) {
            GameObject go = Instantiate(skinPrefab.lodModels[i]);
            go.transform.parent = rootTransform;
            instantiatedSkinObjs.Add(go);
            SkinnedMeshRenderer render = go.GetComponent<SkinnedMeshRenderer>();
            render.rootBone = GetBoneByName(skinPrefab.rootBoneName);
            render.bones = bonesForRender;
            skinsLods[i].Add(render);
            go.SetActive(true);
            skins.Add(skinPrefab);
        }
    }

    private Transform GetBoneByName(string boneName) {
        foreach (GameObject bone in bones)
            if (bone.name == boneName)
                return bone.transform;

        Debug.Log($"ERROR. Bone can't be found: {boneName}", gameObject);
        return transform;
    }

    /// <summary>
    /// Call this method after you change all avatar skins.
    /// </summary>
    private void ApplyLodGroup() {
        LOD[] lods = new LOD[3];
        for (int i = 0; i < lods.Length; i++) {
            List<SkinnedMeshRenderer> totalLodRenders = new List<SkinnedMeshRenderer>();
            totalLodRenders.AddRange(bodyRenders[i].renders);
            totalLodRenders.AddRange(skinsLods[i]);
            lods[i] = new LOD(lodTransitionLevels[i], totalLodRenders.ToArray());
        }
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }

    #endregion

    #region Emotions

    /// <summary>
    /// Change face emotion.
    /// </summary>
    /// <param name="smooth">Should it be changed smoothly over short time?</param>
    public void ChangeEmotion(EmotionType emotion, bool smooth) {
        if (smooth) {
            if (emotionChangingRoutine != null)
                StopCoroutine(emotionChangingRoutine);
            emotionChangingRoutine = StartCoroutine(ChangingEmotionSmoothly(emotion));
        }
        else
            for (byte i = 0; i < face.sharedMesh.blendShapeCount; i++)
                face.SetBlendShapeWeight(i, i == (byte)emotion ? 100f : 0f);
    }

    private IEnumerator ChangingEmotionSmoothly(EmotionType emotion) {

        // Time calculations.
        float startTime = Time.time;
        float changingDuration = emotionChangeTime;
        float completeTime = Time.time + changingDuration;

        // Values calculations.
        float[] startValues = new float[face.sharedMesh.blendShapeCount];
        float[] valueRanges = new float[startValues.Length];
        float[] finishValues = new float[startValues.Length];
        for (byte i = 0; i < startValues.Length; i++)
            startValues[i] = face.GetBlendShapeWeight(i);
        for (byte i = 0; i < valueRanges.Length; i++) {
            if (i == (byte)emotion) {
                finishValues[i] = 100f;
                valueRanges[i] = 100f - startValues[i];
            }
            else {
                finishValues[i] = 0f;
                valueRanges[i] = startValues[i];
            }
        }

        // Smoothly change emotion over time.
        while (Time.time < completeTime) {
            float timePart = (Time.time - startTime) / changingDuration;
            for (byte i = 0; i < face.sharedMesh.blendShapeCount; i++) {
                face.SetBlendShapeWeight(i, Mathf.Lerp(startValues[i], finishValues[i], timePart));
            }

            yield return null;
        }
    }

    #endregion

    #region Animations

    public void PlayAnimation(AnimationType animationType) {
        animator.SetTrigger(animationType.ToString());
        Debug.Log($"Play anim: {animationType.ToString()}", gameObject);

    }

    #endregion

    #region Enums & local structures

    [System.Serializable]
    public class BodyLodRenders {
        public SkinnedMeshRenderer[] renders;
    }

    // Don't change the order of emotions. It is based on imported model properties.
    public enum EmotionType : byte {
        Contempt = 0,
        Fear = 1,
        Disgust = 2,
        Sad = 3,
        Neutral = 4,
        Surprise = 5,
        Angry = 6,
        Happy = 7,
    }

    public enum AnimationType : byte {
        Idle,
        Walk,
        Jump,
        Hello,
        Dance
    }

    #endregion

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
