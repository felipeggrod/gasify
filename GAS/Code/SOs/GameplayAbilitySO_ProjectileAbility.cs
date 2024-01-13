using System.Collections.Generic;
using UnityEngine;
using System;
using GAS;


namespace GAS {
    [CreateAssetMenu(menuName = "GAS/GameplayAbilitySO_ProjectileAbility", fileName = "GA_ProjectileAbility")]
    [Serializable]
    public class GameplayAbilitySO_ProjectileAbility : GameplayAbilitySO {
        public GameplayAbilitySO_ProjectileAbility() {
            ga = new ProjectileAbility();
        }
    }

}