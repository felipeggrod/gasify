using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace GAS {
    /// <summary>
    /// This object holds the initialization data for an ASC.
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/DataGroup - AbilitySystemComponent", fileName = "GroupASC_")]
    [Serializable]
    public class GroupASC : PrefixedScriptableObject {
        [Tooltip("Attributes to be added to an ASC. Will be sorted automatically on script reload.")]
        public GroupAttribute attributes;
        public GroupAttributeProcessor attributeProcessors;
        public GroupGA abilities;

        public void AddAttributes(AbilitySystemComponent asc) {
            asc.attributes.Clear();
            if (attributes == null) { Debug.LogWarning(asc.name + " has no attributes."); return; }

            foreach (AttributeInitialData init in attributes.group) {
                asc.attributes.Add(new Attribute() { attributeName = init.attributeName, name = init.attributeName.name, baseValue = init.baseValue });
            }
        }

        public void AddAttributeProcessors(AbilitySystemComponent asc) {
            asc.attributesProcessors.Clear();
            if (attributeProcessors == null) return;
            foreach (var attProcessor in attributeProcessors.group) {
                Type processorType = attProcessor.GetType();
                AttributeProcessor newProcessor = (AttributeProcessor)Activator.CreateInstance(processorType);
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(attProcessor), newProcessor);
                asc.attributesProcessors.Add(newProcessor);
            }
        }

        public void OnEnable() {
            if (attributes == null) Debug.LogError("NULL attributes in " + name);
        }

        public override void OnValidate() {
            base.OnValidate();
        }

        public void GrantAbilities(AbilitySystemComponent asc) {
            asc.grantedGameplayAbilities.Clear();
            if (abilities == null) return;
            foreach (var gaSO in abilities.group) {
                if (gaSO == null) {
                    Debug.LogError($"GrantAbilitiesFromSO - NULL GameplayAbilitySO");
                    continue;
                }

                if (gaSO is GameplayAbilitySO abilitySO) {
                    GameplayAbility ga = abilitySO.ga; // Access the 'ga' property directly
                    asc.GrantAbility(ga);
                } else {
                    Debug.LogError($"Invalid type in the initialGameplayAbilitiesSO list.");
                }
            }
        }
    }

}

