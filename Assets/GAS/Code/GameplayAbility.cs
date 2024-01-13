using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Events;
using EasyButtons;


namespace GAS {
    /// <summary>
    ///  A Gameplay Ability is any action the ASC (Ability System Component) can use. <br />
    /// These can be spells, skills, passives, interactions or any other action.
    /// The most common uses (e.g. Instant, Projectile, etc..) examples are implemented. <br />
    /// You can also extend that class to create even more interesting abilities.
    /// </summary> 
    // Instant, Passive, Toggeable, Channelled(todo), Cast(todo), triggered(todo?)
    // Some channelled abilities still have a duration time e.g. MindControl
    [Serializable]
    public class GameplayAbility {
        [ReadOnly] public string name;
        public GameplayEffect cooldown = null; // GE with duration of Xs
        public GameplayEffect cost = null;
        [ReadOnly] public List<GameplayEffect> effects = new List<GameplayEffect>();
        public List<GameplayEffectSO> effectsSO = new List<GameplayEffectSO>();
        [SerializeReference] public AbilityTags abilityTags = new AbilityTags();

        public AbilitySystemComponent source, target, owner; //Only filled on activation/instantiation. Owner adds the ASC that has it instantiated on. Mostly used by PassiveAbility
        [SerializeReference] public List<GameplayTag> cuesTags = new List<GameplayTag>();

        [ReadOnly] public string guid;
        // [ReadOnly] public string activationGUID;
        [ReadOnly] public string className;
        public float level = 1;
        public bool isActive;
        private float timeActivated;
        public string activationGUID;


        /// <summary>
        /// Creates a new instance for the GA, also re-instantiate its GEs.
        /// Used on GrantAbility.
        /// </summary> 
        public virtual GameplayAbility Instantiate(AbilitySystemComponent owner) {
            this.owner = owner;
            Type classType = this.GetType();
            GameplayAbility gaCopy = (GameplayAbility)Activator.CreateInstance(classType);
            gaCopy.guid = Guid.NewGuid().ToString();
            gaCopy.className = this.GetType().FullName;

            gaCopy.effects = this.effects.Select(fx => fx.Instantiate()).ToList(); //this creates a new list

            foreach (var effectSO in effectsSO) { //Do this after object has been copied. Otherwise we'll add the SO's to the original instance and accumulate a bunch of them over time.
                gaCopy.effects.Add(effectSO.ge.Instantiate()); //Instantiate all effects out of their SO
            }
            gaCopy.effectsSO.Clear(); //absolutely needed. remove SOs from new instance. So if it gets instantiated again, it wont add more copies of SO ges.

            //When jsonUtility serializes GA, it doesnt serialize the types of our derived classes. e.g. child classes will be deserialized as base classes. Also serialization is VERY VERY SLOW.
            //Manual copying
            gaCopy.name = this.name;
            gaCopy.abilityTags = this.abilityTags;
            gaCopy.level = this.level;
            gaCopy.isActive = false;
            gaCopy.cuesTags = this.cuesTags;

            if (cooldown != null) {
                gaCopy.CreateCoolDownGE(this.cooldown.durationValue);
                gaCopy.cooldown.gameplayEffectTags = cooldown.gameplayEffectTags;
            }
            if (cost != null) {
                gaCopy.CreateCostGE(cost.modifiers, cost.durationType, cost.durationValue);
                gaCopy.cost.gameplayEffectTags = cost.gameplayEffectTags;
            }

            //Copy tags to new instance
            gaCopy.abilityTags = abilityTags; //ability tags are not being instantiated, this is passing them by reference.
            if (!abilityTags.initialized) {
                gaCopy.abilityTags.FillTags(gaCopy); //Once instantiated, fill the SOs
                gaCopy.abilityTags.ClearStrings();
            }

            return gaCopy;
        }

        /// <summary>
        /// Additional network serialization for inherited classes
        /// </summary>
        public virtual void SerializeAdditionalData() { }
        /// <summary>
        /// Additional network serialization for inherited classes
        /// </summary>
        public virtual void DeserializeAdditionalData() { }

