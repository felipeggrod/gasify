using UnityEngine;
using System;


namespace GAS {
    /// <summary> <para>
    /// Attribute processors are called everytime an attribute is about to change. They add a layer of logic to constraint the values attributes can have. <br/>
    /// e.g. Clampers (clamp Health at MaxHealth), (Clam MovementSpeed at min 0), (Clamp bullets at max ClipSize).
    ///  </para> </summary>
    [Serializable]
    public class AttributeProcessor {
        [ReadOnly] public string name;
        public virtual void PreProcess(Attribute attribute, GameplayEffect ge, AbilitySystemComponent asc) {
            // Debug.Log($"PreProcess: {attribute.attributeName}, partialValue {attribute.partialValue} ge.name {ge.name}");
        }
        public virtual void PostProcessed(AttributeName name, float oldValue, float newValue, GameplayEffect ge) {
            // Debug.Log($"PostProcess: {name}, oldValue {oldValue} newValue {newValue} ge.name {ge.name}");
        }

    }

    [Serializable]
    public class Clamper : AttributeProcessor {
        public float min, max;
        public AttributeName clampedAttributeName;
        public override void PreProcess(Attribute attribute, GameplayEffect ge, AbilitySystemComponent asc) {
            // Debug.Log($"Clamper PreProcess: {attribute.attributeName}");
            if (attribute.attributeName == clampedAttributeName) {
                if (attribute.partialValue < min) attribute.partialValue = min;
                if (attribute.partialValue > max) attribute.partialValue = max;
            }
        }
    }

    [Serializable]
    public class ClamperMaxAttributeValue : AttributeProcessor {
        public AttributeName max;
        public AttributeName clampedAttributeName;

        [HideInInspector] public Attribute clamper = null;

        public override void PreProcess(Attribute attribute, GameplayEffect ge, AbilitySystemComponent asc) {
            if (attribute.attributeName == clampedAttributeName) {
                if (clamper == null || clamper.attributeName == null) asc.attributesDictionary.TryGetValue(max.name, out clamper);
                if (attribute.partialValue > clamper.GetValue()) attribute.partialValue = clamper.GetValue();
            }
        }
    }

    [Serializable]
    public class ClamperMinAttributeValue : AttributeProcessor {
        public AttributeName min;
        public AttributeName clampedAttributeName;

        [HideInInspector] public Attribute clamper = null;

        public override void PreProcess(Attribute attribute, GameplayEffect ge, AbilitySystemComponent asc) {
            if (attribute.attributeName == clampedAttributeName) {
                if (clamper == null || clamper.attributeName == null) asc.attributesDictionary.TryGetValue(min.name, out clamper);
                if (attribute.partialValue < clamper.GetValue()) attribute.partialValue = clamper.GetValue();
            }
        }
    }
}
