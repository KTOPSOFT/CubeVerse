using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SoftKitty.PCW
{
    [CustomEditor(typeof(BlockPool))]
    public class BlockPool_Inspector : Editor
    {
        bool _cubeExpanded = false;
        bool [] _cubeLevelExpand = new bool[5] { false,false,false,false,false};
        bool _customCubeExpand = false;
        bool _propExpanded = false;
        bool[] _propStyleExpand = new bool[10] { false, false, false, false, false, false, false, false, false, false };
        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            bool _valueChanged = false;
            Color _activeColor = new Color(0.3F, 0.6F, 1F);
            Color _disableColor = new Color(0F, 0.3F, 0.8F);
            Color[] _typeColor = new Color[4] {
              new Color(0.6F,1F,0.4F),//grass
              new Color(1F,0.6F,0.2F),//mud
              new Color(0.9F,0.8F,0.7F),//stone
              new Color(1F,0.1F,0.1F)//Lava
            };

            var script = MonoScript.FromScriptableObject(this);
            BlockPool myTarget = (BlockPool)target;
            bool _needRefresh = false;
            Texture warningIcon=null;
            if (Application.isPlaying)
            {
                GUILayout.Box("Game is running.");
                return;
            }
            else
            {
                string _thePath = AssetDatabase.GetAssetPath(script);
                _thePath = _thePath.Replace("BlockPool_Inspector.cs", "");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                GUILayout.Box((Texture)AssetDatabase.LoadAssetAtPath(_thePath + "banner.png", typeof(Texture)), GUIStyle.none);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                _needRefresh = (myTarget.transform.childCount != myTarget.ChildCount);
                warningIcon = (Texture)AssetDatabase.LoadAssetAtPath(_thePath + "warning.png", typeof(Texture));
            }
           
            Color _backgroundColor = GUI.backgroundColor;


            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = _needRefresh?Color.red: Color.green;
            if (_needRefresh && warningIcon != null)
            {
                GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
            }
            else
            {
                GUILayout.Space(20);
            }
            if (GUILayout.Button("Refresh Assets",GUILayout.Width(150)))
            {
                myTarget.ChildCount = myTarget.transform.childCount;
                myTarget.Initialize();
                _valueChanged = true;
            }
            GUI.backgroundColor = _backgroundColor;
           
            GUILayout.Label("Click this after you modified cubes/props.");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_cubeExpanded?"[-]":"[+]", GUILayout.Width(20));
            GUI.backgroundColor = _cubeExpanded ? _activeColor : _disableColor ;
            if (GUILayout.Button("Terrain Cubes List"))
            {
                _cubeExpanded = !_cubeExpanded;
            }
            GUI.backgroundColor = _backgroundColor;
            EditorGUILayout.EndHorizontal();

            if (_cubeExpanded) {
                bool refreshError = false;
                if (myTarget.mBlockPrefabs.Length<5) {
                    refreshError = true;
                } else {
                   
                    for (int _level = 0; _level < 5; _level++)
                    {
                        if(myTarget.mBlockPrefabs[_level]._sets.Length<4) refreshError = true;
                    }

                    if (!refreshError)
                    {
                        for (int _level = 0; _level < 5; _level++)
                        {
                            bool _error = false;
                            for (int _type = 0; _type < 4; _type++)
                            {
                                if (myTarget.mBlockPrefabs[_level]._sets[_type]._models.Count == 0) _error = true;
                            }
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20);
                            GUILayout.Label(_cubeLevelExpand[_level] ? "[-]" : "[+]", GUILayout.Width(20));
                            GUI.backgroundColor = _cubeLevelExpand[_level] ? _activeColor : _disableColor;
                            if (_error) GUI.backgroundColor = _cubeLevelExpand[_level] ? new Color(1F, 0.3F, 0.3F) : Color.red;
                            if (GUILayout.Button("Cubes [Height Level " + _level.ToString() + "]"))
                            {
                                _cubeLevelExpand[_level] = !_cubeLevelExpand[_level];
                            }
                            GUI.backgroundColor = _backgroundColor;
                            if (_error) GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                            EditorGUILayout.EndHorizontal();

                            if (_cubeLevelExpand[_level])
                            {
                                for (int _type = 0; _type < 4; _type++)
                                {
                                    GUI.color = _typeColor[_type];
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(50);
                                    GUILayout.Box("[" + ((BlockPool.BlockType)_type).ToString() + " Cubes]:");

                                    if (myTarget.mBlockPrefabs[_level]._sets[_type]._models.Count > 0)
                                    {
                                        EditorGUILayout.EndHorizontal();
                                        for (int i = 0; i < myTarget.mBlockPrefabs[_level]._sets[_type]._models.Count; i++)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Space(70);
                                            if (GUILayout.Button(myTarget.mBlockPrefabs[_level]._sets[_type]._models[i].name))
                                            {
                                                Selection.activeObject = myTarget.mBlockPrefabs[_level]._sets[_type]._models[i];
                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                    else
                                    {
                                        GUI.color = Color.white;
                                        GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.Space(70);
                                        GUI.color = Color.red;
                                        GUILayout.Box("You have to add at least 1 cube with Height(" + _level.ToString() + ") Type(" + ((BlockPool.BlockType)_type).ToString() + ").");
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                            }
                            GUI.color = Color.white;

                        }

                        GUI.backgroundColor = Color.yellow;
                        EditorGUILayout.HelpBox("A best practice is to have 2~3 slightly different cubes under each terrain type of each [Height Level]", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                    }
                }
                if (refreshError)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = Color.red;
                    GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                    GUILayout.Box("Please press the [Refresh Assets] button on the top.");
                    GUI.backgroundColor = _backgroundColor;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_customCubeExpand ? "[-]" : "[+]", GUILayout.Width(20));
            GUI.backgroundColor = _customCubeExpand ? _activeColor : _disableColor;
            if (GUILayout.Button("Custom Cubes List"))
            {
                _customCubeExpand = !_customCubeExpand;
            }
            GUI.backgroundColor = _backgroundColor;
            EditorGUILayout.EndHorizontal();

            if (_customCubeExpand)
            {

                if (myTarget.mCustomCubePrefabs.Count > 0)
                {
                    for (int i = 0; i < myTarget.mCustomCubePrefabs.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUI.backgroundColor = new Color(0.2F, 0.7F, 0.4F);
                        if (GUILayout.Button("[UID: "+ myTarget.mCustomCubePrefabs[i].GetComponent<BlockInfo>()._customUid.ToString() + "] " + myTarget.mCustomCubePrefabs[i].name ))
                        {
                            Selection.activeGameObject = myTarget.mCustomCubePrefabs[i];
                        }
                        GUI.backgroundColor = _backgroundColor;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUILayout.Box("Empty", GUILayout.Width(300));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_propExpanded ? "[-]" : "[+]", GUILayout.Width(20));
            GUI.backgroundColor = _propExpanded ? _activeColor : _disableColor;
            if (GUILayout.Button("Props List"))
            {
                _propExpanded = !_propExpanded;
            }
            GUI.backgroundColor = _backgroundColor;
            EditorGUILayout.EndHorizontal();

            if (_propExpanded)
            {
                if (myTarget.mRockProps.Count >= myTarget.mTotalStyle
                    && myTarget.mSandProps.Count >= myTarget.mTotalStyle
                    && myTarget.mSnowProps.Count >= myTarget.mTotalStyle
                    && myTarget.mMudProps.Count >= myTarget.mTotalStyle
                    && myTarget.mWaterProps.Count >= myTarget.mTotalStyle
                    )
                {
                    for (int _style = 0; _style < myTarget.mTotalStyle; _style++)
                    {
                        bool _error = false;
                        if (myTarget.mRockProps[_style]._models.Count
                            + myTarget.mSandProps[_style]._models.Count
                             + myTarget.mSnowProps[_style]._models.Count
                              + myTarget.mMudProps[_style]._models.Count
                               + myTarget.mWaterProps[_style]._models.Count
                            <= 0) _error = true;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label(_propStyleExpand[_style] ? "[-]" : "[+]", GUILayout.Width(20));
                        GUI.backgroundColor = _propStyleExpand[_style] ? _activeColor : _disableColor;
                        if (_error) GUI.backgroundColor = _propStyleExpand[_style] ? new Color(1F, 0.3F, 0.3F) : Color.red;
                        if (GUILayout.Button("Props " + (_style == 0 ? "[All style]" : "[Style " + _style.ToString() + "]")))
                        {
                            _propStyleExpand[_style] = !_propStyleExpand[_style];
                        }
                        GUI.backgroundColor = _backgroundColor;
                        if (_error) GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();

                        if (_propStyleExpand[_style])
                        {
                            GUI.color = new Color(1F, 0.6F, 0.2F);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Box("[Mud Props]:");
                            if (myTarget.mMudProps[_style]._models.Count > 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                for (int i = 0; i < myTarget.mMudProps[_style]._models.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    if (GUILayout.Button(myTarget.mMudProps[_style]._models[i].name))
                                    {
                                        Selection.activeObject = myTarget.mMudProps[_style]._models[i];
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                GUI.color = Color.white;
                                GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.color = Color.red;
                                GUILayout.Box("No props matchs this terrain type yet.");
                                EditorGUILayout.EndHorizontal();
                            }

                            GUI.color = new Color(1F, 0.6F, 0.2F);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Box("[Rock Props]:");
                            if (myTarget.mRockProps[_style]._models.Count > 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                for (int i = 0; i < myTarget.mRockProps[_style]._models.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    if (GUILayout.Button(myTarget.mRockProps[_style]._models[i].name))
                                    {
                                        Selection.activeObject = myTarget.mRockProps[_style]._models[i];
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                GUI.color = Color.white;
                                GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.color = Color.red;
                                GUILayout.Box("No props matchs this terrain type yet.");
                                EditorGUILayout.EndHorizontal();
                            }

                            GUI.color = new Color(1F, 0.6F, 0.2F);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Box("[Sand Props]:");
                            if (myTarget.mSandProps[_style]._models.Count > 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                for (int i = 0; i < myTarget.mSandProps[_style]._models.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    if (GUILayout.Button(myTarget.mSandProps[_style]._models[i].name))
                                    {
                                        Selection.activeObject = myTarget.mSandProps[_style]._models[i];
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                GUI.color = Color.white;
                                GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.color = Color.red;
                                GUILayout.Box("No props matchs this terrain type yet.");
                                EditorGUILayout.EndHorizontal();
                            }

                            GUI.color = new Color(1F, 0.6F, 0.2F);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Box("[Snow Props]:");
                            if (myTarget.mSnowProps[_style]._models.Count > 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                for (int i = 0; i < myTarget.mSnowProps[_style]._models.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    if (GUILayout.Button(myTarget.mSnowProps[_style]._models[i].name))
                                    {
                                        Selection.activeObject = myTarget.mSnowProps[_style]._models[i];
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                GUI.color = Color.white;
                                GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.color = Color.red;
                                GUILayout.Box("No props matchs this terrain type yet.");
                                EditorGUILayout.EndHorizontal();
                            }

                            GUI.color = new Color(1F, 0.6F, 0.2F);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Box("[Water Props]:");
                            if (myTarget.mWaterProps[_style]._models.Count > 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                for (int i = 0; i < myTarget.mWaterProps[_style]._models.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    if (GUILayout.Button(myTarget.mWaterProps[_style]._models[i].name))
                                    {
                                        Selection.activeObject = myTarget.mWaterProps[_style]._models[i];
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                GUI.color = Color.white;
                                GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.color = Color.red;
                                GUILayout.Box("No props matchs this terrain type yet.");
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        GUI.color = Color.white;

                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = Color.red;
                    GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                    GUILayout.Box("Please press the [Refresh Assets] button on the top.");
                    GUI.backgroundColor = _backgroundColor;
                    EditorGUILayout.EndHorizontal();
                }
            }


            if ((_valueChanged || GUI.changed) && !Application.isPlaying) myTarget.UpdatePrefab();
        }
    }
}
