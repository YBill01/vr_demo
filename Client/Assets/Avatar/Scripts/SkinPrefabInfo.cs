using UnityEngine;

/// <summary>
/// Contains information about some avatar clothes.
/// </summary>
[System.Serializable]
public class SkinPrefabInfo  {

    public string name;
    public SkinType type;
    public GameObject[] lodModels;
    public string rootBoneName;
    public string[] boneNames;
}

public enum SkinType : byte {
    None,
    Boots,
    Hair,
    Hat,
    Pants,
    Top,
    TShirt
}
