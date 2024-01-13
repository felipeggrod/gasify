using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GAS;


namespace GAS {
    [System.Serializable]
    public enum ModifierType {
        SimpleModifier,
        ScalableModifier,
        ScalableModifierCSV,
        AttributeBasedModifier,
    }

    [System.Serializable]
    public enum AttributeProcessorType {
        Clamper,
        ClamperMaxAttributeValue,
        ClamperMinAttributeValue,
    }

    public class Helpers {
        public static void AddModifier(ModifierType modifierType, GameplayEffect ge) {
            ge.modifiers.Add(CreateModifier(modifierType));
        }

        public static void ChangeModifier(ModifierType modifierType, Modifier modifier) {
            modifier = CreateModifier(modifierType);
        }

        public static Modifier CreateModifier(ModifierType modifierType) {

            BasicModifier mod = new BasicModifier();
            return mod;

        }



        public static void AddAttributeProcessor(AttributeProcessorType attProcessorType, List<AttributeProcessor> list) {
            switch (attProcessorType) {
                case AttributeProcessorType.Clamper:
                    list.Add(new Clamper() { min = 0, max = 1000, clampedAttributeName = null });
                    break;
                case AttributeProcessorType.ClamperMaxAttributeValue:
                    list.Add(new ClamperMaxAttributeValue() { max = null, clampedAttributeName = null });
                    break;
                case AttributeProcessorType.ClamperMinAttributeValue:
                    list.Add(new ClamperMinAttributeValue() { min = null, clampedAttributeName = null });
                    break;

            }
        }

        public static string StringFromList(IEnumerable<string> stringArray) {
            return StringFromList(stringArray.ToList());
        }
        public static string StringFromList(List<string> stringList) {
            return $"[{string.Join(", ", stringList)}]";
        }

    }
}
