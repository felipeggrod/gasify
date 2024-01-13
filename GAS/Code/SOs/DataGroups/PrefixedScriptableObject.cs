
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GAS {
    [Serializable]
    public abstract class PrefixedScriptableObject : ScriptableObject {
        public virtual void OnValidate() {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(name)) {
                string prefix = this.GetType().ToString().Replace("GAS.", "") + "_";// + "_PREFIX_";
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this.GetInstanceID());
                string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (!assetName.Contains(prefix)) {
                    UnityEditor.AssetDatabase.RenameAsset(assetPath, prefix + assetName);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
                // ga.name = assetName.Replace(prefix, "");
            }
#endif
        }

    }
}