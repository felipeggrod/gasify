using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace GAS {
    public static class GameplayCueManager {
        public static void Register(AbilitySystemComponent asc) {
            asc.OnGameplayEffectApplied += (ge) => { if (ge.cuesTags?.Count > 0) OnApplyCue(ge.cuesTags, asc, ge.durationType == GameplayEffectDurationType.Instant, null, ge); };
            asc.OnGameplayEffectRemoved += (ge) => { if (ge.cuesTags?.Count > 0) OnRemoveCue(ge.cuesTags, asc, null, ge); };

            asc.OnGameplayAbilityActivated += (ga, activationGUID) => { if (ga.cuesTags?.Count > 0) OnApplyCue(ga.cuesTags, asc, !ga.isActive, ga, null); };
            asc.OnGameplayAbilityDeactivated += (ga, activationGUID) => { if (ga.cuesTags?.Count > 0) OnRemoveCue(ga.cuesTags, asc, ga, null); };
        }

        static void OnApplyCue(List<GameplayTag> cueTags, AbilitySystemComponent asc, bool instantDestroy, GameplayAbility ga, GameplayEffect ge) {
            foreach (GameplayTag cueTag in cueTags) {
                List<GameplayCue> instancedCues = CuesLibrary.Instance.CreateCues(cueTag);
                foreach (var instancedCue in instancedCues) {
                    if (instancedCue == null) return;
                    instancedCue.AddCue(asc, instantDestroy, new GameplayCueApplicationData(ga, ge, asc, null));
                }
            }
        }

        static void OnRemoveCue(List<GameplayTag> cueTags, AbilitySystemComponent asc, GameplayAbility ga, GameplayEffect ge) {
            for (int i = 0; i < asc.instancedCues.Count; i++) {
                if (cueTags.Contains(asc.instancedCues[i].tag) && asc.instancedCues[i].applicationData.IsOrigin(ga, ge)) {
                    asc.instancedCues[i].RemoveCue(asc);
                }
            }
        }
    }

    public class GameplayCueApplicationData {
        public GameplayAbility ga;
        public GameplayEffect ge;
        public AbilitySystemComponent src, tgt;
        public string originName;

        public GameplayCueApplicationData(GameplayAbility ga, GameplayEffect ge, AbilitySystemComponent src, AbilitySystemComponent tgt) {
            this.ga = ga;
            this.ge = ge;
            this.src = src;
            this.tgt = tgt;
            originName = ga == null ? ge.name : ga.name;
        }

        public bool IsOrigin(GameplayAbility gaToCheck, GameplayEffect geToCheck) {
            if (gaToCheck == ga) return true;
            if (geToCheck == ge) return true;
            return false;
        }
    }

    [System.Serializable]
    public class GameplayCue {
        public GameObject prefab; //Can be a looping SFX or VFX
        public GameObject instance; //instantiated cue go
        public GameplayTag tag;
        public Vector3 offset;

        public GameplayCueApplicationData applicationData;

        public virtual void AddCue(AbilitySystemComponent asc, bool instantDestroy, GameplayCueApplicationData appData) {
            if (prefab == null) { Debug.Log($"AddCue with NULL prefab"); return; }
            applicationData = appData;
            PlaceCue(asc);
            if (instantDestroy) {
                RemoveCue(asc);
            }
        }

        public virtual async void RemoveCue(AbilitySystemComponent asc) {
            // Debug.Log($"RemoveCue - cue tag: {tag.name}");
            if (instance != null) instance.SendMessage("OnDestroySoon", SendMessageOptions.DontRequireReceiver);
            await Task.Delay(3_000);
            // Debug.Log($"RemoveCue - cue tag: {tag.name} AFTER DELAY");
            asc.instancedCues.Remove(this);
            if (instance == null) return;
            // Debug.Log($"RemoveCue: instance.name {instance.name}");
            GameObject.Destroy(instance);
        }

        public void PlaceCue(AbilitySystemComponent asc) {
            instance = GameObject.Instantiate(prefab);
            instance.name = "cueInstance_" + prefab.name;
            // Debug.Log($"PlaceCue: place {spawnPlace} src {src} target {target} ");

            instance.transform.SetParent(asc.transform);
            instance.transform.position = asc.transform.position + asc.transform.forward * offset.z + asc.transform.right * offset.x + asc.transform.up * offset.y;
            asc.instancedCues.Add(this);

        }


        //Examples: Melee vfx (trail and impact), Projectile impact, Spell cooldown failed.

        // We trigger GameplayCues by sending a corresponding GameplayTag with the mandatory parent name of GameplayCue. e.g. CueEvent<GameplayTag> = GameplayTag.Cue_XYZ
        // public Action OnActive, WhileActive, Removed, Executed; unreal has those... do we need them?

        // AggregatedSourceTags
        // AggregatedTargetTags
        // GameplayEffectLevel
        // AbilityLevel
        // EffectContext
        // Magnitude (if the GameplayEffect has an Attribute for magnitude selected in the dropdown above the GameplayCue tag container and a corresponding Modifier that affects that Attribute)
    }
}
