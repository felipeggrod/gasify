using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace GAS {
    [System.Serializable]
    public class GameplayTagsWithCue {
        public string name = "";
        public GameplayTag tag;
        public GameplayCue cue;
    }

    [CreateAssetMenu(menuName = "GAS/CuesLibrary", fileName = "CuesLibrary")]
    [Serializable]
    public class CuesLibrary : SingletonScriptableObject<CuesLibrary> {
        public List<GameplayTagsWithCue> cuesLibrary = new List<GameplayTagsWithCue>();


        public List<GameplayCue> CreateCues(GameplayTag tag) {//pass an copy instance of a gameplay cue, so we can trigger same cue multiple times.
            List<GameplayCue> originalCues = cuesLibrary.Where(tagWithCue => tagWithCue.tag == tag).Select(tagWithCue => tagWithCue.cue).ToList();
            List<GameplayCue> copyCues = new List<GameplayCue>();

            foreach (GameplayCue cue in originalCues) {
                if (cue == null) return null;
                GameplayCue copy = new GameplayCue() {
                    prefab = cue.prefab,
                    offset = cue.offset,
                    tag = tag
                };
                copyCues.Add(copy);
            }
            // Debug.Log($"CreateCues copyCues: [{string.Join(", ", copyCues)}] \n");
            return copyCues;
        }

        private void OnValidate() {
            cuesLibrary.ForEach(cue => cue.name = cue.tag.name); //validate names in inspector for SOs
        }
    }
}