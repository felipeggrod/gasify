using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using GAS;
using EasyButtons;
using System.Threading.Tasks;

//  The networking for the ASC needs 2 things: Replication and Prediction. 
//  Replication means that whatever happens on the server is replicated/mirrored/copied back to the clients. 
//  Prediction means that whatever the client does, it will predict changes immediately, and send the action for the server to process. 
//       The server is the source of truth. So if a prediction is incorrect, it will be reconciliated/corrected when the server replicates to the client what really happened.
//  
//  GA:
//  GA REPLICATION:
//       On the GA, we only replicate the GA's state. Meaning, if it is active or not. 
//       The GEs, tags and attribute changes are replicated by their event triggers. (Although we trigger them on local client's prediction, the replication of the GA object itself do not trigger them.)
//       GAs from other clients (non local player) have their GEs stripped away and so they don't invoke events on GA activation (e.g. OnTagsChanged), so they dont trigger local attribue/tags changes when activation is replicated but still execute its custom code (e.g. spawning projectile).
// 
//  GA PREDICTION:
//       The client activates an ability, effects, tags and attributes are modified immediately. An activationGUID (a.k.a Prediction Key) is generated and added to a buffer queue. 
//       The id (guid) of the activated ability and activationGUID is sent to the server. 
//       The server activates the ability on its own simulation, assigns it the same activationGUID and send that to all clients for replication.
//       The client receives the replication, it checks it the activationGUID is present in the buffer queue. If present, dont replicate (it was already predicted), else replicate it. 
// 
// 
//  TAGS, ATTRIBUTES PREDICTION:
//       When a client activates an ability, tags and attributes are modified immediately.
//       The new state for tags and attributes are added to each corresponding buffer queue.
//       The client receives the replication state from the server:
//           if the received state is in the buffer queue, we remove all previous state values until that one.
//           if the received state is NOT in the buffer queue, we clear the buffer queue and set the current state value to the one received. (e.g. our prediction sequence were wrong, we snap to the latest value received from the server)
// 
//   GE REPLICATION:
//       
//   GE PREDICTION
// 
//  NOTES:
//       TriggerAbility should not be triggered on the client, only in the server and replicated to clients.
//           We can do this by not triggering the Tag events in client, only on server. 
//           Client gets triggers for cues and also get tags lists synced, it just doesnt get them events. 
//           Clients should only trigger the events for locally predicted activations (GA client Activation), and not for received activations. 
//           
//       Gampley Cues networking, The source and target ASC in cue data can be NULL on client if, for example, the GE was applied by another ASC outside of the Area of Interest (Network Culling Area). 
// 
//  Disclaimer: Attribute prediction works with BasicModifier, AttributeBasedModifier, but is not guaranteed to work with other modifiers
// 





// GA Activate(AdditionalNetworkingActivationData = null), allows custom abilities to be synchronized e.g. Send rotation for Shoot Ability activation


//  (x) REPLICATE GAs (objects)
//  (x) REPLICATE attributes: sync on OnAttributeChange on server, also calls on client's asc.OnChangeAttribute.Invoke(newValue)
//  (x) REPLICATE sync tags: OnTAGSChanged, refresh tag list. EASY
//  (x) REPLICATE cues
//  (x) REPLICATE ges: (OnGEChange? NO, use mirror syncList), rpc to client. (send whole object vs. send SO reference boolean?)
//  (x) REPLICATE GA OnGrant/OnUngrant OnActivate/OnDeactivate cooldown/cost rpc from other players. should change bool and play cues, but not apply ges?
//  (x) REPLICATE Triggered, and tag events 
//  Replication Modes: 1. FULL - GE replication to everyone 2. MIXED - GE replication only to owner, tags & cues to everyone else 3. MINIMAL - tags a& cues to everyone.


//  UNREAL GAS PREDICTION:
//  What is predicted:
//  (x) Ability activation
//  (x) Triggered Events
//  (x) GameplayEffect application:
//  (x) Attribute modification (EXCEPTIONS: Executions do not currently predict, only attribute modifiers)
//  (x) GameplayTag modification
//  Gameplay Cue events (both from within predictive gameplay effect and on their own)
//  (Ability Tasks) Montages
//  (NOT DOING) Movement (built into UE5 UCharacterMovement)
//  
//  What is not predicted:
//  GameplayEffect removal
//  GameplayEffect periodic effects (dots ticking) -> if we predict a periodic GE with another non-predicted perioc GE on the server, a mismatch in sequence of ticks will cause crazy jittery in the attribute.

