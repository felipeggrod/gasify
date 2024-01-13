using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace GAS {
    // [CreateAssetMenu(menuName = "GAS/GameplayAbility", fileName = "GA_")] //this is the parent class. it doesnt apply the GEs (e.g. instant vs. projectile abilities apply ges at different times)
    [Serializable]
    public abstract class GameplayAbilitySO : ScriptableObject {
        [SerializeReference] public GameplayAbility ga;

        [EasyButtons.Button]
        public void CreateCostGE() {
            ga.CreateCostGE(new List<Modifier>() { new BasicModifier() });
        }

        [EasyButtons.Button]
        public void CreateCoolDownGE() {
            ga.CreateCoolDownGE(1);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            // private void OnEnable() {
            if (!string.IsNullOrEmpty(name)) {
                string prefix = "GA_";
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this.GetInstanceID());
                string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (!assetName.Contains(prefix)) {
                    UnityEditor.AssetDatabase.RenameAsset(assetPath, prefix + assetName);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
                ga.name = assetName.Replace(prefix, "");
            }
        }
#endif
    }
}