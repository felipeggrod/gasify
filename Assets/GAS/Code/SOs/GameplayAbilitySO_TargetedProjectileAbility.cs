using System.Collections.Generic;
using UnityEngine;
using System;
using GAS;


namespace GAS {
    [CreateAssetMenu(menuName = "GAS/GameplayAbilitySO_TargetedProjectileAbility", fileName = "GA_TargetedProjectileAbility")]
    [Serializable]
    public class GameplayAbilitySO_TargetedProjectileAbility : GameplayAbilitySO {
        public GameplayAbilitySO_TargetedProjectileAbility() {
            ga = new TargetedProjectileAbility();
        }
    }

}