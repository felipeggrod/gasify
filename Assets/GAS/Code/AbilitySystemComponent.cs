using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using System;

namespace GAS {
    /// <summary>
    /// ASC for short, frequently also named AbilitySystemCHARACTER. This is the container that holds all the gears of the GameplayAbilitySystem. It represents a single character/entity with its attributes and abilities.
    /// </summary>
    [Serializable]
    public class AbilitySystemComponent : MonoBehaviour {
        public GroupASC initialData;
        [ReadOnly] public Dictionary<string, Attribute> attributesDictionary = new(); //Much faster search performance!!!
        public List<Attribute> attributes = new List<Attribute>(); // Adding attributes is only supported in edit mode. We can't add attribute at runtime for now.
        public Action<AttributeName, float, float, GameplayEffect> OnAttributeChanged;
        public Action<Attribute, GameplayEffect> OnPreAttributeChange;
        [SerializeReference] public List<AttributeProcessor> attributesProcessors = new List<AttributeProcessor>();


        [SerializeReference] public List<GameplayAbility> grantedGameplayAbilities = new List<GameplayAbility>();
        public Action<GameplayAbility, string> OnGameplayAbilityPreActivate, OnGameplayAbilityActivated, OnGameplayAbilityTryActivate, OnGameplayAbilityDeactivated;
        public Action<GameplayAbility, string, ActivationFailure> OnGameplayAbilityFailedActivation;
        public Action<GameplayAbility> OnGameplayAbilityGranted, OnGameplayAbilityUngranted;

        public List<GameplayEffect> appliedGameplayEffects;
        public Action<GameplayEffect> OnGameplayEffectApplied, OnGameplayEffectRemoved;
        public Action<List<GameplayEffect>> OnGameplayEffectsChanged;

        public List<GameplayTag> tags;
        public Action<List<GameplayTag>, AbilitySystemComponent, AbilitySystemComponent, string> OnTagsChanged; //returns the new tags list when any change happens 
        public Action<List<GameplayTag>, AbilitySystemComponent, AbilitySystemComponent, string> OnTagsInstant; //tags that were applied by an instant GE, they are a single instantaneous event.

        public float level = 1; // This can be used to amplify the numbers of effects. Logic must be manually implemented for each game's progression design.

        public List<GameplayCue> instancedCues = new List<GameplayCue>();

        public bool logging = false;
        // public UnityEvent unityEvent = new UnityEvent();

        [ReadOnly] public bool invokeEventsGA = true;
        [ReadOnly] public bool invokeEventsGE = true;

        /// <summary> If an ability can't be activated immediately, keeps retrying it for a moment.</summary>
        public bool inputBuffering = true;
        private float inputBufferDurationSeconds = .16f;

        public void Awake() {
            initialData.AddAttributes(this);
            initialData.AddAttributeProcessors(this);
            initialData.GrantAbilities(this); //We need to wait the GE events to be registered first (e.g. passive abilities activate on instantiation, we need it so initial Granted GAs trigger ge events).


            ResetStatsAttributesValues();

            // EVENT HANDLES
            //Trigger GameplayEffects Handles
            OnGameplayEffectApplied += (ge) => OnGameplayEffectsChanged?.Invoke(appliedGameplayEffects);
            OnGameplayEffectRemoved += (ge) => OnGameplayEffectsChanged?.Invoke(appliedGameplayEffects);

            //Trigger Attribute Change Handles
            attributes.ForEach(x => x.OnPostAttributeChange += (attributeName, oldValue, newValue, ge) => { OnAttributeChanged?.Invoke(attributeName, oldValue, newValue, ge); });
            attributes.ForEach(x => x.OnPreAttributeChange += (att, ge) => { OnPreAttributeChange?.Invoke(att, ge); });
            attributes.ForEach(x => { x.name = x.attributeName.name; attributesDictionary.Add(x.attributeName.name, x); });

            // TAGS
            //Update gameplayEffectsTags
            OnGameplayEffectApplied += UpdateTagsOnEffectChange;
            OnGameplayEffectRemoved += UpdateTagsOnEffectChange;

            //Process GA tags
            OnGameplayAbilityActivated += UpdateTagsOnGameplayAbilityActivate;
            OnGameplayAbilityDeactivated += UpdateTagsOnGameplayAbilityDeactivate;

            OnGameplayEffectApplied += TriggerOnTagsAdded;

            //Register AttributeProcessors
            attributesProcessors.ForEach(x => OnPreAttributeChange += (att, ge) => { x.PreProcess(att, ge, this); }); //Pre Processing
            attributesProcessors.ForEach(x => OnAttributeChanged += (attributeName, oldValue, newValue, ge) => { x.PostProcessed(attributeName, oldValue, newValue, ge); });


            GameplayCueManager.Register(this);
        }

