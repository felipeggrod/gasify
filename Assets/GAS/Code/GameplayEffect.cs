using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using EasyButtons;

namespace GAS {
    [Serializable]
    public enum GameplayEffectDurationType {
        Instant,
        Infinite,
        Duration,
    }

    /// <summary> <para>
    /// Effects define how attributes are modified, and the duration of these modifications. 
    /// </para> </summary> 
    [Serializable]
    public class GameplayEffect {
        public string name;
        public string description;
        // public Modifier durationModifier; //what is this for on unreal gas??? modifier magnitude???
        public GameplayEffectDurationType durationType; //duration type or policy
        [Tooltip("How long the effect will last. Ignored if durationType is Instant or Infinite.")]
        public float durationValue;
        [Tooltip("Modifiers will be applied every (period) seconds, while GE is active (Duration or Infinite). Ignored if 0.")]
        public float period = 0f;
        public bool periodicExpired = false;
        [SerializeReference] public List<Modifier> modifiers = new List<Modifier>();
        public AbilitySystemComponent source, target;
        public GameplayEffectTags gameplayEffectTags = new GameplayEffectTags();
        [SerializeReference]
        public List<GameplayTag> cuesTags = new List<GameplayTag>();
        public float level = 1f;
        public float chanceToApply = 1f;
        public string guid;
        public string applicationGUID;

        [EasyButtons.Button]
        public GameplayEffect Instantiate() { //break references to original
            // double initTime = Time.realtimeSinceStartupAsDouble;
            // !!! IMPORTANT !!! Apparently, unity's json utility serializes our class WRONGLY. It turns all calculations, which are derived classes, into their base class.

            GameplayEffect geCopy = new GameplayEffect();
            geCopy.guid = Guid.NewGuid().ToString();
            geCopy.applicationGUID = applicationGUID;

            geCopy.name = name;
            geCopy.description = description;
            geCopy.durationType = durationType;
            geCopy.durationValue = durationValue;
            geCopy.period = period;
            geCopy.modifiers = modifiers;


            geCopy.cuesTags = cuesTags;
            geCopy.level = level;
            geCopy.chanceToApply = chanceToApply;

            geCopy.target = target;
            geCopy.source = source;


            //Copy tags to new instance
            geCopy.gameplayEffectTags = gameplayEffectTags;
            if (!gameplayEffectTags.initialized) {
                geCopy.gameplayEffectTags.FillTags(geCopy); //Once instantiated, fill the SOs
                geCopy.gameplayEffectTags.ClearStrings(); //design issue with initialized. Causes CD GEs with strings to have tags added duplicately (because initialized is false, and it has string.) ALSO LOOK INTO APPLICATION GUID
            }

            return geCopy;
        }
    }

    [Serializable]
    public class GameplayEffectTags {
        /// <summary>
        /// Tags that live on the GameplayEffect but are also given to the ASC that the GameplayEffect is applied to. 
        /// They are removed from the ASC when the GameplayEffect is removed. 
        /// This only works for Duration and Infinite GameplayEffects. </summary>
        [Tooltip("Tags that live on the GameplayEffect but are also given to the ASC that the GameplayEffect is applied to. They are removed from the ASC when the GameplayEffect is removed.")]
        [SerializeField] public List<GameplayTag> GrantedTags = new List<GameplayTag>();
        /// <summary>
        /// Tags that describe the GameplayEffect. They do not do any function on their own and serve only the purpose of describing the GameplayEffect. </summary>
        [Tooltip("Tags that describe the GameplayEffect. They do not do any function on their own and serve only the purpose of describing the GameplayEffect.")]
        [SerializeField] public List<GameplayTag> DescriptionTags = new List<GameplayTag>();