        public virtual void PreActivate(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            // Debug.Log($"GA PreActivate: {name} src {source} tgt {target}");
            isActive = true;
            timeActivated = Time.time;

            this.source = source;
            this.target = target;
            source.OnGameplayAbilityPreActivate?.Invoke(this, activationGUID);
        }
        public virtual void Activate(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            //Apply Cooldown
            if (cooldown != null && cooldown.durationValue > 0f) {
                source.ApplyGameplayEffect(source, source, cooldown, activationGUID);
            }
            //Apply cost
            if (cost != null && cost.modifiers.Count > 0) {
                source.ApplyGameplayEffect(source, source, cost, activationGUID);
            }
            //Apply tags from this GA
            // this.activationGUID = activationGUID;
            foreach (GameplayTag tag in abilityTags.CancelAbilitiesWithTags) { //Other GameplayAbilities that grant these GameplayTags (ActivationOwned) will be cancelled when this GameplayAbility is activated.
                foreach (GameplayAbility ga in source.grantedGameplayAbilities) {
                    if (ga.isActive && ga.abilityTags.ActivationOwnedTags.Contains(tag)) {
                        ga.DeactivateAbility(activationGUID);
                    }
                }
            }
        }
        public virtual void PostActivate(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            if (source.invokeEventsGA) source.OnGameplayAbilityActivated?.Invoke(this, activationGUID);
        }

        public virtual void CommitAbility(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            PreActivate(source, target, activationGUID);
            Activate(source, target, activationGUID);
            PostActivate(source, target, activationGUID);
        }

        public virtual void DeactivateAbility(string activationGUID = null) {
            if (!isActive) return;
            // Debug.Log($"DeactivateAbility: {name} activationGUID: {activationGUID}");
            isActive = false;
            //Remove the tags applied by this GA 
            if (source.invokeEventsGA) source.OnGameplayAbilityDeactivated?.Invoke(this, activationGUID);
        }

        public float GetCooldownRemaining() {
            if (cooldown == null) return 0;
            return Math.Clamp((timeActivated + cooldown.durationValue) - Time.time, 0, 100000f);
        }

        public GameplayEffect CreateCoolDownGE(float durationValue, GameplayTag cooldownTag = null, string cooldownName = "Cooldown") {
            cooldown = new GameplayEffect() {
                durationType = GameplayEffectDurationType.Duration,
                name = cooldownName + " " + name,
                durationValue = durationValue,
            };
            if (cooldownTag != null) {
                cooldown.gameplayEffectTags = new GameplayEffectTags() {
                    GrantedTags = new List<GameplayTag>() { cooldownTag }
                };
            }
            // Debug.Log("cooldown from" + name + " : " + JsonUtility.ToJson(cooldown)); //This causes some weirds error when exiting playmode. related to coroutine usage.
            return cooldown;
        }

        public GameplayEffect CreateCostGE(List<Modifier> modifiers, GameplayEffectDurationType durationType = GameplayEffectDurationType.Instant, float duration = 0, GameplayTag costTag = null, string costName = "Cost") {
            if (costTag == null) costTag = GameplayTagLibrary.Instance.GetByName("Ability.Cost"); // GameplayTags.library.GetByName("Ability.Cost");

            var createdCost = new GameplayEffect() {
                durationType = durationType,
                name = costName + " " + name,
                durationValue = duration,
                gameplayEffectTags = new GameplayEffectTags() {
                    GrantedTags = new List<GameplayTag>() { costTag }
                }
            };
            if (costTag != null) {
                createdCost.gameplayEffectTags = new GameplayEffectTags() {
                    GrantedTags = new List<GameplayTag>() { costTag }
                };
            }
            // Debug.Log("createdCost from" + createdCost + " : " + JsonUtility.ToJson(createdCost)); //This causes some weirds error when exiting playmode. related to coroutine usage.
            createdCost.modifiers = modifiers;
            cost = createdCost;
            return createdCost;
        }