        private void Start() {
            // private void Start() {
            InitializeAttributesListeners();

            //Loggers
            if (logging) {
                // Debug.Log($"LOGGING {this.name}");
                OnPreAttributeChange += (attribute, ge) => { Debug.Log($"OnPreAttributeChange: {attribute.attributeName.name} {ge?.name}"); };
                OnAttributeChanged += (attributeName, oldValue, newValue, ge) => { Debug.Log($"{attributeName.name} {oldValue} -> {newValue} / ge: {ge?.name}"); };
                // OnTagsChanged += (newTags, src, tgt) => Debug.Log($"[TAGS] OnTagsChanged! newTags: [{string.Join(", ", newTags.Select(x => x.name))}]");
                OnTagsInstant += (newTags, src, tgt, applicationGUID) => Debug.Log($"[TAGS] OnTagsInstant! tags: [{string.Join(", ", newTags.Select(x => x.name))}]");

                // OnGameplayEffectsChanged += (ges) => {
                //     var geNames = new List<string>();
                //     ges.ForEach(ge => geNames.Add(ge.name));
                //     // Debug.Log($"GameplayEffectsChanged, appliedGEs: {new JsonListWrapper<string>(geNames).ToJson()}");
                //     Debug.Log($"GameplayEffectsChanged, appliedGEs: [{string.Join(", ", geNames)}]");
                // };
                // OnGameplayEffectApplied += (newGE) => Debug.Log($"OnGameplayEffectApplied ge: {newGE.name} ");
                // OnGameplayEffectRemoved += (removedGE) => Debug.Log($"OnGameplayEffectRemoved ge: {removedGE.name}");
                // OnGameplayAbilityFailedActivation += (ga, activationGUID, failureCause) => Debug.Log($"GA Failed Activation: {ga.name} {failureCause}");
            }
            OnGameplayAbilityFailedActivation += (ga, activationGUID, failureCause) => Debug.Log($"GA Failed Activation: {ga.name} {failureCause}");

        }

        private void OnDestroy() {
            foreach (var ga in grantedGameplayAbilities) if (ga.isActive) ga.DeactivateAbility(); // Cleans up toggle/passive abilities.
        }

        //Tag events
        public void UpdateTagsOnEffectChange(GameplayEffect ge) {
            TagProcessor.UpdateTags(ge.source, ge.target, ref tags, appliedGameplayEffects, grantedGameplayAbilities, OnTagsChanged, ge.applicationGUID);
        }
        public void UpdateTagsOnGameplayAbilityActivate(GameplayAbility ga, string activationGUID) { //This needs to be a declared function, because we must remove this subscription for non owner client objects on multiplayer.
            TagProcessor.UpdateTags(ga.source, ga.target, ref tags, appliedGameplayEffects, grantedGameplayAbilities, OnTagsChanged, activationGUID);
        }
        public void UpdateTagsOnGameplayAbilityDeactivate(GameplayAbility ga, string activationGUID) { //This needs to be a declared function, because we must remove this subscription for non owner client objects on multiplayer.
            TagProcessor.UpdateTags(ga.source, ga.target, ref tags, appliedGameplayEffects, grantedGameplayAbilities, OnTagsChanged, activationGUID);
        }

        public void TriggerOnTagsAdded(GameplayEffect appliedGE) {
            if (appliedGE.gameplayEffectTags.GrantedTags.Count == 0) return;
            if (appliedGE.durationType == GameplayEffectDurationType.Instant) OnTagsInstant?.Invoke(appliedGE.gameplayEffectTags.GrantedTags, appliedGE.source, appliedGE.target, appliedGE.applicationGUID);

        }