// UE DOCS ON GAS NETWORKING:
//   GA has predictionKey, like ours, but it is associated with all state side effects, not just its own activation. <predictionKey, state> kinda
//   Attributes are treated as a delta (+100 health, -40 mana). this fixes? 'undo' and 'redo' issue.
//   predicted Instant GEs are treated as Infinite GE. This allows 'undo' easily.

// We are doing client prediction + server conciliation
// REVERT/CANCEL EARLY. If the server does not activate the ability, after it was locally predictively activated, we need a way to revert it locally??? or stop it locally??? (Can be solved with a GA state prediction buffer)




//  AUTOMATE TESTS ALREADY??? What we should test:
//  FULL MODE (ga only?):
//   Ability activate/disactivate: 
//   TODO Sync already active abilities for late joiners.
//       Duration/Infinite GE's previously will not be known. We could trigger a single GE Replication on start.
//   TODO Undo on client if activation failed on server
//   Prediction queue/buffer on clients, regardless of ga prediction
//   deal with chance of apply on GEs (if local -> 0,  but the server should trigger the GE and replicate it for us, but how will it replicate if we only replicate GA Activation?) (replicateChanceToApply RPC, finds the ge.guid from ga.guid and apply it)
//   deal with calculations (just remove them? Fixes calculation that handles DMG for example, BUT What about a calculation that changes GE's chance of apply OR GA's cooldown?)
//   how to activate GA's when the source or target is not visible by client (e.g. interest manager, a ASC that is not visible locally, activate its GA on a locally visible target OR a locally visible SRC ASC, not local player, activate its GA on a TGT ASC not visible locally)
//       1. GE replication code's could handle that (SEPARATED MODE) 
//   If you need to just apply GEs without having a specific src ASC for it, you'll need to make a 'global' networked asc. You can activate ges from it on the server, and they will be replicated locally to clients. (e.g. Weapon GEs, you could make the weapon give an passive ability that applies GEs or have the 'global' asc apply just the GE to the asc)
//       
// 

// The bandwidth usage of the ASC networking is already super low. Even for an action game with multiples of GAs and GEs being activated per second, it stays under 1kbps per client. But if you want to go even lower than that, there is a way. You must have all possible GEs in a list, and then make a custom serializer for GameplayEffects that just sends the index of that GE in the list.


namespace GAS {
    public enum ReplicationMode {// Mostly GEs
        GA_Only, // Only 
        GA_and_GE,
        Full, // Every GameplayEffect is replicated to every client.
        Mixed,  // GameplayEffects are only replicated to the owning client. Only GameplayTags and GameplayCues are replicated to everyone.
        Minimal // GameplayEffects are never replicated to anyone. Only GameplayTags and GameplayCues (and Attributes?) are replicated to everyone.
    }

    // / <summary> An additional component to be attached to the asc's GameObject to enable networking replication and prediction. </summary>   
    public class AbilitySystemComponentMirror : NetworkBehaviour {
        // / <summary> Are we doing client side prediction?  </summary>   
        public bool predictGameplayAbilityActivations = true;
        private int predictionBufferDuration = 6_000;

        // / <summary> The asc to be synced.  </summary>   
        public AbilitySystemComponent asc;
        [ReadOnly] public static GenericDictionary<uint, AbilitySystemComponentMirror> ascmDictionary = new();
        public GenericDictionary<uint, AbilitySystemComponentMirror> ascmDictionaryRef = new();
        public static AbilitySystemComponent localPlayerAsc;
        public static AbilitySystemComponentMirror localPlayerAscm;

        [SerializeReference] public readonly SyncDictionary<string, GameplayAbility> syncGrantedAbilities = new();
        public static GenericDictionary<string, GameplayAbility> localAbilityActivationsBuffer = new(); // Prediction Buffer for ability activations

        [SerializeReference] public readonly SyncDictionary<string, float> syncAttributes = new();
        public GenericDictionary<string, Queue<float>> localAttributesBuffer = new(); // Buffers the sequence of changes to attributes. If the sequence is different from sequence of changes received from server the. Clear it and reset to server value.