        /// <summary>
        /// Once applied, these tags determine whether the GameplayEffect is on or off. A GameplayEffect can be off and still be applied. 
        /// If a GameplayEffect is off due to failing the Ongoing Tag Requirements, but the requirements are then met, the GameplayEffect will turn on again and reapply its modifiers. 
        /// This only works for Duration and Infinite GameplayEffects. </summary> 
        [Tooltip("Once applied, these tags determine whether the GameplayEffect is on or off.")]
        [SerializeField] public List<GameplayTag> OngoingTagRequirementsRequired = new List<GameplayTag>();
        [Tooltip("Once applied, these tags determine whether the GameplayEffect is on or off.")]
        [SerializeField] public List<GameplayTag> OngoingTagRequirementsForbidden = new List<GameplayTag>();

        /// <summary> If any of these tags IS NOT present, the GameplayEffect will not apply. </summary>
        [Tooltip("If any of these tags IS NOT present, the GameplayEffect will not apply.")]
        [SerializeField] public List<GameplayTag> ApplicationTagRequirementsRequired = new List<GameplayTag>();
        /// <summary> If any of these tags IS present, the GameplayEffect will not apply.  </summary>
        [Tooltip("If any of these tags IS present, the GameplayEffect will not apply.")]
        [SerializeField] public List<GameplayTag> ApplicationTagRequirementsForbidden = new List<GameplayTag>();

        // /// <summary> If any of these tags IS NOT present, this GE will be removed.</summary>
        // [SerializeField] public List<GameplayTagSO> RemovalTagRequirementsRequired = new List<GameplayTagSO>(); //Tag requirements that will remove this GE 
        // /// <summary>  If any of these tags IS present, this GE will be removed.  </summary>
        // [SerializeField] public List<GameplayTagSO> RemovalTagRequirementsForbidden = new List<GameplayTagSO>();

        /// <summary>GameplayEffects on the Target that have any of these tags in their Granted Tags will be removed from the Target when this GameplayEffect is successfully applied. </summary>
        [Tooltip("GameplayEffects on the Target that have any of these tags in their Granted Tags will be removed from the Target when this GameplayEffect is successfully applied. ")]
        [SerializeField] public List<GameplayTag> RemoveGameplayEffectsWithTag = new List<GameplayTag>(); //Easier to just make it on GA, or maybe not depending on situation... e.g. AntiPoison, Remove All Debuffs, Remove all bleed effects, Remove all shadow resistance???

        // WARNING! These must be public for networkg serialization to pick them!
        [HideInInspector] public bool initialized = false;
        [HideInInspector] public List<string> string_GrantedTags = new List<string>();
        [HideInInspector] public List<string> string_DescriptionTags = new List<string>();

        [HideInInspector] public List<string> string_OngoingTagRequirementsRequired = new List<string>();
        [HideInInspector] public List<string> string_OngoingTagRequirementsForbidden = new List<string>();

        [HideInInspector] public List<string> string_ApplicationTagRequirementsRequired = new List<string>();
        [HideInInspector] public List<string> string_ApplicationTagRequirementsForbidden = new List<string>();

        [HideInInspector] public List<string> string_RemovalTagRequirementsRequired = new List<string>();
        [HideInInspector] public List<string> string_RemovalTagRequirementsForbidden = new List<string>();

        [HideInInspector] public List<string> string_RemoveGameplayEffectsWithTag = new List<string>();

        [HideInInspector] public List<string> string_CueTags = new List<string>();

