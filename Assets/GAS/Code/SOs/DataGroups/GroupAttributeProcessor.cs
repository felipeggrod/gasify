using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using EasyButtons;


namespace GAS {

    [CreateAssetMenu(menuName = "GAS/DataGroup - Attribute Processors", fileName = "GroupAttributeProcessor_")]
    public class GroupAttributeProcessor : PrefixedScriptableObject {
        [SerializeReference] public List<AttributeProcessor> group = new List<AttributeProcessor>();

        public override void OnValidate() {
            base.OnValidate();
            NameClampers(group);
        }

        public void NameClampers(List<AttributeProcessor> processors) {
            processors.ForEach(x => {
                if (x is Clamper) x.name = $"{(x as Clamper).min} < {(x as Clamper).clampedAttributeName.name} < {(x as Clamper).max}";
                if (x is ClamperMaxAttributeValue) x.name = $"{(x as ClamperMaxAttributeValue).clampedAttributeName.name} < {(x as ClamperMaxAttributeValue).max.name}";
                if (x is ClamperMinAttributeValue) x.name = $"{(x as ClamperMinAttributeValue).min.name} < {(x as ClamperMinAttributeValue).clampedAttributeName.name}";
            });
        }

        [Button("ADD ATTRIBUTE PROCESSOR", Expanded = true)]
        public void AddAttributeProcessor(AttributeProcessorType processorType) {
            Helpers.AddAttributeProcessor(processorType, group);
        }


    }
}