        [SerializeReference] public readonly SyncDictionary<string, List<GameplayTag>> syncTags = new();

        [SerializeReference] public readonly SyncDictionary<string, GameplayEffect> syncEffects = new();
        public static GenericDictionary<string, GameplayEffect> localEffectsBuffer = new(); // Buffers the sequence of changes to Effects. If the sequence is different from sequence of changes received from the server. Clear it and reset to server value.


        void Start() {
            this.name = this.name + " " + (this.isLocalPlayer ? "[LocalPlayer]" : "[Srvr]") + " netId=" + this.netId;

            // ASC and ASCM CACHES AND CACHE DICTIONARY
            if (asc == null) asc = GetComponent<AbilitySystemComponent>();
            if (this.isLocalPlayer) localPlayerAsc = asc;
            if (this.isLocalPlayer) localPlayerAscm = this;
            if (ascmDictionary.ContainsKey(netId)) ascmDictionary.Clear(); // If the ascm already has this id before we added, it means the client disconnected and reconnected and the dictionary contains previous references. We need to refresh it.
            ascmDictionary.Add(netId, this);
            ascmDictionaryRef = ascmDictionary;

            //  SERVER-SIDED TRIGGERS, if host OR server-only
            if (!isClientOnly) {
                // GA REPLICATION
                asc.grantedGameplayAbilities.ForEach(x => {
                    if (x.guid == null || x.guid == "") { // fix in case the instantiated ability doesnt have a guid yet.
                        Debug.Log($"Trying to sync ga with no guid: {x.name}");
                        x.guid = System.Guid.NewGuid().ToString();
                    }
                    syncGrantedAbilities.Add(x.guid, x);
                });
                asc.OnGameplayAbilityGranted += (GameplayAbility ga) => { syncGrantedAbilities.TryAdd(ga.guid, ga); }; // Adds ASC stuff so it is replicated. We call replication code when you just use the ASC normally as in a singleplayer case.
                asc.OnGameplayAbilityUngranted += (GameplayAbility ga) => { syncGrantedAbilities.Remove(ga.guid); };

                // GA ACTIVATION REPLICATION
                asc.OnGameplayAbilityActivated += (GameplayAbility ga, string activationGUID) => {
                    if (ga.source != this.asc) activationGUID = null; // Only send the activationGUID to the client that generated it.
                    RpcOnGameplayAbilityActivated(
                        ga.guid,
                        ga.source.GetComponent<AbilitySystemComponentMirror>().netId,
                        ga.target.GetComponent<AbilitySystemComponentMirror>().netId,
                        activationGUID
                    );
                };
                asc.OnGameplayAbilityDeactivated += SubscribeOnGameplayAbilityDeactivated;

                // GA PREDICTION
                asc.OnGameplayAbilityFailedActivation += (GameplayAbility ga, string activationGUID, ActivationFailure failure) => {
                    if (ga.source != this.asc) return; // Only send the activation failured to the client that generated it.
                    RpcGameplayAbilityUndo(activationGUID, failure);
                };

                // ATTRIBUTES
                asc.attributes.ForEach(att => syncAttributes.TryAdd(att.attributeName.name, att.GetValue())); // copy attribute list on the asc
                asc.OnAttributeChanged += (attName, oldValue, newValue, ge) => { syncAttributes[attName.name] = newValue; };

                // GE REPLICATION
                asc.appliedGameplayEffects.ForEach(ge => {
                    if (ge.guid == null || ge.guid == "") { // fix in case the instantiated ge doesnt have a guid yet.
                        // Debug.Log($"Trying to sync ga with no guid: {ge.name}");
                        ge.guid = System.Guid.NewGuid().ToString();
                    }
                    syncEffects.Add(ge.guid, ge);
                });
                asc.OnGameplayEffectApplied += (ge) => {
                    if (ge.durationType == GameplayEffectDurationType.Instant) {// We dont need to add an instant GE to syncDict, it is instant, we just trigger it once.
                        RpcSynchronizeGE(ge.guid, ge);
                        return;
                    }
                    syncEffects.Add(ge.guid, ge);
                };
                asc.OnGameplayEffectRemoved += (ge) => {
                    if (NetworkServer.active) syncEffects.Remove(ge.guid);
                };

            } else { //  isClientOnly. CLIENT-SIDED TRIGGERS

                // GA PREDICTION BUFFER
                asc.OnGameplayAbilityTryActivate += LocalTryActivateAbility;
                // GA REPLICATION
                asc.grantedGameplayAbilities.Clear();
                asc.appliedGameplayEffects.Clear();
                syncGrantedAbilities.Callback += SynchronizeGA;
                foreach (KeyValuePair<string, GameplayAbility> kvp in syncGrantedAbilities)
                    SynchronizeGA(SyncDictionary<string, GameplayAbility>.Operation.OP_ADD, kvp.Key, kvp.Value);

                // ATTRIBUTE PREDICTION BUFFER
                asc.attributes.ForEach(att => localAttributesBuffer.TryAdd(att.attributeName.name, new Queue<float>()));
                asc.OnAttributeChanged += AddAttributeToPredictionBuffer;
                // ATTRIBUTES REPLICATION
                syncAttributes.Callback += SynchronizeAttributes;
                foreach (KeyValuePair<string, float> kvp in syncAttributes) SynchronizeAttributes(SyncDictionary<string, float>.Operation.OP_ADD, kvp.Key, kvp.Value); // initial payload

                // GE PREDICTION BUFFER
                asc.OnGameplayEffectApplied += (ge) => AddEffectApplicationToPredictionBuffer(ge, ge.source);
                // GE REPLICATION
                syncEffects.Callback += SynchronizeGE;
                foreach (KeyValuePair<string, GameplayEffect> kvp in syncEffects) SynchronizeGE(SyncDictionary<string, GameplayEffect>.Operation.OP_ADD, kvp.Key, kvp.Value); // initial payload
            }
        }

