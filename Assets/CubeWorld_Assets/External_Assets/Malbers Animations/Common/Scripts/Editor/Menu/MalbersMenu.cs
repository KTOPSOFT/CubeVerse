

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations
{
    public class MalbersMenu : EditorWindow
    {
        //const string URP14_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_URP_14.unitypackage";
        const string URP14_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_URP_14.unitypackage";
        // const string URP16_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_URP_16.unitypackage";
        const string URP17_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_URP_17.unitypackage";

        //  const string HRP15_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_HDRP_15.unitypackage";
        const string HRP14_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_HDRP_15.unitypackage";
        // const string HRP16_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_HDRP_16.unitypackage";
        const string HRP17_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_HDRP_17.unitypackage";

        const string D_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_Standard.unitypackage";


        [MenuItem("Tools/Malbers Animations/Malbers URP 17 Shaders (Unity6) ", false, 2)]
        public static void UpgradeMaterialsURP_17() => AssetDatabase.ImportPackage(URP17_Shader_Path, true);

        [MenuItem("Tools/Malbers Animations/Malbers URP 14 Shaders (Unity 2022.3 LTS) ", false, 1)]
        public static void UpgradeMaterialsURP_14() => AssetDatabase.ImportPackage(URP14_Shader_Path, true);

        //[MenuItem("Tools/Malbers Animations/Malbers URP 16 Shaders", false, 2)]
        //public static void UpgradeMaterialsURP_16() => AssetDatabase.ImportPackage(URP16_Shader_Path, true);

        [MenuItem("Tools/Malbers Animations/Malbers HDRP 14 Shaders (Unity 2022.3 LTS) ", false, 3)]
        public static void UpgradeMaterialsHDRP_14() => AssetDatabase.ImportPackage(HRP14_Shader_Path, true);

        [MenuItem("Tools/Malbers Animations/Malbers HDRP 17 Shaders (Unity6)", false, 3)]
        public static void UpgradeMaterialsHDRP_17() => AssetDatabase.ImportPackage(HRP17_Shader_Path, true);


        [MenuItem("Tools/Malbers Animations/Malbers Standard Shaders", false, 1)]
        public static void UpgradeMaterialsStandard() => AssetDatabase.ImportPackage(D_Shader_Path, true);




        [MenuItem("Tools/Malbers Animations/Integrations", false, 600)]
        public static void OpenIntegrations() => Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/annex/integrations");


        [MenuItem("Tools/Malbers Animations/What's New", false, 600)]
        public static void OpenWhatsNew() => Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/whats-new");
    }
}
#endif