        /// <summary>
        /// You can override this and make additional checks if needed. 
        /// e.g. SERVER CHECKS FOR TARGET ASC's CONNECTION.
        /// e.g. CHECK DISTANCE, CHECK LOS...
        /// </summary>
        public virtual bool CanActivateAbility(AbilitySystemComponent src, AbilitySystemComponent target, string activationGUID, bool sendFailedEvent) {
            //Cannot activate if already active
            if (this.isActive) {
                if (src.logging || target.logging) Debug.Log($"{this.name} is already active.");
                if (sendFailedEvent) src.OnGameplayAbilityFailedActivation?.Invoke(this, activationGUID, ActivationFailure.ALREADY_ACTIVE);
                return false;
            }

            //Check cooldown
            if (GetCooldownRemaining() > 0) { // Cooldown without tags...is it ok? Maybe. How can we track it for things that need to know if this ability is happening? e.g. isRolling is needed for Jumping 
                if (src.logging || target.logging) Debug.Log($"{this.name} is on Cooldown. Time Remaining: {GetCooldownRemaining()}");
                if (sendFailedEvent) src.OnGameplayAbilityFailedActivation?.Invoke(this, activationGUID, ActivationFailure.COOLDOWN);
                return false;
            }

            //Check Cost
            if (!CheckCost(src)) {
                if (sendFailedEvent) src.OnGameplayAbilityFailedActivation?.Invoke(this, activationGUID, ActivationFailure.COST);
                return false;
            }
            //If no constraint prevents activation, we activate
            return true;
        }

        public virtual bool CheckCost(AbilitySystemComponent src) {
            //Override this if your cost is not a character attribute resource. (e.g. ammo in the inventory system, instead of mana.)
            //Check Cost
            if (this.cost == null) return true;
            //Attribute must be a resource... not a stat?

            //Check if has all attributes
            List<AttributeName> costAttributes = new List<AttributeName>();
            this.cost.modifiers.ForEach(modifier => costAttributes.Add(modifier.attributeName));

            List<AttributeName> presentAttributes = new List<AttributeName>();
            src.attributes.ForEach(attribute => presentAttributes.Add(attribute.attributeName));

            foreach (AttributeName costAttributeName in costAttributes) {
                // Debug.Log($"[{string.Join(", ", presentAttributes)}].Contains({costAttributeName}) {presentAttributes.Contains(costAttributeName)}");
                if (presentAttributes.Contains(costAttributeName) == false) {
                    if (src.logging || target.logging) Debug.Log($"ASC presentAttributes DOES NOT contain costAttributeName: {costAttributeName}");
                    return false;
                }
            }

            //Check if attributes can pay cost
            foreach (AttributeName costAttributeName in costAttributes) {
                // Debug.Log($"[{string.Join(", ", presentAttributes)}].Contains({costAttributeName}) {presentAttributes.Contains(costAttributeName)}");
                Attribute presentAttribute = src.attributes.Find((presentAttribute) => presentAttribute.attributeName == costAttributeName);
                Modifier costModifier = this.cost.modifiers.Find((costAttribute) => costAttribute.attributeName == costAttributeName);
                // Debug.Log($"presentAttribute.attributeName: {presentAttribute.attributeName}  costModifier.modifierOperator {costModifier.modifierOperator} costModifier.value {costModifier.value}");


                if (presentAttribute.baseValue < -costModifier.GetValue()) { //RESOURCE ONLY
                    if (src.logging || target.logging) Debug.Log($"CANT PAY GA COST - ASC {presentAttribute.attributeName} {presentAttribute.baseValue} cannot pay {costAttributeName} {costModifier.GetValue()}");
                    return false;
                }
            }
            return true;
        }

        /// <summary> Returns a list of gameplay tags, any ability that grants them will have its activation blocked </summary>
        public List<GameplayTag> GetBlockedAbilitiesTags(AbilitySystemComponent src) {
            var blockedAbilityTags = new List<GameplayTag>();
            foreach (GameplayAbility ga in src.grantedGameplayAbilities) {
                if (ga.isActive) {
                    blockedAbilityTags.AddRange(ga.abilityTags.BlockAbilitiesWithTags);
                }
            }
            // Debug.Log($"GetBlockedAbilitiesTags: [{string.Join(", ", blockedAbilityTags)}]");
            return blockedAbilityTags;
        }
    }

