using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientTypeHolder : MonoBehaviour
{
    [SerializeField] private ClientType _clientType = ClientType.Player;
    
    public static ClientTypeHolder Instance;

    public ClientType ClientType => _clientType;
    private void Awake()
    {
        Instance = this;
    }
}

public enum ClientType
{
    Player,
    Bot
}
