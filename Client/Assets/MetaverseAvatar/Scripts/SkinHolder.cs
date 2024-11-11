using UnityEngine;

public class SkinHolder : MonoBehaviour
{
    [SerializeField] private Skin[] hairSkins;
    [SerializeField] private Skin[] glassesSkins;

    public Skin GetRandomSkin(SkinTypeOld skinType)
    {
        switch (skinType)
        {
            case SkinTypeOld.Hat:
                Debug.Log("Hat");
                return hairSkins[Random.Range(0, hairSkins.Length)];
            case SkinTypeOld.Glasses:
                Debug.Log("Glasses");
                return glassesSkins[Random.Range(0, glassesSkins.Length)];
            default:
                return null;
        }
    }

    public Skin GetSkinToName(string name)
    {
        foreach (var hairSkin in hairSkins)
            if (hairSkin.name == name)
                return hairSkin;
        
        foreach (var glasses in glassesSkins)
            if (glasses.name == name)
                return glasses;
        return null;
    }
}

public enum SkinTypeOld
{
    Hat, 
    Glasses
}