    /// <summary>
    /// Reason for an activation failure. You can add new reasons when implementing your own abilities.
    /// </summary>
    public enum ActivationFailure {
        ALREADY_ACTIVE,
        COST,
        COOLDOWN,
        TAGS_SOURCE_FAILED,
        TAGS_TARGET_FAILED,
        TAGS_BLOCKED,
        OTHER,
    }

    /// <summary>
    /// Tags for Gameplay Abilities
    /// </summary>
    [Serializable]
    public class AbilityTags {//Block and Cancel and similar to ActivationIgnored, but instead of being from this GA it is related to other GAs
        /// <summary> While this Ability is active/executing, the owner of the Ability will be granted this set of Tags. ( ActivationOwnedTags) </summary>
        [Tooltip("While this Ability is active/executing, the owner of the Ability will be granted this set of Tags.")]
        [SerializeField] public List<GameplayTag> ActivationOwnedTags = new List<GameplayTag>();

        /// <summary> Tags that describe the GameplayAbility. They do not do any function on their own and serve only the purpose of describing the GameplayEffect. </summary>
        [Tooltip("GameplayTags that the GameplayAbility owns. These are just GameplayTags to describe the GameplayAbility")]
        [SerializeField] public List<GameplayTag> DescriptionTags = new List<GameplayTag>();

        /// <summary> Active Gameplay Abilities (on the same character) that have these tags will be cancelled.
        /// Cancels any already-executing Ability with Tags matching the list provided while this Ability is executing. </summary>
        [Tooltip("Active Gameplay Abilities (on the same character) that have these tags will be cancelled. Cancels any already-executing Ability with Tags matching the list provided while this Ability is executing")]
        [SerializeField] public List<GameplayTag> CancelAbilitiesWithTags = new List<GameplayTag>();
        /// <summary> Gameplay Abilities that have these tags will be blocked from activating on the same character</summary>
        [Tooltip("Gameplay Abilities that have these tags will be blocked from activating on the same character. (blocking others)")]
        [SerializeField] public List<GameplayTag> BlockAbilitiesWithTags = new List<GameplayTag>(); //Gameplay Abilities that have these tags will be blocked from activating on the same character

        /// <summary>If any of these tags IS NOT present on source ASC, this ability won't be activated.</summary>
        [Tooltip("If any of these tags IS NOT present on source ASC, this ability won't be activated.")]
        [SerializeField] public List<GameplayTag> SourceTagsRequired = new List<GameplayTag>(); //The Ability can only be activated if the activating Component has all Required Tags and none of Ingnored Tags.
        /// <summary> If any of these tags IS present on source ASC, this ability won't be activated.</summary>
        [Tooltip("If any of these tags IS present on source ASC, this ability won't be activated. (self ignoring)")]
        [SerializeField] public List<GameplayTag> SourceTagsForbidden = new List<GameplayTag>();

        /// <summary> If any of these tags IS NOT present on target, this ability won't be activated. </summary>
        [Tooltip("If any of these tags IS NOT present on target, this ability won't be activated. ")]
        [SerializeField] public List<GameplayTag> TargetTagsRequired = new List<GameplayTag>(); //The Ability can only be activated if the activating Component has all Required Tags and none of Ingnored Tags. </summary>
        /// <summary> If any of these tags IS present on target, this ability won't be activated. </summary>
        [Tooltip("If any of these tags IS present on target, this ability won't be activated.")]
        [SerializeField] public List<GameplayTag> TargetTagsForbidden = new List<GameplayTag>();


        [HideInInspector] public bool initialized = false;
        [ReadOnly][HideInInspector] public List<string> stringActivationOwnedTags = new List<string>();
        [ReadOnly][HideInInspector] public List<string> stringDescriptionTags = new List<string>();
        [ReadOnly][HideInInspector] public List<string> stringCancelAbilitiesWithTags = new List<string>();
        [ReadOnly][HideInInspector] public List<string> stringBlockAbilitiesWithTags = new List<string>();

