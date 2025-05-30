using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SoftKitty.PCW
{
    [CustomEditor(typeof(BlockGenerator))]
    public class BlockGenerator_Inspector : Editor
    {
        
        bool _assetExpand=false;
        bool _settingExpand = false;
        bool _styleExpand = false;
        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            bool _valueChanged = false;
            Color _activeColor = new Color(0.3F, 0.6F, 1F);
            Color _disableColor = new Color(0F, 0.3F, 0.8F);
            Color _backgroundColor = GUI.backgroundColor;
            var script = MonoScript.FromScriptableObject(this);
            BlockGenerator myTarget = (BlockGenerator)target;
            if (Application.isPlaying)
            {
                GUILayout.Box("Game is running.");
                return;
            }
            else
            {
                string _thePath = AssetDatabase.GetAssetPath(script);
                _thePath = _thePath.Replace("BlockGenerator_Inspector.cs", "");
                Texture warningIcon = (Texture)AssetDatabase.LoadAssetAtPath(_thePath + "warning.png", typeof(Texture));

                GUILayout.BeginHorizontal();
                GUI.color = (myTarget.Player == null ? Color.red : Color.white);
                GUILayout.Label("Player:",GUILayout.Width(60));
                myTarget.Player = (GameObject)EditorGUILayout.ObjectField(myTarget.Player,typeof(GameObject), true);
                GUILayout.EndHorizontal();
                if (myTarget.Player == null)
                {
                    GUILayout.BeginHorizontal();
                    GUI.color = Color.white;
                    GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                    GUI.color = Color.red;
                    GUILayout.Box("Please assign your player gameobject.");
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("System will only display detailed cubes around the player.", MessageType.Info);
                GUI.color = Color.white;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Sight:", GUILayout.Width(60));
                myTarget.Sight = EditorGUILayout.IntSlider(myTarget.Sight, 5, 50);
                GUILayout.EndHorizontal();
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("How far the detailed cubes will be displayed.(meters)", MessageType.Info);
                GUI.color = Color.white;

                GUILayout.BeginHorizontal();
                GUILayout.Label("How many cubes will be loaded per frame:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                myTarget.LoadBlockPerFrame = EditorGUILayout.IntSlider(myTarget.LoadBlockPerFrame, 10, 100);
                GUILayout.EndHorizontal();
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("Higher value makes performance worse,Lower value makes load process more noticeable." +
                    "\nIf your player moves super fast, it's recommend to use higher value.", MessageType.Info);
                GUI.color = Color.white;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Moving speed in the water:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                myTarget.MoveSpeedInWater = EditorGUILayout.IntSlider(Mathf.CeilToInt(myTarget.MoveSpeedInWater*100), 10, 100)*0.01F;
                GUILayout.Label("%",GUILayout.Width(30));
                GUILayout.EndHorizontal();
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("When player standing in the water, if you call (BlockGenerator.instance.GetRunningSpeedByPosition)" +
                    "will return the speed with above multiplier to you.", MessageType.Info);
                GUI.color = Color.white;



                GUILayout.BeginHorizontal();
                GUILayout.Label(_settingExpand ? "[-]" : "[+]", GUILayout.Width(20));
                GUI.backgroundColor = _settingExpand ? _activeColor : _disableColor;
                if (myTarget.mSettings.Count  == 0) GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Terrain Styles"))
                {
                    _settingExpand = !_settingExpand;
                }
                if (myTarget.mSettings.Count == 0) GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                GUILayout.EndHorizontal();
                if (_settingExpand)
                {
                    if (myTarget.mSettings.Count == 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUI.color = Color.white;
                        GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                        GUI.color = Color.red;
                        GUILayout.Box("Please add at least one style.");
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }

                    for (int i = 0; i < myTarget.mSettings.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUI.backgroundColor = myTarget.mSettings[i].Expand ? _activeColor : _disableColor;
                        if (GUILayout.Button("Style " + (i + 1).ToString() + "[" + myTarget.mSettings[i].Name + "]"))
                        {
                            myTarget.mSettings[i].Expand = !myTarget.mSettings[i].Expand;
                        }
                        GUI.backgroundColor = new Color(1F, 0.5F, 0.5F);
                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            myTarget.mSettings.RemoveAt(i);
                            _valueChanged = true;
                        }
                        GUI.backgroundColor = _backgroundColor;
                        GUILayout.EndHorizontal();

                        if (i < myTarget.mSettings.Count)
                        {
                            if (myTarget.mSettings[i].Expand)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.color = Color.yellow;
                                GUILayout.Box("Style ID: " + (i + 1).ToString());
                                myTarget.mSettings[i].StyleID = i + 1;
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                EditorGUILayout.HelpBox("When you setup props, you can specify the [Style ID] to make them only spawn with this terrain style.", MessageType.Info);
                                GUILayout.EndHorizontal();
                                GUI.color = Color.white;

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUILayout.Box("Style Name: ");
                                myTarget.mSettings[i].Name = GUILayout.TextField(myTarget.mSettings[i].Name);
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUILayout.Label("Mountain Height:", GUILayout.Width(130));
                                myTarget.mSettings[i].MoutainHeight = EditorGUILayout.IntSlider(myTarget.mSettings[i].MoutainHeight, 2, 8);
                                GUILayout.Label("Meters", GUILayout.Width(60));
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUI.color = Color.yellow;
                                GUILayout.Space(70);
                                EditorGUILayout.HelpBox("Set the maxium height of the mountains in meters", MessageType.Info);
                                GUILayout.EndHorizontal();
                                GUI.color = Color.white;

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                myTarget.mSettings[i].WithGrass = EditorGUILayout.Toggle(myTarget.mSettings[i].WithGrass, GUILayout.Width(30));
                                GUILayout.Label("Grass Cubes", GUILayout.Width(100));
                                GUILayout.Space(70);
                                myTarget.mSettings[i].WithLava = EditorGUILayout.Toggle(myTarget.mSettings[i].WithLava, GUILayout.Width(30));
                                GUILayout.Label("Lava Cubes", GUILayout.Width(100));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                myTarget.mSettings[i].WithSnow = EditorGUILayout.Toggle(myTarget.mSettings[i].WithSnow, GUILayout.Width(30));
                                GUILayout.Label("Snow Cubes", GUILayout.Width(100));
                                GUILayout.Space(70);
                                myTarget.mSettings[i].WithWater = EditorGUILayout.Toggle(myTarget.mSettings[i].WithWater, GUILayout.Width(30));
                                GUILayout.Label("Lakes & Rivers", GUILayout.Width(200));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUI.color = Color.yellow;
                                GUILayout.Space(70);
                                EditorGUILayout.HelpBox("Toggle certain terrain features for this style.", MessageType.Info);
                                GUILayout.EndHorizontal();
                                GUI.color = Color.white;
                            }
                        }
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    GUI.backgroundColor = new Color(0.5F, 1F, 0.2F);
                    if (GUILayout.Button("Add New Terrain Style"))
                    {
                        CubeWorldSettings _newSetting = new CubeWorldSettings();
                        _newSetting.StyleID = myTarget.mSettings.Count + 1;
                        _newSetting.Name = "Unnamed " + _newSetting.StyleID.ToString();
                        _newSetting.Expand = false;
                        _newSetting.MoutainHeight = 4;
                        _newSetting.WithSnow = true;
                        _newSetting.WithGrass = true;
                        _newSetting.WithLava = true;
                        _newSetting.WithWater = true;
                        myTarget.mSettings.Add(_newSetting);
                        _valueChanged = true;
                    }
                    GUI.backgroundColor = _backgroundColor;
                    GUILayout.EndHorizontal();

                }


                GUILayout.BeginHorizontal();
                GUILayout.Label(_styleExpand ? "[-]" : "[+]", GUILayout.Width(20));
                GUI.backgroundColor = _styleExpand ? _activeColor : _disableColor;
                if (myTarget.mUseWorldMap && myTarget.mWorldMap==null) GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Style Mapping"))
                {
                    _styleExpand = !_styleExpand;
                }
                if (myTarget.mUseWorldMap && myTarget.mWorldMap == null) GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                GUILayout.EndHorizontal();
                if (_styleExpand)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.backgroundColor = myTarget.mUseWorldMap ?  new Color(0.8F, 0.5F, 0F): new Color(1.5F, 0.7F, 0.3F);
                    if (GUILayout.Button("Random Style"))
                    {
                        myTarget.mUseWorldMap = false;
                        _valueChanged = true;
                    }
                    GUI.backgroundColor = myTarget.mUseWorldMap ? new Color(1.5F, 0.7F, 0.3F) : new Color(0.8F, 0.5F, 0F);
                    if (GUILayout.Button("Use World Map"))
                    {
                        myTarget.mUseWorldMap = true;
                        _valueChanged = true;
                    }
                    GUI.backgroundColor = _backgroundColor;
                    GUILayout.EndHorizontal();

                    if (myTarget.mUseWorldMap)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUI.color = (myTarget.mWorldMap == null ? Color.red : Color.white);
                        GUILayout.Label("World Map:", GUILayout.Width(100));
                        myTarget.mWorldMap = (Texture2D)EditorGUILayout.ObjectField(myTarget.mWorldMap, typeof(Texture2D), false);
                        GUILayout.EndHorizontal();
                        if (myTarget.mWorldMap == null)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            GUI.color = Color.red;
                            EditorGUILayout.HelpBox("Please assign a world map texture in tga format to mapping the style.\n" +
                                "red channel=Style#1, green channel=Style#2,\n" +
                                "blue channel=Style#3, alpha channel=Style#4" +
                                "The brighter the channel is, the more chance to spawn the terrain with the matching style.", MessageType.Warning);
                            GUI.color = Color.white;
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            string path = AssetDatabase.GetAssetPath(myTarget.mWorldMap);
                            TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(path);
                            if (!A.isReadable || A.mipmapEnabled)
                            {
                                A.isReadable = true;
                                A.mipmapEnabled = false;
                                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                                _valueChanged = true;
                            }
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("red channel=Style#1, green channel=Style#2,\n" +
                            "blue channel=Style#3, alpha channel=Style#4" +
                            "The brighter the channel is, the more chance to spawn the terrain with the matching style.", MessageType.Info);
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUILayout.Label("World Map Scale:");
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        myTarget.mWorldMapScale = EditorGUILayout.IntSlider(myTarget.mWorldMapScale,1,200);
                        GUILayout.Label("meter/pixel", GUILayout.Width(100));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("The world map texture will be map the style ID to the terrain with the above size,\n" +
                            "Terrain beyond this size will extend the style ID defined by the border", MessageType.Info);
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUI.color = Color.yellow;
                        GUILayout.Space(30);
                        EditorGUILayout.HelpBox("When the terrain is generated, it will pick a random style setting.", MessageType.Info);
                        GUILayout.EndHorizontal();
                        GUI.color = Color.white;
                    }
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(_assetExpand ? "[-]" : "[+]", GUILayout.Width(20));
                GUI.backgroundColor = _assetExpand ? _activeColor : _disableColor;
                if (myTarget.mTerrainMask == null || myTarget.mTerrainMask2 == null) GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Procedural Seed Maps"))
                {
                    _assetExpand = !_assetExpand;
                }
                if (myTarget.mTerrainMask == null || myTarget.mTerrainMask2 == null) GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                GUI.backgroundColor = _backgroundColor;
                GUILayout.EndHorizontal();

                if (_assetExpand) {
                    bool _noMap = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.color = (myTarget.mTerrainMask == null ? Color.red : Color.white);
                    GUILayout.Label("Terrain Seed Map #1:", GUILayout.Width(180));
                    myTarget.mTerrainMask = (Texture2D)EditorGUILayout.ObjectField(myTarget.mTerrainMask, typeof(Texture2D), false);
                    if (myTarget.mTerrainMask == null) _noMap = true;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.color = (myTarget.mTerrainMask2 == null ? Color.red : Color.white);
                    GUILayout.Label("Terrain Seed Map #2:", GUILayout.Width(180));
                    myTarget.mTerrainMask2 = (Texture2D)EditorGUILayout.ObjectField(myTarget.mTerrainMask2, typeof(Texture2D), false);
                    if (myTarget.mTerrainMask2 == null) _noMap = true;
                    GUILayout.EndHorizontal();

                    if (_noMap)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUI.color = Color.white;
                        GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                        GUI.color = Color.red;
                        GUILayout.Box("Please assign seed maps.\nClick the below button if you want to use the default one.");
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUI.backgroundColor = new Color(0.5F,1F,0.2F);
                        if (GUILayout.Button("Use Default Maps")) {
                            myTarget.mTerrainMask= (Texture2D)AssetDatabase.LoadAssetAtPath(_thePath.Replace("Editor", "Textures") + "TerrainSeed.tga", typeof(Texture2D));
                            myTarget.mTerrainMask2 = (Texture2D)AssetDatabase.LoadAssetAtPath(_thePath.Replace("Editor", "Textures") + "TerrainSeed2.tga", typeof(Texture2D));
                        }
                        GUI.backgroundColor = _backgroundColor;
                        GUILayout.EndHorizontal();
                    }
                }

            }
           
            if ((_valueChanged || GUI.changed) && !Application.isPlaying) myTarget.UpdatePrefab();
        }
    }
}
