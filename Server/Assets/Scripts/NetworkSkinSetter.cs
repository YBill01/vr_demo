using UnityEngine;
using Transport.Messaging;
using Qos = UnityEngine.Networking.QosType;
using Writer = Transport.Messaging.MsgStreamWriter;

public class NetworkSkinSetter : NetworkComponent, IRuntimeSync
{
    [SerializeField] private Avatar _avatar;

    private string _skinNames => _avatar.GetSkinsInformation();

    public bool HasSyncData { get; protected set; } = false;
    public Qos SyncChannel { get => Qos.Reliable; }

    public override ushort SpawnDataLenght
    {
        get
        {
            if (string.IsNullOrEmpty(_skinNames) || string.IsNullOrWhiteSpace(_skinNames))
                return 2;
            
            return (ushort)(2 + Writer.WriteStringLenght(_skinNames));
        }
    }
    public ushort DataLenght { get => (ushort)(HasSyncData ? 2 + Writer.WriteStringLenght(_skinNames) : 0); }
    
    
    public void SetSkin(string newValue)
    {
        if (string.IsNullOrEmpty(newValue) || string.IsNullOrWhiteSpace(newValue))
            return;
        
        if (_skinNames != newValue)
        {
            _avatar.SetSkins(newValue);
            HasSyncData = true;
        }
    }

    public override void ApplySpawnData(MsgStreamReader reader, bool skipSync = false) => ApplySyncData(reader, false);

    public override void CreateSpawnData(ref Writer writer) 
    {
        writer.WriteUInt16(ComponentId);
        writer.WriteString(_skinNames);
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
        HasSyncData = true;
    }

    public void CreateSyncData(ref Writer writer, bool writeAnyway = false) 
    {
        if (writeAnyway || HasSyncData) 
        {
            writer.WriteUInt16(ComponentId);
            writer.WriteString(_skinNames);
            HasSyncData = false;
        }
    }

}