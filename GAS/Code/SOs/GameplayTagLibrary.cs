using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.IO;
using EasyButtons;
using GAS;
using System.Threading.Tasks;

namespace GAS {
#pragma warning disable 0618
    public static class GameplayTags {
        public static GameplayTagLibrary library;
    }

    [CreateAssetMenu(menuName = "GAS/GameplayTagLibrary", fileName = "GameplayTagLibrary")]
    [Serializable]
    public class GameplayTagLibrary : SingletonScriptableObjectLibrary<GameplayTagLibrary, GameplayTag> {

        public List<GameplayTag> GetByNames(List<string> tagNames) { //full name, including parents. A.B.XYZ
            List<GameplayTag> foundTag = itemList.Where(tag => tagNames.Contains(tag.name)).ToList();
            return foundTag;
        }

        public GameplayTag GetByIndex(int index) {
            return itemList[index];
        }

        [Button]
        public void LogStaticReference() {
            Debug.Log($"static ref: {GameplayTags.library}");
        }

        // [Button]
        // [Tooltip("Create tags from a list of tagNames separated by comma. e.g. Tag1, Special.Tag2, A.B.C.Tag3...")]
        // public void CreateAssetsForGameplayTagNames(string names) {//creates the gameplayTag SO assets for given strings. so we could serialize/deserialize all tags 
        //     List<string> tagNames = names.Split(',').ToList();
        //     // tagNames.ForEach(tagName => )
        // }

        public bool SerializeString() {
            string s = JsonUtility.ToJson(this, true);
            Debug.Log(s);
            return true;
        }

        public bool IsParent(GameplayTag child, GameplayTag parent) { //Checks if tag is parent of another
            if (child.name.Contains(parent.name)) return true;
            else return false;
        }



    }

#pragma warning restore 0618
}