        public float GetAttributeValue(string attNameString) {
            if (attributesDictionary.TryGetValue(attNameString, out var attribute)) {
                return attribute.GetValue();
            }
            Debug.LogWarning($"No Attribute named {attNameString}");
            return 0;
        }
        public float GetAttributeValue(AttributeName attName) {
            return GetAttributeValue(attName.name);
        }

        public void ResetStatsAttributesValues() { foreach (var att in attributes) att.currentValue = att.baseValue; }
        public void InitializeAttributesListeners() { foreach (var att in attributes) att.OnPostAttributeChange?.Invoke(att.attributeName, 0, att.baseValue, null); }
        public void ApplyAttributeModifiersValues(GameplayEffect ge) { attributes.ForEach(att => att.ApplyModifiers(ge)); }

        //Recalculate GE AttributeModifiers from all appliedGEs
        public void RefreshAttributesModifiers(GameplayEffect ge) {
            //Clean all attributeModifiers before rebuilding values
            attributes.ForEach(att => att.modification.Clear());
        }



        /// <summary> Grants ability and returns the newly instantiated GA </summary>
        public GameplayAbility GrantAbility(GameplayAbility ga) {
            GameplayAbility gaCopy = ga.Instantiate(this);
            grantedGameplayAbilities.Add(gaCopy);
            OnGameplayAbilityGranted?.Invoke(gaCopy);

            // if (gaCopy is PassiveAbility passiveAbility) { passiveAbility.CommitAbility(this, this, Guid.NewGuid().ToString()); } //Needed for passive ability. I dont like to put it here, BUT, it's the cleanest approach so far.
            return gaCopy;
        }

        public void UngrantAbilityByTag(GameplayTag tag) {
            var removeIndexes = new List<int>();
            grantedGameplayAbilities.ForEach(ga => {
                if (ga.abilityTags.DescriptionTags.Contains(tag)) {
                    removeIndexes.Add(grantedGameplayAbilities.IndexOf(ga));
                }
            });
            removeIndexes.ForEach(i => UngrantAbility(i));
        }
        [EasyButtons.Button]
        public void UngrantAbility(int index) {
            UngrantAbility(grantedGameplayAbilities[index]);
        }

        public void UngrantAbility(string guid) {
            UngrantAbility(grantedGameplayAbilities.Find(x => x.guid == guid));
        }

        /// <summary> Ungrants an ability, this is the correct way to remove the ability so we can clean it's effects. </summary>
        public void UngrantAbility(GameplayAbility ga) {
            ga.DeactivateAbility(null);
            grantedGameplayAbilities.Remove(ga);
            OnGameplayAbilityUngranted?.Invoke(ga);
        }

        public List<GameplayTag> GetAllTags() {
            return tags;
        }

        public void TryActivateAbility(string abilityName, AbilitySystemComponent target) {
            GameplayAbility ga = grantedGameplayAbilities.Find(ga => ga.name == abilityName);

            if (ga == null) ga = grantedGameplayAbilities.Find(ga => ga.name.Contains(abilityName));
            if (ga == null) {

                Debug.Log($"No granted Ability named {abilityName}");
                return;
            }
            TryActivateAbility(ga, target, null);
        }

        [EasyButtons.Button]
        public void TryActivateAbility(int index, AbilitySystemComponent target) {
            if (index >= grantedGameplayAbilities.Count) {
                Debug.Log($"No granted Ability at given index {grantedGameplayAbilities}");
                return;
            }
            TryActivateAbility(grantedGameplayAbilities[index], target);
        }

        public void TryActivateAbility(string guid, AbilitySystemComponent target, string activationGUID) {
            GameplayAbility ga = grantedGameplayAbilities.Find(ga => ga.guid == guid);
            if (ga == null) {
                Debug.Log($"No granted Ability with guid {guid}");
                return;
            }
            TryActivateAbility(ga, target, activationGUID);
        }

