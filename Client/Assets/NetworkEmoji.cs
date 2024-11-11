using Emoji;
using UnityEngine;
using Transport.Messaging;
using Qos = UnityEngine.Networking.QosType;
using Writer = Transport.Messaging.MsgStreamWriter;

public class NetworkEmoji : NetworkComponent, IRuntimeSync
{
    [SerializeField] private EmojiManager _emojiManager;
    [SerializeField] private EmojiButtonInitializer _emojiButtonInitializer;
    
    public byte EmojiType => (byte)_emojiManager.CurrentEmojiType;

    public bool HasSyncData { get; protected set; } = false;
    public Qos SyncChannel { get => Qos.Reliable; }

    public override ushort SpawnDataLenght => (ushort)(2 + EmojiType);

    public ushort DataLenght => HasSyncData ? (ushort)(2 + EmojiType) : (ushort)0;


    public void CreateEmoji(byte type)
    {
        _emojiManager.CreateEmoji((Emoji.Emoji.Type)type);
        HasSyncData = true;
    }

    public void DisableCanvas()
    {
        print("DisableCanvas");
        _emojiButtonInitializer.transform.parent.gameObject.SetActive(true);
    }

    public void ApplySyncData(MsgStreamReader reader, bool skipSync = false)
    {
        Debug.Log("Apply sync data");
        var text = reader.ReadByte();
        _emojiManager.CreateEmoji((Emoji.Emoji.Type)text);
    }

    public void CreateSyncData(ref Writer writer, bool writeAnyway = false) 
    {
        if (HasSyncData)
        {
            Debug.Log("Create sync data");
            writer.WriteUInt16(ComponentId);
            writer.WriteByte(EmojiType);
            HasSyncData = false;
        }
    }
}
