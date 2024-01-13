using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    [System.Serializable]
    public class Modifier {
        [SerializeField][ReadOnly] public string name;
        [SerializeReference] public AttributeName attributeName;
        [HideInInspector] public string attributeNameSerialized = ""; //Needed for Networking serialization

        public Modifier() { name = GetType().Name; }

        public virtual float GetValue(GameplayEffect ge = null) {
            return 0;
        }

        public virtual void FillString() {
            // Debug.Log($"Modifier.FillString: {attributeName.name}");
            attributeNameSerialized = attributeName.name;
        }
        public virtual void FillModifier() {
            // Debug.Log($"Modifier.FillAttribute: {attributeNameString}");
            attributeName = AttributeNameLibrary.Instance.GetByName(attributeNameSerialized);
        }
    }

    [System.Serializable]
    public class BasicModifier : Modifier {
        public float value;

        public override float GetValue(GameplayEffect ge = null) {
            return value;
        }
    }
}
