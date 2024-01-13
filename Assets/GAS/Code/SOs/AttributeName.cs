using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace GAS {
    public enum AttributeType {
        STAT,
        RESOURCE
    }

    [CreateAssetMenu(menuName = "GAS/AttributeName", fileName = "AttributeName")]
    [Serializable]
    public class AttributeName : ScriptableObject {
        public AttributeType attributeType;
    }

}