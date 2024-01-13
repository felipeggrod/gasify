using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    /// <summary>
    /// The value representation of a parameter in our system.
    /// Very important: There are 2 types of Attributes: Resources and Stats.
    /// Resources use baseValue. Expendable e.g. HealthPoints, ManaPoints, Potions, Ammo, Coins
    /// Stats use currentValue. Not expendable e.g. MaxHealth, MaxMana, JumpHeight, CritChance
    /// Also, creating them anytime during gameplay is not supported currently. Create all of them in the inspector or on Awake().
    /// </summary>
    [System.Serializable]
    public class Attribute {
        [ReadOnly] public string name;
        [SerializeReference] public AttributeName attributeName;
        public float baseValue; //The BaseValue is the permanent value of the Attribute
        public float currentValue; //CurrentValue is the BaseValue plus temporary modifications from GameplayEffects
        public AttributeModifier modification = new();
        public Action<AttributeName, float, float, GameplayEffect> OnPostAttributeChange;
        public Action<Attribute, GameplayEffect> OnPreAttributeChange;
        private float oldValue; //OnAttributeChange cache
        public float partialValue;

        //Only for stats attributes e.g. MaxHealth, MaxMana, MaxSpeed
        public void ApplyModifiers(GameplayEffect ge) {
            oldValue = currentValue;

            partialValue = baseValue + modification.value;


            if (oldValue != partialValue) OnPreAttributeChange?.Invoke(this, ge);
            currentValue = partialValue;

            if (oldValue != currentValue && attributeName.attributeType == AttributeType.STAT) {
                OnPostAttributeChange?.Invoke(attributeName, oldValue, currentValue, ge);
            }
        }

        //Only for resource attributes e.g. HealthPoints, ManaPoints, Ammo, Coins
        public void ApplyModifierAsResource(Modifier modifier, GameplayEffect ge) {
            oldValue = baseValue;

            partialValue = baseValue;

            partialValue += modifier.GetValue(ge);


            if (oldValue != partialValue) OnPreAttributeChange?.Invoke(this, ge);
            baseValue = partialValue;

            if (oldValue != baseValue && attributeName.attributeType == AttributeType.RESOURCE) {
                OnPostAttributeChange?.Invoke(attributeName, oldValue, baseValue, ge);
            }
        }

        public float GetValue() {
            if (attributeName.attributeType == AttributeType.STAT) return currentValue;
            else return baseValue;
        }

    }

    //Only for Stats attributes
    [System.Serializable]
    public class AttributeModifier {
        public float value;

        public void Value(List<Modifier> modifiers, GameplayEffect ge) {
            foreach (Modifier modifier in modifiers) {
                value += modifier.GetValue(ge);
                break;
            }
        }

        public void Clear() {
            value = 0;
        }
    }
}