        public void FillTags(GameplayEffect ge) {
            initialized = true;
            // Debug.Log($"GE {ge.name} - GetAllTags GrantedTags: [{string.Join(", ", GrantedTags.Select(x => x.name))}]  string_GrantedTags: [{string.Join(", ", string_GrantedTags.Select(x => x))}]");
            GrantedTags = GrantedTags.Union(GameplayTagLibrary.Instance.GetByNames(string_GrantedTags)).ToList();
            DescriptionTags = DescriptionTags.Union(GameplayTagLibrary.Instance.GetByNames(string_DescriptionTags)).ToList();
            OngoingTagRequirementsRequired = OngoingTagRequirementsRequired.Union(GameplayTagLibrary.Instance.GetByNames(string_OngoingTagRequirementsRequired)).ToList();
            OngoingTagRequirementsForbidden = OngoingTagRequirementsForbidden.Union(GameplayTagLibrary.Instance.GetByNames(string_OngoingTagRequirementsForbidden)).ToList();
            ApplicationTagRequirementsRequired = ApplicationTagRequirementsRequired.Union(GameplayTagLibrary.Instance.GetByNames(string_ApplicationTagRequirementsRequired)).ToList();
            ApplicationTagRequirementsForbidden = ApplicationTagRequirementsForbidden.Union(GameplayTagLibrary.Instance.GetByNames(string_ApplicationTagRequirementsForbidden)).ToList();
            // Uncomment the following lines if needed
            // RemovalTagRequirementsRequired = RemovalTagRequirementsRequired.Union(GameplayTagLibrary.Instance.GetByNames(string_RemovalTagRequirementsRequired)).ToList();
            // RemovalTagRequirementsForbidden = RemovalTagRequirementsForbidden.Union(GameplayTagLibrary.Instance.GetByNames(string_RemovalTagRequirementsForbidden)).ToList();
            RemoveGameplayEffectsWithTag = RemoveGameplayEffectsWithTag.Union(GameplayTagLibrary.Instance.GetByNames(string_RemoveGameplayEffectsWithTag)).ToList();
            ge.cuesTags = ge.cuesTags.Union(GameplayTagLibrary.Instance.GetByNames(string_CueTags)).ToList();

        }

        public void FillStrings(GameplayEffect ge) {
            string_GrantedTags = GrantedTags.Select(tag => tag.name).ToList();
            string_DescriptionTags = DescriptionTags.Select(tag => tag.name).ToList();
            string_OngoingTagRequirementsRequired = OngoingTagRequirementsRequired.Select(tag => tag.name).ToList();
            string_OngoingTagRequirementsForbidden = OngoingTagRequirementsForbidden.Select(tag => tag.name).ToList();
            string_ApplicationTagRequirementsRequired = ApplicationTagRequirementsRequired.Select(tag => tag.name).ToList();
            string_ApplicationTagRequirementsForbidden = ApplicationTagRequirementsForbidden.Select(tag => tag.name).ToList();
            // string_RemovalTagRequirementsRequired = RemovalTagRequirementsRequired.Select(tag => tag.name).ToList();
            // string_RemovalTagRequirementsForbidden = RemovalTagRequirementsForbidden.Select(tag => tag.name).ToList();
            string_RemoveGameplayEffectsWithTag = RemoveGameplayEffectsWithTag.Select(tag => tag.name).ToList();

            string_CueTags = ge.cuesTags.Select(tag => tag.name).ToList();
        }

        public void ClearTags(GameplayEffect ge) {
            GrantedTags.Clear();
            DescriptionTags.Clear();
            OngoingTagRequirementsRequired.Clear();
            OngoingTagRequirementsForbidden.Clear();
            ApplicationTagRequirementsRequired.Clear();
            ApplicationTagRequirementsForbidden.Clear();
            // RemovalTagRequirementsRequired.Clear();
            // RemovalTagRequirementsForbidden.Clear();
            RemoveGameplayEffectsWithTag.Clear();

            ge.cuesTags.Clear();
        }

        public void ClearStrings() {
            string_DescriptionTags.Clear();
            string_GrantedTags.Clear();
            string_OngoingTagRequirementsRequired.Clear();
            string_OngoingTagRequirementsForbidden.Clear();
            string_ApplicationTagRequirementsRequired.Clear();
            string_ApplicationTagRequirementsForbidden.Clear();
            // string_RemovalTagRequirementsRequired.Clear();
            // string_RemovalTagRequirementsForbidden.Clear();
            string_RemoveGameplayEffectsWithTag.Clear();

            string_CueTags.Clear();
        }
    }
}
