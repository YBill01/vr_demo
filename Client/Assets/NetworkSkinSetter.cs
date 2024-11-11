using UnityEngine;
using Transport.Messaging;
using Qos = UnityEngine.Networking.QosType;
using Writer = Transport.Messaging.MsgStreamWriter;

public class NetworkSkinSetter : NetworkComponent, IRuntimeSync
{
    [SerializeField] private Avatar _avatar;

    public string SkinNames => _avatar.GetSkinsInformation();

    public bool HasSyncData { get; protected set; } = false;
    public Qos SyncChannel { get => Qos.Reliable; }

    public override ushort SpawnDataLenght
    {
        get
        {
            if (string.IsNullOrEmpty(SkinNames) || string.IsNullOrWhiteSpace(SkinNames))
                return 2;
            
            return (ushort)(2 + Writer.WriteStringLenght(SkinNames));
        }
    }
    public ushort DataLenght { get => (ushort)(HasSyncData ? 2 + Writer.WriteStringLenght(SkinNames) : 0); }
    
    
    public void SetSkin(string newValue)
    {
        if (string.IsNullOrEmpty(newValue) || string.IsNullOrWhiteSpace(newValue))
            return;
        
        if (SkinNames != newValue)
        {
            _avatar.SetSkins(newValue);
            HasSyncData = true;
            
        }
    }

    public override void ApplySpawnData(MsgStreamReader reader, bool skipSync = false) => ApplySyncData(reader, false);

    public override void CreateSpawnData(ref Writer writer) 
    {
        writer.WriteUInt16(ComponentId);
        writer.WriteString(SkinNames);
    }

    public void ApplySyncData(MsgStreamReader reader, bool skipSync = false)
    {
        var text = reader.ReadString();
        
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
            return;
        
        if (!skipSync)
        {
            _avatar.SetSkins(text);
        }
    }

    public void CreateSyncData(ref Writer writer, bool writeAnyway = false) 
    {
        if (writeAnyway || HasSyncData) 
        {
            writer.WriteUInt16(ComponentId);
            writer.WriteString(SkinNames);
            HasSyncData = false;
        }
    }

}