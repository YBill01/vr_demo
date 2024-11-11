using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class AvatarVisualManager : MonoBehaviour
{
    public static AvatarVisualManager Instance;
    
    [SerializeField] private SkinHolder _skinHolder;

    private List<Skin> _activeSkins;

    public List<Skin> ActiveSkin => _activeSkins;

    private void Awake()
    {
        Instance = this;
    }
    public void RandomizeAvatar()
    {
        DisabledAllSkins();
        _activeSkins = new List<Skin>
        {
            _skinHolder.GetRandomSkin(SkinTypeOld.Hat),
            _skinHolder.GetRandomSkin(SkinTypeOld.Glasses)
        };
        foreach (var skin in _activeSkins)
        {
            Debug.Log(skin.name);
        }
        SetSkinForAvatar(_activeSkins.ToArray());
    }
    
    private void SetSkinForAvatar(Skin[] skins)
    {
        foreach (var skin in skins)
            skin.SetActive(true);
    }

    private void DisabledAllSkins()
    {
        if (_activeSkins == null)
            return;
        
        foreach (var skin in _activeSkins)
        {
            skin.SetActive(false);
        }
    }

    public void ChangeActiveSkin(string name)
    {
        _activeSkins[0].name = name;
        SetSkinForAvatar(_activeSkins.ToArray());
    }
}
