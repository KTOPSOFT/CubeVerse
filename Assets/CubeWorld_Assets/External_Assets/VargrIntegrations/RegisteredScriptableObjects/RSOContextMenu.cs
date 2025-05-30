using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VargrIntegrations
{
#if UNITY_EDITOR
    public class RSOContextMenu
    {
        [MenuItem("CONTEXT/ScriptableObject/Add to RSO Library")]
        static void AddContextRSO(MenuCommand command)
        {
            if(command.context is RSORoot){
                RSOManager.Database.Add((RSORoot)command.context);
            }else{
                RSOManager.Database.Add((ScriptableObject)command.context);
            }
        }

        [MenuItem("Assets/Add to RSO Library")]
        static void AddAssetRSO()
        {
            if(Selection.activeObject is RSORoot){
                RSOManager.Database.Add((RSORoot)Selection.activeObject);
            }else{
                RSOManager.Database.Add((ScriptableObject)Selection.activeObject);
            }
        }

        [MenuItem("Assets/Add to RSO Library", true)]
        static bool AddAssetRSOValidation()
        {
            return Selection.activeObject is ScriptableObject;
        }
    }
#endif
}