        [ReadOnly][HideInInspector] public List<string> stringSourceTagsRequired = new List<string>();
        [ReadOnly][HideInInspector] public List<string> stringSourceTagsForbidden = new List<string>();
        [ReadOnly][HideInInspector] public List<string> stringTargetTagsRequired = new List<string>();
        [ReadOnly][HideInInspector] public List<string> stringTargetTagsForbidden = new List<string>();

        [ReadOnly][HideInInspector] public List<string> string_CueTags = new List<string>();

        public void FillTags(GameplayAbility ga) {
            initialized = true;
            // Debug.Log($"GA {ga.name} - GetAllTags GrantedTags: [{string.Join(", ", ActivationOwnedTags.Select(x => x.name))}]  string_GrantedTags: [{string.Join(", ", stringActivationOwnedTags.Select(x => x))}]");
            ActivationOwnedTags = ActivationOwnedTags.Union(GameplayTagLibrary.Instance.GetByNames(stringActivationOwnedTags)).ToList();
            DescriptionTags = DescriptionTags.Union(GameplayTagLibrary.Instance.GetByNames(stringDescriptionTags)).ToList();
            CancelAbilitiesWithTags = CancelAbilitiesWithTags.Union(GameplayTagLibrary.Instance.GetByNames(stringCancelAbilitiesWithTags)).ToList();
            BlockAbilitiesWithTags = BlockAbilitiesWithTags.Union(GameplayTagLibrary.Instance.GetByNames(stringBlockAbilitiesWithTags)).ToList();

            SourceTagsRequired = SourceTagsRequired.Union(GameplayTagLibrary.Instance.GetByNames(stringSourceTagsRequired)).ToList();
            SourceTagsForbidden = SourceTagsForbidden.Union(GameplayTagLibrary.Instance.GetByNames(stringSourceTagsForbidden)).ToList();
            TargetTagsRequired = TargetTagsRequired.Union(GameplayTagLibrary.Instance.GetByNames(stringTargetTagsRequired)).ToList();
            TargetTagsForbidden = TargetTagsForbidden.Union(GameplayTagLibrary.Instance.GetByNames(stringTargetTagsForbidden)).ToList();

            ga.cuesTags = ga.cuesTags.Union(GameplayTagLibrary.Instance.GetByNames(string_CueTags)).ToList();

        }

        public void FillStrings(GameplayAbility ga) {
            stringActivationOwnedTags = ActivationOwnedTags.Select(tag => tag.name).ToList();
            stringDescriptionTags = DescriptionTags.Select(tag => tag.name).ToList();
            stringCancelAbilitiesWithTags = CancelAbilitiesWithTags.Select(tag => tag.name).ToList();
            stringBlockAbilitiesWithTags = BlockAbilitiesWithTags.Select(tag => tag.name).ToList();
            stringSourceTagsRequired = SourceTagsRequired.Select(tag => tag.name).ToList();
            stringSourceTagsForbidden = SourceTagsForbidden.Select(tag => tag.name).ToList();
            stringTargetTagsRequired = TargetTagsRequired.Select(tag => tag.name).ToList();
            stringTargetTagsForbidden = TargetTagsForbidden.Select(tag => tag.name).ToList();

            string_CueTags = ga.cuesTags.Select(tag => tag.name).ToList();
        }


        public void ClearTags(GameplayAbility ga) {
            ActivationOwnedTags.Clear();
            DescriptionTags.Clear();
            CancelAbilitiesWithTags.Clear();
            BlockAbilitiesWithTags.Clear();
            SourceTagsRequired.Clear();
            SourceTagsForbidden.Clear();
            TargetTagsRequired.Clear();
            TargetTagsForbidden.Clear();

            ga.cuesTags.Clear();
        }

        public void ClearStrings() {
            stringActivationOwnedTags.Clear();
            stringDescriptionTags.Clear();
            stringCancelAbilitiesWithTags.Clear();
            stringBlockAbilitiesWithTags.Clear();
            stringSourceTagsRequired.Clear();
            stringSourceTagsForbidden.Clear();
            stringTargetTagsRequired.Clear();
            stringTargetTagsForbidden.Clear();


            string_CueTags.Clear();
        }
    }
}