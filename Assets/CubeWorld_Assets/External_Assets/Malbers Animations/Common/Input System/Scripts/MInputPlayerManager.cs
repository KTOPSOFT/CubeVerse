
using System.Collections.Generic;
using UnityEngine;

#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.PlayerInputManager;
#endif

namespace MalbersAnimations.InputSystem
{
    [AddComponentMenu("Malbers/Input/MInput Player Manager")]
    public class MInputPlayerManager : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public PlayerInputManager Manager;

        [SerializeField] private List<LayerMask> playerLayers;

        public List<PlayerInput> players;

        public List<Transform> SpawnPoints = new();

        private int NextPoint;

        public PlayerJoinedEvent OnPlayerJoined = new();
        public PlayerJoinedEvent OnPlayerLeft = new();

        private void OnEnable()
        {
            if (Manager == null)
            {
                Manager = FindFirstObjectByType<PlayerInputManager>();
            }

            if (Manager != null)
            {
                Manager.onPlayerJoined += PlayerJoined;
                Manager.onPlayerLeft += PlayerLeft;
            }
        }


        private void OnDisable()
        {
            if (Manager != null)
            {
                Manager.onPlayerJoined -= PlayerJoined;
                Manager.onPlayerLeft -= PlayerLeft;
            }
        }


        /// <summary> Check when the Player has Joined </summary>
        public void PlayerJoined(PlayerInput player)
        {
            Debug.Log($"Player Joined {player.name}");

            players.Add(player);

            var Player = player.transform;

            //Position the Player in a spawn point
            Player.position = SpawnPoints[NextPoint].position;

            CameraLayerSettings(player);

            NextPoint = (NextPoint + 1) % SpawnPoints.Count;
            OnPlayerJoined.Invoke(player);
        }

        private void CameraLayerSettings(PlayerInput player)
        {
            player.name += $"[{player.playerIndex}]";

            //Convert Layer Mask (bit) to Integer
            int layerToAdd = (int)Mathf.Log(playerLayers[NextPoint].value, 2);


            //It can have multiple Virtual Cameras
            var VirtualCams = player.transform.root.GetComponentsInChildren<CinemachineVirtualCameraBase>();

            foreach (var v in VirtualCams)
            {
                v.gameObject.SetActive(false);

                v.name += $"[{player.playerIndex}]";
                v.gameObject.SetLayer(layerToAdd, true);
            }

            var Camera = player.FindComponent<Camera>();

            Camera.name += $"[{player.playerIndex}]";

            //add the layer
            int bitMask = Camera.cullingMask;

            //  Debug.Log(Convert.ToString(bitMask, 2).PadLeft(32, '0'));

            foreach (var mask in playerLayers)
            {
                // Debug.Log($"mask {mask.value}, layerToAdd{1 << layerToAdd}");
                if (mask == 1 << layerToAdd) continue; //Do not Remove its own Layer

                //Debug.Log($"MASK: {Convert.ToString(mask, 2).PadLeft(32, '0')}");
                bitMask &= ~mask;
                // Debug.Log($"BIT: {Convert.ToString(bitMask, 2).PadLeft(32, '0')}");
            }

            Camera.cullingMask = bitMask;
            foreach (var v in VirtualCams)
            {
                v.gameObject.SetActive(true);
            }
        }



        //Check when the player has left
        public void PlayerLeft(PlayerInput input)
        {
            OnPlayerLeft.Invoke(input);
        }
#endif
    }
}