        public async void TryActivateAbility(GameplayAbility ga, AbilitySystemComponent target, string activationGUID = null) {//activationGUID is only used when networking. It is an unique identifier (uuid) for a given ability activation attempt
            ga.source = this;
            ga.target = target;
            ga.activationGUID = activationGUID;

            // If doing networking, this is event that sends the server CMD. 
            OnGameplayAbilityTryActivate?.Invoke(ga, activationGUID);

            if (ga.isActive) { //Ability Toggle behaviour
                ga.DeactivateAbility(ga.activationGUID);
                return;
            }

            await InputBuffering(ga, target, activationGUID);

            if (!ga.CanActivateAbility(this, target, activationGUID, true)) return;

            //Commit Ability Activation.
            ga.CommitAbility(this, target, ga.activationGUID);
        }

        public async Task InputBuffering(GameplayAbility ga, AbilitySystemComponent target, string activationGUID = null) {
            //TODO: Add a check to know if the ga is already on the buffer. If it is, stop PREVIOUS TryActivate buffer or reset its timer. So we have only 1 checking loop for it at any given time.
            float finalTime = Time.realtimeSinceStartup + inputBufferDurationSeconds;
            while (!ga.isActive && Time.realtimeSinceStartup < finalTime && !ga.CanActivateAbility(this, target, activationGUID, false)) {
                // Debug.Log($"InputBuffering - ga: {ga.name} CanActivate? {!ga.CanActivateAbility(this, target, activationGUID, false)} ga.isActive {ga.isActive} buffer remaining time: {finalTime - Time.realtimeSinceStartup} ");
                await Task.Delay(10);
            }
        }

        /// <summary> Apply any GE to an ASC. ApplicationGUID is used for client side prediction on multiplayer/networking, ignore it for single-player. </summary>
        public GameplayEffect ApplyGameplayEffect(AbilitySystemComponent source, AbilitySystemComponent target, GameplayEffect ge, string applicationGUID = null) {
            if (logging) Debug.Log($"ASC ApplyGameplayEffect {ge.name} {this.name} applicationGUID: {applicationGUID} data: {JsonUtility.ToJson(ge, true)}");
            //References
            ge.source = source;
            ge.target = target;
            ge.applicationGUID = applicationGUID;

            if (!TagProcessor.CheckApplicationTagRequirementsGE(this, ge, tags)) {
                if (logging) Debug.Log($"GE: {ge.name} couldnt be applied on this ASC. Failed application tag requirements");
                return null;
            }
            if (ge.chanceToApply < 1f) {
                if (!(UnityEngine.Random.Range(0f, 1f) <= ge.chanceToApply)) return null;
            }

            // Instantiate a new GE, to avoid references to the original GE
            GameplayEffect geCopy = ge.Instantiate();
            // Debug.Log($"{this.name} ApplyGameplayEffect geCopy.name {geCopy.name} applicationGUID: {applicationGUID} data: {JsonUtility.ToJson(ge, true)}");
            if (geCopy.durationType != GameplayEffectDurationType.Instant) appliedGameplayEffects.Add(geCopy);

            switch (ge.durationType) {
                case GameplayEffectDurationType.Infinite:
                case GameplayEffectDurationType.Duration:
                    Debug.Log("[FREE VERSION] Duration and Infinite GameplayEffects are not fully available on the free version. Check GASify on the Assetstore for more options.");
                    RemoveDurationGE(geCopy);
                    break;
                case GameplayEffectDurationType.Instant:
                    ApplyInstantGameplayEffect(geCopy);
                    break;
            }
            if (invokeEventsGE) OnGameplayEffectApplied?.Invoke(geCopy);
            return geCopy;
        }
        public async void RemoveDurationGE(GameplayEffect ge) {
            await Task.Delay(1000);
            appliedGameplayEffects.Remove(ge);
            OnGameplayEffectRemoved?.Invoke(ge);
        }

        List<Modifier> modifiersToProcess = new List<Modifier>();
        public void ApplyInstantGameplayEffect(GameplayEffect ge) {
            //Get the modifiers list from the GE itself
            modifiersToProcess.Clear();
            modifiersToProcess.AddRange(ge.modifiers);

            //Apply the modifiers to attribute values
            foreach (Attribute attribute in attributes) {
                foreach (Modifier modifier in modifiersToProcess) {
                    if (attribute.attributeName == modifier.attributeName) {
                        attribute.ApplyModifierAsResource(modifier, ge);
                    }
                }
            }
        }
    }

}