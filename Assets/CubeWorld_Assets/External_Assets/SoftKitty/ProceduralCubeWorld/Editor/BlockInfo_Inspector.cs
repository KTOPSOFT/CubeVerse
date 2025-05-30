using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

namespace SoftKitty.PCW
{
    [CustomEditor(typeof(BlockInfo))]

    public class BlockInfo_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            bool _valueChanged = false;
            var script = MonoScript.FromScriptableObject(this);
            BlockInfo myTarget = (BlockInfo)target;
            if (Application.isPlaying)
            {
                GUILayout.Box("Game is running.");
                return;
            }
            else
            {
                string _thePath = AssetDatabase.GetAssetPath(script);
                _thePath = _thePath.Replace("BlockInfo_Inspector.cs", "");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                GUILayout.Box((Texture)AssetDatabase.LoadAssetAtPath(_thePath + "banner.png", typeof(Texture)), GUIStyle.none);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                Texture warningIcon = (Texture)AssetDatabase.LoadAssetAtPath(_thePath + "warning.png", typeof(Texture));
                Color _backgroundColor = GUI.backgroundColor;
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Please replace the mesh in <MeshFilter> to CubeMobile.fbx in you're targeting mobile platform.");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("____________________________________________________________________________");
                GUILayout.EndHorizontal();

                if (myTarget.gameObject.GetComponentInParent<BlockGenerator>())
                {
                    BlockGenerator _mainScript = myTarget.gameObject.GetComponentInParent<BlockGenerator>();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Type:", GUILayout.Width(100));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.backgroundColor = myTarget.type == BlockPool.BlockType.Grass ? new Color(0.6F, 1F, 0.4F) : new Color(0.6F, 1F, 0.4F)*0.3F;
                    if (GUILayout.Button("Grass",GUILayout.Width(60))) {
                        myTarget.type = BlockPool.BlockType.Grass;
                    }
                    GUI.backgroundColor = myTarget.type == BlockPool.BlockType.Mud ? new Color(1F, 0.6F, 0.2F) : new Color(1F, 0.6F, 0.2F) * 0.3F;
                    if (GUILayout.Button("Mud", GUILayout.Width(60)))
                    {
                        myTarget.type = BlockPool.BlockType.Mud;
                    }
                    GUI.backgroundColor = myTarget.type == BlockPool.BlockType.Stone ? new Color(1F, 0.8F, 0.7F) : new Color(1F, 0.8F, 0.7F) * 0.3F;
                    if (GUILayout.Button("Stone", GUILayout.Width(60)))
                    {
                        myTarget.type = BlockPool.BlockType.Stone;
                    }
                    GUI.backgroundColor = myTarget.type == BlockPool.BlockType.Lava ? new Color(1F, 0.1F, 0.1F) : new Color(1F, 0.1F, 0.1F) * 0.3F;
                    if (GUILayout.Button("Lava", GUILayout.Width(60)))
                    {
                        myTarget.type = BlockPool.BlockType.Lava;
                    }
                    GUI.backgroundColor = _backgroundColor;
                    GUI.backgroundColor = myTarget.type == BlockPool.BlockType.Custom ? new Color(1F, 1F, 1F) : new Color(1F, 1F, 1F) * 0.3F;
                    if (GUILayout.Button("Custom", GUILayout.Width(60)))
                    {
                        myTarget.type = BlockPool.BlockType.Custom;
                    }
                    GUI.backgroundColor = _backgroundColor;
                    GUILayout.EndHorizontal();

                    if (myTarget.type != BlockPool.BlockType.Custom)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label("Height Level:", GUILayout.Width(100));
                        myTarget._height = EditorGUILayout.IntSlider(myTarget._height, 0, 4);
                        GUILayout.Label("m", GUILayout.Width(15));
                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("This cube will only be used on this height level of terrain.\n" +
                            "Check [Cubes List] of <Block Pool> script.", MessageType.Info);
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }

                    if (myTarget.type == BlockPool.BlockType.Custom) 
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label("Custom Block Type:", GUILayout.Width(140));
                        myTarget.customBlockType = (BlockInfo.CustomBlockType)EditorGUILayout.EnumPopup(myTarget.customBlockType);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Select the specific custom block type. This will affect visual style and logic.", MessageType.Info);
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Top Color:", GUILayout.Width(100));
                    myTarget._topColor = EditorGUILayout.ColorField(myTarget._topColor);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox("Set the main color of the top surface of this cube.\n" +
                        "This is for LOD generation and mini map display", MessageType.Info);
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Side Color:", GUILayout.Width(100));
                    myTarget._sideColor = EditorGUILayout.ColorField(myTarget._sideColor);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox("Set the main color of the side surface of this cube.\n" +
                        "This is for LOD generation", MessageType.Info);
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Move Speed:", GUILayout.Width(100));
                    myTarget._walkableSpeed = (byte)EditorGUILayout.IntSlider( myTarget._walkableSpeed,0,100);
                    GUILayout.Label("%", GUILayout.Width(15));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox("Set the speed of player walk on this cube in percentage.", MessageType.Info);
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Friction:", GUILayout.Width(100));
                    myTarget._friction = (byte)EditorGUILayout.IntSlider(myTarget._friction, 0, 100);
                    GUILayout.Label("%", GUILayout.Width(15));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox("Set the _friction of player walk on this cube in percentage.", MessageType.Info);
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                    
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.color = Color.white;
                    GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                    GUI.color = Color.red;
                    GUILayout.Box("Please put this prop under [CubeWorldGenerator>Cubes] gameobject");
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }
            }

            if ((GUI.changed || _valueChanged) && !Application.isPlaying) myTarget.UpdatePrefab();
        }
    }
}