        void OnDestroy() {
            // Clean ASCM Cache dictionary.
            ascmDictionary.Remove(netId);
        }

        public override void OnStopServer() {
            asc.OnGameplayAbilityDeactivated -= SubscribeOnGameplayAbilityDeactivated; //Stop errors when exiting playmode on HOST mode with toggle abilities (RPC would be called on client)
        }
        public void SubscribeOnGameplayAbilityDeactivated(GameplayAbility ga, string activationGUID) {
            RpcOnGameplayAbilityDeactivated(
                ga.guid,
                activationGUID
            );
        }
        //  GAMEPLAY ABILITY REPLICATION
        void SynchronizeGA(SyncDictionary<string, GameplayAbility>.Operation op, string key, GameplayAbility ga) {
            if (!isClientOnly) return;
            if (!isLocalPlayer) { // If this is not the local player (which will use local prediction), remove the GEs, they will come from GE synclist (replication). If we leave them here, they will be applied duplicated when their ability is activated locally.
                ga.effects.Clear();
            }
            switch (op) {
                case SyncIDictionary<string, GameplayAbility>.Operation.OP_ADD:
                    //  Non deterministic things must be server only. We have to filter them out on client, so we only predict deterministic things.
                    foreach (var effect in ga.effects) {
                        if (effect.chanceToApply != 1) effect.chanceToApply = 0; // Remove Chance to Apply. (Only the server decides if it was applied or not)
                    }

                    asc.grantedGameplayAbilities.Add(ga); // Cant use asc.GrantAbility because it will create a new guid, and get out of sync with server's guid.
                    asc.OnGameplayAbilityGranted?.Invoke(ga);
                    if (ga.isActive) asc.OnGameplayAbilityActivated?.Invoke(ga, ga.activationGUID);
                    break;
                case SyncIDictionary<string, GameplayAbility>.Operation.OP_REMOVE:
                    asc.UngrantAbility(ga.guid);
                    break;
                case SyncIDictionary<string, GameplayAbility>.Operation.OP_CLEAR:
                    asc.grantedGameplayAbilities.Clear();
                    break;
            }
        }

