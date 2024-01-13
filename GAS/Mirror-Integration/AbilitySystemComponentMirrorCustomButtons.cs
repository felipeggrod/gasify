using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Data.SqlTypes;
using System;
using GAS;
using EasyButtons;

public class AbilitySystemComponentMirrorCustomButtons : MonoBehaviour {
    public AbilitySystemComponentMirror ascm;
    // Start is called before the first frame update
    void Start() {
        if (ascm == null) ascm = GetComponent<AbilitySystemComponentMirror>();
        // ascm.asc.OnAttributeChanged += (attributeName, oldValue, newValue, ge) => { if (attributeName == AttributeNameLibrary.Instance.GetByName("Health")) Debug.Log($"OnAttributeChanged Health: new {newValue} old {oldValue} current {ascm.asc.GetAttributeValue(attributeName.name)}"); ; };
        // ascm.asc.OnTagsChanged += (tags, s, t, activationGUID) => { Debug.Log($"ASCMIRRORCB asc.OnTagsChanged activationGUID: {activationGUID} [{string.Join(", ", tags.Select(x => x.name))}] ascm.tagsBuffer [{string.Join(", ", ascm.localTagsBuffer.Select(x => x))}]"); };
    }


    [Button]
    public void Test3() {
        // ascm.syncAppliedGEs.TryAdd(System.Guid.NewGuid().ToString(), new GameplayEffect() { name = " NEW DUDERINO in the list " + Time.fixedDeltaTime, source = ascm.asc });
        // ascm.syncAppliedGEs.TryAdd(ascm.asc.grantedGameplayAbilities[4].effects[0].guid, ascm.asc.grantedGameplayAbilities[4].effects[0]);
    }

    [Button]
    public void Log() {
        // Debug.Log($"sync GEs [{string.Join(", ", ascm.syncAppliedGEs.Select(x => x.Value.name))}]");
    }

    [Button]
    public void changeGA() {
        ascm.syncGrantedAbilities[ascm.asc.grantedGameplayAbilities[4].guid] = new GameplayAbility() { name = "Noname", level = -2 };
    }



}
