using UnityEngine;

public class AvatarSkinHolder : MonoBehaviour
{
    public static AvatarSkinHolder Instance;
    
    [SerializeField] private Avatar _demoAvatar;

    private string _skinsName;

    private void Awake() => Instance = this;
    
    public void ApplyActiveSkins(string skins) => _skinsName = skins;
    public string GetActiveSkins() => _skinsName;
    
}