        public void LocalTryActivateAbility(GameplayAbility ga, string wrongGUID) {
            if (ga.source != localPlayerAsc) { Debug.Log($"NETWORK WARNING: ability activation for {ga.name} will fail. Not activated by the local player."); }

            string activationGUID = Guid.NewGuid().ToString(); // If networking, this should be generated on the client, and sent to server, then client back for prediction purposes. Use it as a prediction key.
            ga.activationGUID = activationGUID;

            CmdTryActivateAbility(ga.guid, ga.target.GetComponent<NetworkIdentity>().netId, activationGUID);
            if (!predictGameplayAbilityActivations) ga.isActive = true; // This will prevent local ability activation in the TryActivateAbility that called that event, and wait for the server.
            if (predictGameplayAbilityActivations) AddAbilityActivityToPredictionBuffer(ga, activationGUID);
        }

        [Command]
        public void CmdTryActivateAbility(string guid, uint tgtNetId, string activationGUID) { // Try to activate the ability on the server
            AbilitySystemComponentMirror targetASCM = AbilitySystemComponentMirror.ascmDictionary[tgtNetId];
            asc.TryActivateAbility(guid, targetASCM?.asc, activationGUID);
        }
        [ClientRpc]
        public void RpcOnGameplayAbilityActivated(string guid, uint srcNetId, uint tgtNetId, string activationGUID) {
            if (!isClientOnly) return; // Only run this if this is a client only. If it's a host or server, we dont want this code because the changes have been applied already?

            AbilitySystemComponentMirror srcASCM = ascmDictionary[srcNetId];
            AbilitySystemComponentMirror tgtASCM = ascmDictionary[tgtNetId];
            //  Debug.Log($"RpcOnGameplayAbilityActivated src: {srcASCM.netId} srcNetId: {srcNetId} tgt: {tgtASCM.netId} tgtNetId: {tgtNetId} activationGUID: {activationGUID} ascmDictionary.Count {ascmDictionary.Count}");
            if (tgtASCM.netId != tgtNetId) Debug.LogWarning("WARNING! ASCM NETID MISMATCH");
            GameplayAbility ga = asc.grantedGameplayAbilities.Find(x => x.guid == guid);

            if (activationGUID == null || (ga != null && !localAbilityActivationsBuffer.ContainsKey(activationGUID))) { // Activation not predicted, activate.
                ga?.CommitAbility(srcASCM.asc, tgtASCM.asc, activationGUID);
            } else { //  Activation already predicted. Do nothing.
            }
        }
        [ClientRpc]
        public void RpcOnGameplayAbilityDeactivated(string guid, string activationGUID) {
            if (!isClientOnly) return; // Only run this if this is a client only. If it's a host or server, we dont want this code because the changes have been applied already

            GameplayAbility ga = syncGrantedAbilities.GetValueOrDefault(guid);
            if (activationGUID == null || ga != null && !localAbilityActivationsBuffer.ContainsKey(activationGUID)) { // Deactivation not predicted, deactivate.
                ga?.DeactivateAbility(activationGUID);
            } else {
            }
        }
        // GAMEPLAY ABILITY PREDICTION BUFFER
        async void AddAbilityActivityToPredictionBuffer(GameplayAbility ga, string activationGUID) {
            if (activationGUID == null) {
                Debug.LogWarning("Null activationGUID GA PREDICTION BUFFER");
                return;
            }
            localAbilityActivationsBuffer.TryAdd(activationGUID, ga);
            await Task.Delay(predictionBufferDuration + 1_000); // offset needed, we need GA predictions to be removed AFTER GE predictions.
            localAbilityActivationsBuffer.Remove(activationGUID);
        }

        [ClientRpc]
        void RpcGameplayAbilityUndo(string activationGUID, ActivationFailure failure) { // If a GA failed to activate on the server, we deactivate it if active AND remove any effect applied by it.
            if (!isClientOnly) return; // Only run this if this is a client only. If it's a host or server, we dont want this code because the changes have been applied already
            if (localAbilityActivationsBuffer.Count < 1 ||
                activationGUID == null ||
                localAbilityActivationsBuffer.ContainsKey(activationGUID) == false
            ) {
                //  Debug.Log($"DONT CONTAIN ACT GUID:");
                return;
            }

            localAbilityActivationsBuffer[activationGUID].DeactivateAbility();
            asc.OnGameplayAbilityFailedActivation?.Invoke(localAbilityActivationsBuffer[activationGUID], activationGUID, failure);

            // Remove any durational GE
            HashSet<AbilitySystemComponent> ascmsInvolved = new();
            foreach (var effectEntry in localEffectsBuffer.ToList()) {
                if (effectEntry.Key.Contains(activationGUID)) {
                    ascmsInvolved.Add(effectEntry.Value.source.GetComponent<AbilitySystemComponent>());
                    ascmsInvolved.Add(effectEntry.Value.target.GetComponent<AbilitySystemComponent>());
                }
            }
            // Reset attributes to latest ones received from the server.
            foreach (var asc in ascmsInvolved.ToList()) {
                asc.attributesDictionary.Select(x => x.Value).ToList().ForEach(attribute => {
                    float newValue = syncAttributes[attribute.attributeName.name];
                    float oldValue = attribute.GetValue();
                    attribute.baseValue = newValue;
                    attribute.currentValue = newValue;
                    asc.OnAttributeChanged?.Invoke(attribute.attributeName, oldValue, newValue, null);
                });
            }


        }


