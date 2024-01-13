using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GAS;
using EasyButtons;

namespace GAS {
    [CreateAssetMenu(menuName = "GAS/GameplayEffectSO", fileName = "GE_")]
    [Serializable]
    public class GameplayEffectSO : ScriptableObject {
        public GameplayEffect ge;

#if UNITY_EDITOR
        private void OnValidate() {
            // private void OnEnable() {
            if (!string.IsNullOrEmpty(name)) {
                string prefix = "GE_";
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this.GetInstanceID());
                string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (!assetName.Contains(prefix)) {
                    UnityEditor.AssetDatabase.RenameAsset(assetPath, prefix + assetName);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
                ge.name = assetName.Replace(prefix, "");
            }
        }
#endif


        [Button("ADD MODIFIER WITH TYPE", Expanded = true, Spacing = ButtonSpacing.Before)]
        public void ADD_MODIFIER_VIA_EDITOR(ModifierType modifierType) {// Used to create new mod types on editor. Not recommended to add mods to ges at runtime. But hey, if it works for you, who am I to say no?
            Helpers.AddModifier(modifierType, this.ge);
        }
    }
}