using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AMTutorialArchitecture.Tutorial.Editor
{
    public sealed class RequiredActionAssetSaveProcessor : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                var action = AssetDatabase.LoadAssetAtPath<RequiredActionAsset>(path);
                if (action != null)
                {
                    action.SynchronizeEditorId();
                }
            }

            return paths;
        }
    }

    public sealed class RequiredActionAssetBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:RequiredActionAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var action = AssetDatabase.LoadAssetAtPath<RequiredActionAsset>(path);
                if (action != null)
                {
                    action.SynchronizeEditorId();
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
