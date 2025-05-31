using System.IO;
using InsaneScatterbrain.ScriptGraph;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

#if MAP_GRAPH_TILEMAP
using UnityEngine.Tilemaps;
#endif

namespace InsaneScatterbrain.MapGraph.Editor
{
    public static class MapGraphCreateMenu
    {
        private class CreateTilemapPrefabAction : EndNameEditAction
        {
            public override void Action(int instanceId, string path, string resourceFile)
            {
                var gameObject = new GameObject();
                gameObject.AddComponent<MapGraphTilemapPrefab>();
                PrefabUtility.SaveAsPrefabAsset(gameObject, path);
                DestroyImmediate(gameObject);
            }
        }
        
        private static string CurrentProjectPath()
        {
            if (Selection.activeObject == null) return "Assets";
            
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            
            return Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        }

        [MenuItem("Assets/Create/Map Graph/Default Assets/Tilemap", priority = 0)]
        private static void CreateMapGraphTilemap()
        {
            var graph = MapGraphAsset.Create<MapGraphGraph>("Map Graph");
            var colorSet = MapGraphAsset.Create<NamedColorSet>("Map Graph Color Set");

            graph.NamedColorSet = colorSet;
            
            var tileSet = MapGraphAsset.Create<Tileset>("Map Graph Tileset");
            colorSet.Link(tileSet);
            
            EditorUtility.SetDirty(tileSet);
            EditorUtility.SetDirty(colorSet);
            EditorUtility.SetDirty(graph);
        }

        [MenuItem("Assets/Create/Map Graph/Default Assets/Prefabs", priority = 0)]
        private static void CreateMapGraphPrefabs()
        {
            var graph = MapGraphAsset.Create<MapGraphGraph>("Map Graph");
            var colorSet = MapGraphAsset.Create<NamedColorSet>("Map Graph Color Set");

            graph.NamedColorSet = colorSet;
            
            var prefabSet = MapGraphAsset.Create<PrefabSet>("Map Graph Prefab Set");
            colorSet.Link(prefabSet);
            
            EditorUtility.SetDirty(prefabSet);
            EditorUtility.SetDirty(colorSet);
            EditorUtility.SetDirty(graph);
        }

        [MenuItem("Assets/Create/Map Graph/Map Graph", priority = 0)]
        private static void CreateMapGraph() => MapGraphAsset.Create<MapGraphGraph>("Map Graph");
        
        [MenuItem("Assets/Create/Map Graph/Named Color Set", priority = 0)]
        private static void CreateNamedColorSet() => MapGraphAsset.Create<NamedColorSet>("Named Color Set");
        
        [MenuItem("Assets/Create/Map Graph/Prefab Set", priority = 0)]
        private static void CreatePrefabSet() => MapGraphAsset.Create<PrefabSet>("Prefab Set");
        
        [MenuItem("Assets/Create/Map Graph/Tileset", priority = 0)]
        private static void CreateTileset() => MapGraphAsset.Create<Tileset>("Tileset");

        [MenuItem("Assets/Create/Map Graph/Tilemap Set", priority = 0)]
        private static void CreateTilemapSet() => MapGraphAsset.Create<TilemapSet>("Tilemap Set");
        
        [MenuItem("Assets/Create/Map Graph/Tilemap Prefab", priority = 0)]
        private static void CreateTilemapPrefabMenu()
        {
            var createAction = ScriptableObject.CreateInstance<CreateTilemapPrefabAction>();
            var path = Path.Combine(CurrentProjectPath(), "Tilemap Prefab.prefab");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, createAction, path, EditorGUIUtility.FindTexture("Prefab Icon"), null);
        }

        [MenuItem("GameObject/Map Graph/Graph Runner", priority = 49)]
        private static void CreateGraphRunner()
        {
            var gameObject = new GameObject("Graph Runner");
            gameObject.AddComponent<ScriptGraphRunner>();
            Selection.activeGameObject = gameObject;
        }

        [MenuItem("Assets/Create/Map Graph/Save Data/Save Data", priority = 0)]
        private static void CreateSaveData() => MapGraphAsset.Create<SaveData>("Save Data");

        [MenuItem("Assets/Create/Map Graph/Save Data/Load Tilemap Graph", priority = 0)]
        private static void CreateLoadTilemapGraph()
        {
            var loadGraphPrefab =
                AssetDatabase.LoadAssetAtPath<MapGraphGraph>("Assets/Plugins/InsaneScatterbrain/MapGraph.Assets/SaveLoad/LoadTilemap.asset");

            var loadGraphInstance = Object.Instantiate(loadGraphPrefab);
            ProjectWindowUtil.CreateAsset(loadGraphInstance, "Load Tilemap Graph.asset");
        }
        
        [MenuItem("Assets/Create/Map Graph/Save Data/Load GameObjects Graph", priority = 0)]
        private static void CreateLoadGameObjectsGraph()
        {
            var loadGraphPrefab =
                AssetDatabase.LoadAssetAtPath<MapGraphGraph>("Assets/Plugins/InsaneScatterbrain/MapGraph.Assets/SaveLoad/LoadGameObjects.asset");

            var loadGraphInstance = Object.Instantiate(loadGraphPrefab);
            ProjectWindowUtil.CreateAsset(loadGraphInstance, "Load GameObjects Graph.asset");
        }
        
        [MenuItem("Assets/Create/Map Graph/Custom Node Script", priority = 10)]
        private static void CreateCustomNodeScript()
        {
            var templatePath = Path.Combine(Application.dataPath, "Plugins/InsaneScatterbrain/ScriptGraph.Editor.Assets/CustomNodeTemplate.cs.txt");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "CustomNode.cs");
        }
        
#if MAP_GRAPH_TILEMAP
        [MenuItem("Assets/Create/Map Graph/Tile", priority = 0)]
        private static void CreateTile()
        {
            var tileInstance = ScriptableObject.CreateInstance<Tile>();
            ProjectWindowUtil.CreateAsset(tileInstance, "Tile.asset"); 
        }
#endif
    }
}