        // ATTRIBUTES
        void SynchronizeAttributes(SyncDictionary<string, float>.Operation op, string attributeName, float newValue) {
            if (!isClientOnly) return; // Only run this if this is a client only. If it's a host or server, we dont want this code because the changes have been applied already
            StartCoroutine(SynchronizeAttributes_COROUTINE(op, attributeName, newValue));
        }
        IEnumerator SynchronizeAttributes_COROUTINE(SyncDictionary<string, float>.Operation op, string attributeName, float newValue) {
            yield return new WaitForEndOfFrame(); // Fixes duplicated attribute modification. e.g. MaxMana 100 -> 300 (from sync attribute) then a ga/ge again replicate activation locally (MaxMana -> MaxMana * 3 = 900)

            // ATTRIBUTE PREDICTION BUFFER
            if (localAttributesBuffer.ContainsKey(attributeName) && localAttributesBuffer[attributeName].Count > 0 && localAttributesBuffer[attributeName].Contains(newValue)) {// Dequeue until we found the value already predicted
                while (localAttributesBuffer[attributeName]?.Count > 0 && localAttributesBuffer[attributeName].Dequeue() != newValue) ; // dequeue until we remove newValue
            } else { // If the value received from the server was not predicted, wipe the buffer and assign it immediately. Server conciliation.
                if (localAttributesBuffer.ContainsKey(attributeName)) localAttributesBuffer[attributeName].Clear();

                Attribute attribute;
                asc.attributesDictionary.TryGetValue(attributeName, out attribute);
                float oldValue = attribute.GetValue();
                attribute.baseValue = newValue;
                attribute.currentValue = newValue;
                asc.OnAttributeChanged?.Invoke(asc.attributesDictionary[attributeName].attributeName, oldValue, newValue, null);
            }

        }
        // ATTRIBUTE PREDICTION BUFFER
        void AddAttributeToPredictionBuffer(AttributeName attName, float oldValue, float newValue, GameplayEffect ge) {// Buffers the sequence of changes to attributes. If the sequence is different from sequence of changes receivedfrom server. Clear it and reset to server value.
            if (ge == null || ge.source == null) return; // !!! important. SynchronizeAttributes shouldn't trigger an addition to prediction buffer. (It comes from the server, not a client prediction). When invoking AttributeChange from there, we send a null ge.
            if (ge != null && ge.source != null && ge.source != localPlayerAsc) return; // Do not predict if not activated by local player. In other words, predict only when local player activated it.

            localAttributesBuffer[attName.name].Enqueue(newValue);
        }

