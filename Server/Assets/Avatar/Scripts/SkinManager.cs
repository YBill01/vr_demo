using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// It is used by avatars to manage their clothes. First place game object with this script to your scene, then use this script by static 'instance' variable.
/// </summary>
/// <remarks>
/// Don't look here my friend! :)
/// This script is still under heavy development.
/// </remarks>
public class SkinManager : MonoBehaviour {

    public static SkinManager instance;

    [Tooltip("Path to skins directory. When baked skins are saved here, and then got from here at runtime.")]
    [SerializeField] private string path = "/Avatar/Skins/";

    // These arrays are temporary showed in Inspector for development purposes.
    [SerializeField] private SkinPrefabInfo[] hairSkins;
    [SerializeField] private SkinPrefabInfo[] hatSkins;
    [SerializeField] private SkinPrefabInfo[] tshirtSkins;
    [SerializeField] private SkinPrefabInfo[] pantsSkins;

    private Dictionary<string, SkinPrefabInfo> skinDictionary;

    private void Awake() {
        instance = this;

        // Prepare skins dictionary for fast runtime access.
        skinDictionary = new Dictionary<string, SkinPrefabInfo>();
        foreach (SkinPrefabInfo skin in hairSkins)
            skinDictionary.Add(skin.name, skin);
        foreach (SkinPrefabInfo skin in hatSkins)
            skinDictionary.Add(skin.name, skin);
        foreach (SkinPrefabInfo skin in tshirtSkins)
            skinDictionary.Add(skin.name, skin);
        foreach (SkinPrefabInfo skin in pantsSkins)
            skinDictionary.Add(skin.name, skin);
    }

    /// <summary>
    /// Get skin prefab if you need to apply it to your avatar.
    /// Be sure you have checked it for NULL.
    /// </summary>
    public SkinPrefabInfo GetSkinPrefab(string skinName) {
        if (skinDictionary != null) {
            skinDictionary.TryGetValue(skinName, out SkinPrefabInfo skin);
            return skin;
        }
        else {
            Debug.Log($"WARNING: you use skin dictionary before it is initialized.", gameObject);
            Debug.LogError($"WARNING: you use skin dictionary before it is initialized.", gameObject);
            Debug.LogWarning($"WARNING: you use skin dictionary before it is initialized.", gameObject);
            return null;
        }
    }

    public SkinPrefabInfo GetRandomSkinPrefab(SkinType skinType) {
        switch (skinType) {
            case SkinType.Hair:
                return hairSkins[Random.Range(0, hairSkins.Length)];
                break;
            case SkinType.Hat:
                return hatSkins[Random.Range(0, hatSkins.Length)];
                break;
            case SkinType.TShirt:
                return tshirtSkins[Random.Range(0, tshirtSkins.Length)];
                break;
            case SkinType.Pants:
                return pantsSkins[Random.Range(0, pantsSkins.Length)];
                break;
            default:
                return null;
                break;
        }
    }

    #region Tests & Debug & Development
#if UNITY_EDITOR

    [Header("Editor Only")]
    [SerializeField] private GameObject[] rawSkins;
    [SerializeField] private SkinType skinType;

    [Header("Raw skins for baking")]
    [SerializeField] private GameObject[] rawHairSkins;
    [SerializeField] private GameObject[] rawHatSkins;
    [SerializeField] private GameObject[] rawPantsSkins;
    [SerializeField] private GameObject[] rawTShirtSkins;

    /// <summary>
    /// Use this method for prepare your avatar skins.
    /// </summary>
    [ContextMenu("Bake all skins")]
    private void BakeAllSkins() {
        BakeSkinsArray(rawHairSkins, SkinType.Hair);
        BakeSkinsArray(rawHatSkins, SkinType.Hat);
        BakeSkinsArray(rawPantsSkins, SkinType.Pants);
        BakeSkinsArray(rawTShirtSkins, SkinType.TShirt);
    }

    /// <summary>
    /// This method is needed for testing purposes only.
    /// </summary>
    [ContextMenu("Bake 'raw skins' base skins")]
    private void Editor_BakeRawSkins() {
        BakeSkinsArray(rawSkins, skinType);
    }

    void BakeSkinsArray(GameObject[] rawObjectsArray, SkinType type) {

        SkinPrefabInfo[] skins = new SkinPrefabInfo[rawObjectsArray.Length / 3];
        for (int i = 0; i < skins.Length; i++) {
            GameObject[] lodObjects = new[]
            {
                rawObjectsArray[i * 3],
                rawObjectsArray[i * 3 + 1],
                rawObjectsArray[i * 3 + 2]
            };
            skins[i] = BakeSkinInfo(lodObjects, type);
        }

        switch (type) {
            case SkinType.Hair:
                hairSkins = skins;
                break;
            case SkinType.Hat:
                hatSkins = skins;
                break;
            case SkinType.TShirt:
                tshirtSkins = skins;
                break;
            case SkinType.Pants:
                pantsSkins = skins;
                break;
        }
    }

    /// <param name="lodObjs">If you have 3 levels of LODs, then there must be an array of size 3.</param>
    private SkinPrefabInfo BakeSkinInfo(GameObject[] lodObjs, SkinType type) {
        SkinnedMeshRenderer[] renders = new SkinnedMeshRenderer[lodObjs.Length];
        for (int i = 0; i < renders.Length; i++)
            renders[i] = lodObjs[i].GetComponent<SkinnedMeshRenderer>();

        SkinPrefabInfo skinPrefabInfo = new SkinPrefabInfo();
        skinPrefabInfo.name = lodObjs[0].name;
        skinPrefabInfo.type = type;
        skinPrefabInfo.rootBoneName = renders[0].rootBone.gameObject.name;
        skinPrefabInfo.boneNames = new string[renders[0].bones.Length];
        for (int i = 0; i < skinPrefabInfo.boneNames.Length; i++)
            skinPrefabInfo.boneNames[i] = renders[0].bones[i].gameObject.name;
        skinPrefabInfo.lodModels = new GameObject[lodObjs.Length];
        for (int i = 0; i < lodObjs.Length; i++) {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(lodObjs[i], Application.dataPath + path + lodObjs[i].name + ".prefab");
            skinPrefabInfo.lodModels[i] = prefab;
        }

        return skinPrefabInfo;
    }

    [ContextMenu("Editor: Set Dirty")]
    private void Editor_SetDirty() {
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
    }

#endif
    #endregion
}
