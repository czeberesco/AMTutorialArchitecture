using AMTutorialArchitecture.Tutorial.Domain.Actions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    public abstract class RequiredActionAsset : ScriptableObject
    {
        [SerializeField, HideInInspector] private string assetGuid;

        public string Id
        {
            get
            {
#if UNITY_EDITOR
                SynchronizeEditorId();
#endif
                return assetGuid ?? string.Empty;
            }
        }

        public abstract IRequiredActionDefinition CreateDefinition();

#if UNITY_EDITOR
        public bool SynchronizeEditorId()
        {
            var path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var currentGuid = AssetDatabase.AssetPathToGUID(path);
            if (string.Equals(assetGuid, currentGuid, System.StringComparison.Ordinal))
            {
                return false;
            }

            assetGuid = currentGuid;
            EditorUtility.SetDirty(this);
            return true;
        }

        protected virtual void OnValidate()
        {
            SynchronizeEditorId();
        }
#endif
    }
}