        // GE REPLICATION
        [ClientRpc]
        public void RpcSynchronizeGE(string key, GameplayEffect item) {
            SynchronizeGE(SyncDictionary<string, GameplayEffect>.Operation.OP_ADD, key, item);
        }
        public void SynchronizeGE(SyncDictionary<string, GameplayEffect>.Operation op, string key, GameplayEffect item) {
            if (!isClientOnly) return; // Only run this if this is a client only. If it's a host or server, we dont want this code because the changes have been applied already

            // If it is on syncEffects, the server has processed the chance to apply AND applied it. (Only the server decides if it was applied or not)
            if (item.chanceToApply != 1) item.chanceToApply = 1;

            if (op == SyncDictionary<string, GameplayEffect>.Operation.OP_ADD) {
                // GE PREDICTION BUFFER
                if (localEffectsBuffer.Count > 0 && localEffectsBuffer.ContainsKey(EffectBufferKey(item))) { // Do not add GE if already predicted locally
                    localEffectsBuffer[EffectBufferKey(item)].guid = item.guid; // Synchronize/Reconciliate locally predicted GE guid with actual guid on Server.
                    return;
                } else { // Add GE if not predicted locally
                    asc.ApplyGameplayEffect(item.source, item.target, item, item.guid);
                }
            }
            if (op == SyncDictionary<string, GameplayEffect>.Operation.OP_REMOVE) {
                GameplayEffect geToRemove;
                geToRemove = asc.appliedGameplayEffects.Find(x => x.guid == item.guid);
                if (geToRemove == null) geToRemove = asc.appliedGameplayEffects.Find(x => x.applicationGUID == item.applicationGUID && x.name == item.name); //If we couldnt find the guid, try finding by applicationGUID and name.

                if (geToRemove == null) { //  If the ge still can't be found, dont try to remove it. It was probably removed by prediction. 
                    return;
                }
            }
        }

        // GE PREDICTION BUFFER
        async void AddEffectApplicationToPredictionBuffer(GameplayEffect ge, AbilitySystemComponent src) {// Buffers the sequence of changes to tags. If the sequence is different from sequence of changes receivedfrom server. Clear it and reset to server value.
            if (src != null && src != localPlayerAsc) return; // Do not predict if the source is not the local player
            if (!localAbilityActivationsBuffer.ContainsKey(ge.applicationGUID)) return;

            //  Debug.Log($"GE ADD TO PREDICTION BUFFER : {ge.name} applicationGUID: {ge.applicationGUID} effecbuffer: {EffectBufferKey(ge)}");
            string effectBufferKey = EffectBufferKey(ge);
            localEffectsBuffer.Add(effectBufferKey, ge);
            // wait and try to remove if not null yet.
            await Task.Delay(predictionBufferDuration);
            if (localEffectsBuffer.ContainsKey(effectBufferKey)) localEffectsBuffer.Remove(effectBufferKey);
        }
        string EffectBufferKey(GameplayEffect ge) {// This will make an unique key for the predicted ge, using ge.applicationGUID and its index on the ga.
            if (ge.source != localPlayerAsc) return "NOT_LOCAL"; // if the source is not local player, it wont have been predicted, so it wont be in prediciton buffer and will cause null refs. stop it here.
            if (!localAbilityActivationsBuffer.ContainsKey(ge.applicationGUID)) return "NO_GA"; //  if that guid is not present in our ability activation buffer, we didnt predict it.
            GameplayAbility originGA = localAbilityActivationsBuffer[ge.applicationGUID];

            if (originGA == null) return "NO_GA"; // if there is no ga associated with the applicationGUID, it wasnt predicted by this client. stop it here.
            GameplayEffect originGE = originGA.effects.Find(x => x.name == ge.name);
            string index = originGA.effects.IndexOf(originGE).ToString();

            if (index == "-1") { // Check if it's a cooldown or cost GE
                if (originGA.cooldown.name == ge.name) index = "CD";
                if (originGA.cost.name == ge.name) index = "COST";
            }

            string geKey = $"{index + "_" + ge.applicationGUID}";
            return geKey;
        }
    }

}


public static class CustomReadWriteFunctions {
    public static void WriteMyAttributeName(this NetworkWriter writer, AttributeName value) {
        //  Debug.Log($"WriteMyAttributeName - value.name): {value.name}");
        writer.WriteString(value.name);
    }

    public static AttributeName ReadMyAttributeName(this NetworkReader reader) {
        //  string name = reader.ReadString();
        //  AttributeName attName = AttributeNameLibrary.Instance.GetByName(name);
        //  Debug.Log($"ReadMyAttributeName - name: {name} attName: {attName.name}");
        //  return attName;
        return AttributeNameLibrary.Instance.GetByName(reader.ReadString());
    }

    public static void WriteMyGameplayTag(this NetworkWriter writer, GameplayTag value) {
        writer.WriteString(value.name);
    }

