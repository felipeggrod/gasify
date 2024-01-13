using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

using System;


namespace GAS {

    public static class TagProcessor {
        public static bool HasAnyTag(List<GameplayTag> tagsToCheck, List<GameplayTag> tagList) {
            foreach (var tagToCheck in tagsToCheck) {
                if (tagList.Contains(tagToCheck)) return true;
            }
            return false;
        }
        public static bool HasTag(GameplayTag tagToCheck, List<GameplayTag> tagList) {
            if (tagList.Contains(tagToCheck)) return true;
            return false;
        }

        public static bool CheckTagRequirements(AbilitySystemComponent asc, List<GameplayTag> currentTags, List<GameplayTag> requiredTags, List<GameplayTag> forbiddenTags) {
            return true;
        }

        public static bool CheckApplicationTagRequirementsGE(AbilitySystemComponent asc, GameplayEffect ge, List<GameplayTag> currentTags) {
            // Debug.Log($"CheckTagRequirementsMetGE: {ge.name}");
            return CheckTagRequirements(asc, currentTags, ge.gameplayEffectTags.ApplicationTagRequirementsRequired, ge.gameplayEffectTags.ApplicationTagRequirementsForbidden);
        }

        public static void UpdateTags(AbilitySystemComponent source, AbilitySystemComponent target, ref List<GameplayTag> currentTags, List<GameplayEffect> appliedGameplayEffects, List<GameplayAbility> gameplayAbilities, Action<List<GameplayTag>, AbilitySystemComponent, AbilitySystemComponent, string> OnTagsChanged, string activationGUID) {
            //We could use HashSet instead of List here, if performance here is critical. HOWEVER, the tradeoff is that HashSets cannot contain multiple instances of the same tag. So you would not be able to stack tags.
            //There is some more room for optimization aswell, like removing these allocations
            var geTags = new List<GameplayTag>();
            foreach (var appliedGE in appliedGameplayEffects) {
                // Debug.Log($"        GE: {appliedGE.name} Tags({appliedGE.gameplayEffectTags.GrantedTags.Count}): [{string.Join(",", appliedGE.gameplayEffectTags.GrantedTags.Select(x => x.name))}]");
                foreach (var tag in appliedGE.gameplayEffectTags.GrantedTags) {
                    geTags.Add(tag);
                }
            }

            var gaTags = new List<GameplayTag>();
            foreach (var ability in gameplayAbilities) {
                // Debug.Log($"        GA Tags ({ability.abilityTags.ActivationOwnedTags.Count}): [{string.Join(",", ability.abilityTags.ActivationOwnedTags.Select(x => x.name))}]");
                if (ability.isActive) {
                    foreach (var tag in ability.abilityTags.ActivationOwnedTags) {
                        gaTags.Add(tag);
                    }
                }
            }

            // Debug.Log($"OnGameplayEffectsTagsChanged?: {!currentTags.SequenceEqual(newTags)}");
            var newTags = new List<GameplayTag>();
            newTags.AddRange(geTags);
            newTags.AddRange(gaTags);

            // Debug.Log($"       currentTags ({currentTags.Count}): [{string.Join(",", currentTags.Select(x => x.name))}] / newTags ({newTags.Count}): [{string.Join(",", newTags.Select(x => x.name))}]");
            if (!currentTags.SequenceEqual(newTags)) { //Must run BEFORE calling OnTagsAdded/OnTagsRemoved. Because they will use currentTags to calculate their tag diff. If currentTags doesnt update, then we'll run into a infinite loop with TriggerAbilities where applied GEs still dont have their tags on currentTags, and will be retriggered because the appliedGEs tags will be put into newTags, even tough its a different GE being applied.
                currentTags = new List<GameplayTag>(newTags);
                OnTagsChanged?.Invoke(currentTags, source, target, activationGUID);
            }

        }


    }

}