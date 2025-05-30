using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using SoftKitty.PCW.Demo;
using SoftKitty.PCW;
using UnityEngine;
using FishNet.Transporting;
using FishNet;

public class ServerManagementObject : NetworkBehaviour
{
    public static string world_data;
    public Dictionary<string , CustomCharacterInfo> CharacterDictionary = new Dictionary<string , CustomCharacterInfo>();

    public void SaveCharacterData(string object_ID , CustomCharacterInfo customCharacterInfo)
    {
        CharacterDictionary.Add(object_ID, customCharacterInfo);
        Debug.Log("Character Custom Data saved for "+object_ID);
    }

    public CustomCharacterInfo GetCharacterData(string object_ID)
    {
        return CharacterDictionary[object_ID];
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Your code here - runs when server starts
        RunAfterServerInitialized();
    }

    private void RunAfterServerInitialized()
    {
        Debug.Log("Server initialized — running function now.");
        // Your logic here
        BlockGenerator.instance.GenerateRandomWorld();
        world_data = BlockGenerator.instance.SaveWorld();
    }
}
