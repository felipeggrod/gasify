using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GAS {
    [CreateAssetMenu(menuName = "GAS/DataGroup - GameplayAbilities", fileName = "GroupGA_")]
    public class GroupGA : PrefixedScriptableObject {
        public List<GameplayAbilitySO> group = new();
    }
}