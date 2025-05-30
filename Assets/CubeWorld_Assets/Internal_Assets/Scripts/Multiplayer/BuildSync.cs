using FishNet.Object;
using SoftKitty.PCW;
using SoftKitty.PCW.Demo;
using UnityEngine;

public class BuildSync : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = true)]
    public void SendBuildData(BlockInstance blockInstance, bool _load, Vector3 _pos, Vector3 touch_pos, Vector3 cam_pos, bool _modify_data = true)
    {
        Debug.Log("ServerRPC Build Data");
        ResponseBuildRequest(blockInstance, _load, _pos, touch_pos, cam_pos, _modify_data);
        GameObject.Find("PlayerCamera").GetComponent<BuildControl>().ResponseBuildData(blockInstance, _load, _pos, touch_pos, cam_pos, _modify_data);
        ServerManagementObject.world_data = GameObject.Find("CubeWorldGenerator").GetComponent<BlockGenerator>().SaveWorld();
    }

    [ObserversRpc(ExcludeOwner = false, BufferLast = true)]
    public void ResponseBuildRequest(BlockInstance blockInstance, bool _load, Vector3 _pos, Vector3 touch_pos, Vector3 cam_pos, bool _modify_data = true)
    {
        //if(IsOwner)
            SoftKitty.PCW.Demo.BuildControl.Instance.ResponseBuildData(blockInstance, _load, _pos, touch_pos, cam_pos, _modify_data);
    }

    [ServerRpc(RequireOwnership = true)]
    public void SendDeleteData(byte data, bool status, Vector3 touch_pos, Vector3 cam_pos)
    {
        Debug.Log("ServerRPC Delete Data");
        ResponseDeleteRequest(data, status, touch_pos, cam_pos);
        GameObject.Find("PlayerCamera").GetComponent<BuildControl>().ResponseDeleteData(data, status, touch_pos, cam_pos);
        ServerManagementObject.world_data = GameObject.Find("CubeWorldGenerator").GetComponent<BlockGenerator>().SaveWorld();
    }

    [ObserversRpc(ExcludeOwner = false, BufferLast = true)]
    public void ResponseDeleteRequest(byte data, bool status, Vector3 touch_pos, Vector3 cam_pos)
    {
        //if(IsOwner)
            SoftKitty.PCW.Demo.BuildControl.Instance.ResponseDeleteData(data, status, touch_pos, cam_pos);
    }
}
