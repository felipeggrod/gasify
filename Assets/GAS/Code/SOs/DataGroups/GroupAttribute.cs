using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace GAS {
    [Serializable]
    public class AttributeInitialData {
        [ReadOnly] public string name;
        [SerializeReference] public AttributeName attributeName;
        public float baseValue;

    }

    [CreateAssetMenu(menuName = "GAS/DataGroup - Attributes", fileName = "GroupAttribute_")]
    public class GroupAttribute : PrefixedScriptableObject {
        public List<AttributeInitialData> group = new();

        public override void OnValidate() {
            base.OnValidate();

            // Update names after sorting
            group.ForEach(x => x.name = x.attributeName.name + ": " + x.baseValue);

            // Check for duplicate names
            HashSet<string> uniqueNames = new HashSet<string>();
            foreach (AttributeInitialData attributeInit in group) {
                if (!uniqueNames.Add(attributeInit.attributeName.name)) {
                    Debug.LogWarning($"Duplicate attribute name detected: {attributeInit.attributeName.name}");
                }
            }
        }

        public void OnEnable() {
            //Sort attributes by name
            group = group.OrderBy(attr => attr.attributeName.name).ToList();
            group = group.OrderBy(ga => ga.name).ToList();
        }


    }
}