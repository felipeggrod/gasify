using System.Collections.Generic;
using UnityEngine;
using System;


namespace GAS {
    [CreateAssetMenu(menuName = "GAS/GameplayAbilitySO_InstantAbility", fileName = "GA_InstantAbility")]
    [Serializable]
    public class GameplayAbilitySO_InstantAbility : GameplayAbilitySO {
        public GameplayAbilitySO_InstantAbility() {
            ga = new InstantAbility();
        }
    }

}