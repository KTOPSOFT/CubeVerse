using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

namespace SoftKitty.PCW
{
    [CustomEditor(typeof(PropInfo))]

    public class PropInfo_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            bool _valueChanged = false;
            var script = MonoScript.FromScriptableObject(this);
            PropInfo myTarget = (PropInfo)target;
            if (Application.isPlaying)
            {
                GUILayout.Box("Game is running.");
                return;
            }
            else
            {
               
                string _thePath = AssetDatabase.GetAssetPath(script);
                _thePath = _thePath.Replace("PropInfo_Inspector.cs", "");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                GUILayout.Box((Texture)AssetDatabase.LoadAssetAtPath(_thePath + "banner.png", typeof(Texture)), GUIStyle.none);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                Texture warningIcon = (Texture)AssetDatabase.LoadAssetAtPath(_thePath + "warning.png", typeof(Texture));
                Color _backgroundColor = GUI.backgroundColor;

                if (myTarget.gameObject.GetComponentInParent<BlockGenerator>())
                {
                    BlockGenerator _mainScript = myTarget.gameObject.GetComponentInParent<BlockGenerator>();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUI.color = Color.green;
                    GUILayout.Box("Style ID");
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                    if (_mainScript.mSettings.Count == 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUI.color = Color.white;
                        GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                        GUI.color = Color.red;
                        GUILayout.Box("Please add at least one [Terrain Style] in the <BlockGenerator> script.");
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUI.backgroundColor = myTarget.StyleID == 0 ? new Color(0.5F, 1F, 0.2F) : new Color(0.2F, 0.25F, 0.15F);
                        if (GUILayout.Button("Any Style", GUILayout.Width(180)))
                        {
                            myTarget.StyleID = 0;
                        }
                        for (int i = 0; i < _mainScript.mSettings.Count; i++)
                        {
                            GUI.backgroundColor = myTarget.StyleID == i + 1 ? new Color(0.5F, 1F, 0.2F) : new Color(0.2F, 0.25F, 0.15F);
                            if (GUILayout.Button("(" + (i + 1).ToString() + ") " + _mainScript.mSettings[i].Name, GUILayout.Width(180)))
                            {
                                myTarget.StyleID = i + 1;
                            }
                            if (i % 2 == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(40);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUI.backgroundColor = _backgroundColor;

                        
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.color = Color.white;
                    GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(30));
                    GUI.color = Color.red;
                    GUILayout.Box("Please put this prop under [CubeWorldGenerator>Props] gameobject");
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("This prop will only spawn on the terrain with matching Terrain Style", MessageType.Info);
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Box("Prop Size: ");
                if (myTarget.GroundRightBottomCorner == Vector2.one && myTarget.GroundLeftTopCorner == Vector2.one)
                {
                    GUI.color = Color.red;
                    GUILayout.Box("null");
                }
                else
                {
                    GUILayout.Box(
                        Mathf.Max(1,myTarget.GroundRightBottomCorner.x - myTarget.GroundLeftTopCorner.x).ToString() + " x "
                        + Mathf.Max(1, myTarget.GroundRightBottomCorner.y - myTarget.GroundLeftTopCorner.y).ToString()
                        + " meters");
                }
                GUI.color = Color.green;
                if (GUILayout.Button("Calculate Size")) {
                    myTarget.GroundLeftTopCorner = Vector2.zero;
                    myTarget.GroundRightBottomCorner = Vector2.zero;
                    Renderer[] mRenderers = myTarget.gameObject.GetComponentsInChildren<Renderer>(true);

                    if (myTarget.transform.childCount > 0)
                    {
                        Dictionary<int, Transform> _transDic = new Dictionary<int, Transform>();
                        foreach (Renderer obj in mRenderers)
                        {
                            _transDic.Add(obj.transform.GetInstanceID(), obj.transform.parent);
                            obj.transform.SetParent(null);
                        }
                        myTarget.transform.eulerAngles = Vector3.zero;
                        myTarget.transform.localScale = Vector3.one;
                        foreach (Renderer obj in mRenderers)
                        {
                            if (_transDic.ContainsKey(obj.transform.GetInstanceID()))
                            {
                                obj.transform.SetParent(_transDic[obj.transform.GetInstanceID()]);
                            }
                            else
                            {
                                obj.transform.SetParent(myTarget.transform);
                            }
                        }
                        _transDic.Clear();
                    }

                    foreach (Renderer obj in mRenderers) {
                        float _x = myTarget.transform.InverseTransformPoint(obj.bounds.min).x;
                        if (_x < myTarget.GroundLeftTopCorner.x) myTarget.GroundLeftTopCorner.x = _x;
                        float _z = myTarget.transform.InverseTransformPoint(obj.bounds.min).z;
                        if (_z < myTarget.GroundLeftTopCorner.y) myTarget.GroundLeftTopCorner.y = _z;

                        float _x2 = myTarget.transform.InverseTransformPoint(obj.bounds.max).x;
                        if (_x2 > myTarget.GroundRightBottomCorner.x) myTarget.GroundRightBottomCorner.x = _x2;
                        float _z2 = myTarget.transform.InverseTransformPoint(obj.bounds.max).z;
                        if (_z2 > myTarget.GroundRightBottomCorner.y) myTarget.GroundRightBottomCorner.y = _z2;

                    }
                    myTarget.GroundLeftTopCorner.x = Mathf.RoundToInt(myTarget.GroundLeftTopCorner.x);
                    myTarget.GroundLeftTopCorner.y = Mathf.RoundToInt(myTarget.GroundLeftTopCorner.y);
                    myTarget.GroundRightBottomCorner.x = Mathf.RoundToInt(myTarget.GroundRightBottomCorner.x);
                    myTarget.GroundRightBottomCorner.y = Mathf.RoundToInt(myTarget.GroundRightBottomCorner.y);
                    _valueChanged = true;
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
                if (myTarget.GroundRightBottomCorner == Vector2.one && myTarget.GroundLeftTopCorner == Vector2.one)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.color = Color.white;
                    GUILayout.Box(warningIcon, GUIStyle.none, GUILayout.Width(20));
                    GUI.color = Color.red;
                    GUILayout.Box("Please press [Calculate Size] to calculate prop size.");
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("Prop Size is for keeping props not to overlay on each others, also used for AI path finding.\n" +
                    "Please press [Calculate Size] button Every time you modified your model.", MessageType.Info);
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Box("Spawn chance: ", GUILayout.Width(100));
                myTarget.RandomChanceMulti = EditorGUILayout.IntSlider(myTarget.RandomChanceMulti,1,5);
                GUILayout.Box("x", GUILayout.Width(15));
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                string _startHeight = myTarget.HeightRange.x == -1F ? "Water level" : myTarget.HeightRange.x.ToString();
                string _endHeight = myTarget.HeightRange.y == -1F ? "Water level" : myTarget.HeightRange.y.ToString()+ " meters above water level";
                GUILayout.Box("Height Level Range: "+ _startHeight + "~"+ _endHeight);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                EditorGUILayout.MinMaxSlider(ref  myTarget.HeightRange.x, ref myTarget.HeightRange.y, -1, 8);
                myTarget.HeightRange.x = Mathf.RoundToInt(myTarget.HeightRange.x);
                myTarget.HeightRange.y = Mathf.RoundToInt(myTarget.HeightRange.y);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("This prop will only spawn on the terrain within this height range.", MessageType.Info);
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Box("Only spawn on the following terrain:");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(70);
                myTarget.CanPlaceOnMud = EditorGUILayout.Toggle(myTarget.CanPlaceOnMud, GUILayout.Width(30));
                GUILayout.Label("Mud Cubes", GUILayout.Width(100));
                GUILayout.Space(70);
                myTarget.CanPlaceOnRock = EditorGUILayout.Toggle(myTarget.CanPlaceOnRock, GUILayout.Width(30));
                GUILayout.Label("Rock Cubes", GUILayout.Width(100));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(70);
                myTarget.CanPlaceOnSand = EditorGUILayout.Toggle(myTarget.CanPlaceOnSand, GUILayout.Width(30));
                GUILayout.Label("Sand Cubes", GUILayout.Width(100));
                GUILayout.Space(70);
                myTarget.CanPlaceOnSnow = EditorGUILayout.Toggle(myTarget.CanPlaceOnSnow, GUILayout.Width(30));
                GUILayout.Label("Snow Cubes", GUILayout.Width(100));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(70);
                myTarget.CanPlaceOnLava = EditorGUILayout.Toggle(myTarget.CanPlaceOnLava, GUILayout.Width(30));
                GUILayout.Label("Lava", GUILayout.Width(100));
                GUILayout.Space(70);
                myTarget.CanPlaceOnWater = EditorGUILayout.Toggle(myTarget.CanPlaceOnWater, GUILayout.Width(30));
                GUILayout.Label("Lakes & Rivers", GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }
            

            if ((GUI.changed || _valueChanged) && !Application.isPlaying) myTarget.UpdatePrefab();
        }
    }
}