    public static GameplayTag ReadMyGameplayTag(this NetworkReader reader) {
        //  string tagName = reader.ReadString();
        //  ReadMyGameplayTag gameplayTag = GameplayTagLibrary.Instance.GetByName(tagName);
        //  Debug.Log($"ReadMyGameplayTag - tagName: {tagName} foundTag: {gameplayTag.name}");
        //  return gameplayTag;
        return GameplayTagLibrary.Instance.GetByName(reader.ReadString());
    }

    public static void WriteMyAbilitySystemComponent(this NetworkWriter writer, AbilitySystemComponent value) {
        //  Debug.Log($"WriteMyAbilitySystemComponent: {value}");
        //  Debug.Log($".GetComponent<NetworkIdentity>(): {value?.GetComponent<NetworkIdentity>()}");
        //  Debug.Log($".GetComponent<NetworkIdentity>().netId: {value?.GetComponent<NetworkIdentity>().netId}");
        writer.WriteUIntNullable(value ? value.GetComponent<NetworkIdentity>().netId : null);
        //  Debug.Log($"write uint: {(value ? value.GetComponent<NetworkIdentity>().netId : null)}");
    }

    public static AbilitySystemComponent ReadMyAbilitySystemComponent(this NetworkReader reader) {
        var netId = reader.ReadUIntNullable();
        //  Debug.Log($"ReadMyAbilitySystemComponent uint read: {netId}");
        if (netId == null) return null;
        //  Debug.Log($"ReadMyAbilitySystemComponent uint read: [{string.Join(", ", AbilitySystemComponentMirror.ascmDictionary.Select(x => x.Key + " / " + x.Value.netId + " / " + x.Value.name))}]");
        AbilitySystemComponentMirror.ascmDictionary.TryGetValue((uint)netId, out AbilitySystemComponentMirror ascm);
        return ascm?.asc;
    }

    public static void WriteMyGameplayAbility(this NetworkWriter writer, GameplayAbility value) {
        value.abilityTags.FillStrings(value);
        value.effects.ForEach(ge => ge.gameplayEffectTags.FillStrings(ge));
        value.effects.ForEach(ge => ge.modifiers.ForEach(mod => mod.FillString())); // AttributeNames
        value.cooldown?.gameplayEffectTags.FillStrings(value.cooldown); // Tags on cost GE
        value.cooldown?.modifiers.ForEach(mod => mod.FillString());
        value.cost?.gameplayEffectTags.FillStrings(value.cost); // Tags on cost GE
        value.cost?.modifiers.ForEach(mod => mod.FillString());
        value.SerializeAdditionalData();

        var serializedGA = JsonUtility.ToJson(value, true);
        //  Debug.Log($"WRITER: {serializedGA}");
        writer.WriteString(serializedGA);
        writer.WriteString(value.GetType().FullName);
    }

    public static GameplayAbility ReadMyGameplayAbility(this NetworkReader reader) {

        var serializedGA = reader.ReadString();
        var className = reader.ReadString();
        //  Debug.Log($"READER: {className} {serializedGA}");
        GameplayAbility ga = (GameplayAbility)Activator.CreateInstance(Type.GetType(className));
        JsonUtility.FromJsonOverwrite(serializedGA, ga);

        ga.abilityTags.ClearTags(ga);
        ga.abilityTags.FillTags(ga);
        ga.effects.ForEach(ge => {
            ge.gameplayEffectTags.ClearTags(ge);
            ge.gameplayEffectTags.FillTags(ge);
            ge.modifiers.ForEach(mod => mod.FillModifier()); // AttributeNames
        });
        ga.cooldown?.gameplayEffectTags.ClearTags(ga.cooldown); // Tags on cooldown GE
        ga.cooldown?.gameplayEffectTags.FillTags(ga.cooldown); // Tags on cooldown GE
        ga.cooldown?.modifiers.ForEach(mod => mod.FillModifier());
        ga.cost?.gameplayEffectTags.ClearTags(ga.cost); // Tags on cost GE
        ga.cost?.gameplayEffectTags.FillTags(ga.cost); // Tags on cost GE
        ga.cost?.modifiers.ForEach(mod => mod.FillModifier());

        ga.DeserializeAdditionalData();
        //  Debug.Log($"READER final GA: class: {ga.GetType().FullName}  ga: {JsonUtility.ToJson(ga, true)}");

        return ga;
    }

}



