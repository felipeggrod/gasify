using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using GAS;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GAS {
    public class AbilitySystemComponentTestCalls : MonoBehaviour {
        public AbilitySystemComponent asc;

        private void Awake() {
            AssignAbilitySystemComponent();
        }

        [Button]
        private void AssignAbilitySystemComponent() {
            if (asc == null) {
                if (GetComponent<AbilitySystemComponent>() == null) {
                    asc = gameObject.AddComponent<AbilitySystemComponent>();
                } else {
                    asc = GetComponent<AbilitySystemComponent>();
                }
            }
        }

        [Button]
        public void Test_AddAttributes() {
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("Health"), 70);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("HealthMax"), 100);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("Mana"), 20);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("ManaMax"), 100);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("MovementSpeed"), 5);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("HealthPotion"), 3);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("DamageMin"), 5);
            AddAttributeName(asc, AttributeNameLibrary.Instance.GetByName("DamageMax"), 15);
        }
        // ATTRIBUTES
        // [EasyButtons.Button] // Unity doesn't save newly added attributes to prefabs using this button. Why?  
        [Tooltip("Simplest way to have the editor display names while in edit mode. Adding attributes is only supported in edit mode.")]
        public void AddAttributeName(AbilitySystemComponent asc, AttributeName attributeName, float baseValue) { //Only add attributes in edit mode. If they are added after start/awake they wont work. There is a bunch of attribute event wiring that happens on Awake/Start
            AddAttribute(asc, new Attribute() { attributeName = attributeName, name = attributeName.name, baseValue = baseValue });
        }
        public void AddAttribute(AbilitySystemComponent asc, Attribute attribute) {
            asc.attributes.Add(attribute);
        }

        // [EasyButtons.Button]
        // public void AddAttributeProcessor(AttributeProcessorType processorType) {// Used to create new mod types on editor. Not recommended to add mods to GEs at runtime. But hey, if it works for you, who am I to say no?
        //     Helpers.AddAttributeProcessor(processorType, this);
        // }



        [Button]
        public void Test_ApplySomeGEs_Instant() {
            var ge7 = new GameplayEffect() { durationType = GameplayEffectDurationType.Instant, name = "ge7" };
            var mod7 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("Health"), value = 2 };

            ge7.modifiers.Add(mod7);
            asc.ApplyGameplayEffect(asc, asc, ge7);
        }

        [Button]
        public void Test_ApplySomeGEs_Infinite() {
            var ge1 = new GameplayEffect() { durationType = GameplayEffectDurationType.Infinite, name = "ge1" };
            var mod1 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("MaxHealth"), value = 10 };

            ge1.modifiers.Add(mod1);
            asc.ApplyGameplayEffect(asc, asc, ge1);

            var ge2 = new GameplayEffect() { durationType = GameplayEffectDurationType.Infinite, name = "ge2" };
            var mod2 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("MaxHealth"), value = 2 };

            ge2.modifiers.Add(mod2);
            asc.ApplyGameplayEffect(asc, asc, ge2);

            var ge3 = new GameplayEffect() { durationType = GameplayEffectDurationType.Infinite, name = "ge3" };
            var mod3 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("MaxHealth"), value = 15 };

            ge3.modifiers.Add(mod3);
            asc.ApplyGameplayEffect(asc, asc, ge3);
        }


        [Button]
        public void Test_ApplySomeGEs_Duration() {
            var ge4 = new GameplayEffect() { durationType = GameplayEffectDurationType.Duration, name = "ge4", durationValue = 3 };
            var mod4 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("MaxHealth"), value = 100 };

            ge4.modifiers.Add(mod4);
            asc.ApplyGameplayEffect(asc, asc, ge4);

            var ge5 = new GameplayEffect() { durationType = GameplayEffectDurationType.Duration, name = "ge5", durationValue = 1 };
            var mod5 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("MaxHealth"), value = .1f };

            ge5.modifiers.Add(mod5);
            asc.ApplyGameplayEffect(asc, asc, ge5);

            var ge6 = new GameplayEffect() { durationType = GameplayEffectDurationType.Duration, name = "ge6", durationValue = 7 };
            var mod6 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("MaxHealth"), value = .1f };

            ge6.modifiers.Add(mod6);
            asc.ApplyGameplayEffect(asc, asc, ge6);
        }



        [Button]
        public void Test_ApplySomeGEs_Periodic() {
            var ge7 = new GameplayEffect() { durationType = GameplayEffectDurationType.Duration, name = "ge7 Periodic", durationValue = 10, period = 1 };
            var mod7 = new BasicModifier() { attributeName = AttributeNameLibrary.Instance.GetByName("Health"), value = 1 };

            ge7.modifiers.Add(mod7);
            asc.ApplyGameplayEffect(asc, asc, ge7);

        }


        [Button]
        public void Test_Grant_GAs() {
#if UNITY_EDITOR


            GameplayAbilitySO gaSO = AssetDatabase.LoadAssetAtPath<GameplayAbilitySO>("Assets/GAS/Resources/Abilities&Effects/GA_MyAbility1.asset");
            GameplayAbilitySO dmgSO = AssetDatabase.LoadAssetAtPath<GameplayAbilitySO>("Assets/GAS/Resources/Abilities&Effects/GA_Damage.asset");
            GameplayAbilitySO fireSO = AssetDatabase.LoadAssetAtPath<GameplayAbilitySO>("Assets/GAS/Resources/Abilities&Effects/GA_Fire.asset");
            GameplayAbilitySO iceSO = AssetDatabase.LoadAssetAtPath<GameplayAbilitySO>("Assets/GAS/Resources/Abilities&Effects/GA_Ice.asset");
            GameplayAbilitySO projectileSO = AssetDatabase.LoadAssetAtPath<GameplayAbilitySO>("Assets/GAS/Resources/Abilities&Effects/GA_ProjectileAbility.asset");
            GameplayAbilitySO passiveSO = AssetDatabase.LoadAssetAtPath<GameplayAbilitySO>("Assets/GAS/Resources/Abilities&Effects/GA_PassiveHealingAbility.asset");



            // if (gaSO == null) Debug.Log($"Test_Grant_GAs null gaSO!!!");
            // asc.initialGameplayAbilitiesSO.Add(gaSO);
            // asc.GrantAbility(gaSO.ga);
            if (projectileSO == null) Debug.Log($"Test_Grant_GAs null projectileSO!!!");
            // asc.GrantAbility(projectileSO.ga);
            asc.GrantAbility(projectileSO.ga);

            // GameplayAbility blockingGA = new BlockingAbility().Instantiate();

            // asc.GrantAbility(gat);

            // asc.GrantAbility(blockingGA);
            // asc.GrantAbility(blockedGA);
            // asc.GrantAbility(ignoredGA);

            asc.GrantAbility(dmgSO.ga);
            asc.GrantAbility(fireSO.ga);
            asc.GrantAbility(iceSO.ga);
            // asc.GrantAbilit(passiveSO.ga);

#endif
        }


        [Button]
        public void Test_Clear_Granted_GAs() {
            asc.grantedGameplayAbilities.Clear();
        }

        public void Test_TryActivate(int index) { asc.TryActivateAbility(index, asc); }
        [Button] public void Test_TryActivate_GA_UsePotion() { asc.TryActivateAbility("Potion", asc); }
        [Button] public void Test_TryActivate_GA_UsePoison() { asc.TryActivateAbility("Poison", asc); }
        [Button] public void Test_TryActivate_GA_UseAntiPoison() { asc.TryActivateAbility("AntiPoison", asc); }


        [Button]
        public void Test_GA_TryActivateBlock_Ignored_Required() {
            asc.TryActivateAbility("BlockingAbility", asc);
            asc.TryActivateAbility("BlockedAbility", asc);
            asc.TryActivateAbility("IgnoredAbility", asc);
        }

        [Button]
        public void Test_GA_TryActivateToggle() {
            asc.TryActivateAbility(6, asc);
        }

        [Button]
        public void Test_GA_TryActivate_CalcExec_DamageGA() { // GE CALCULATION
            asc.TryActivateAbility(7, asc);
        }

        [Button]
        public void Test_GA_TryActivateMMC() {// GE MMC
            asc.TryActivateAbility(8, asc);
        }



        [Button]
        public void Test_AddAttributeProcessors() {
            asc.attributesProcessors.Add(new Clamper() { min = 0, max = 1000, clampedAttributeName = AttributeNameLibrary.Instance.GetByName("HealthMax") });
            asc.attributesProcessors.Add(new Clamper() { min = 0, max = 1000, clampedAttributeName = AttributeNameLibrary.Instance.GetByName("Health") });
            asc.attributesProcessors.Add(new ClamperMaxAttributeValue() { max = AttributeNameLibrary.Instance.GetByName("HealthMax"), clampedAttributeName = AttributeNameLibrary.Instance.GetByName("Health") });
        }

        [Button]
        public void Test_ClearProcessors() {
            asc.attributesProcessors.Clear();
        }

        [Button]
        public void Test_PopulateGAS() {
            asc = GetComponent<AbilitySystemComponent>();
            // Debug.Log($"Resources.FindObjectsOfTypeAll<CuesLibrary>(): {(CuesLibrary)FindObjectsOfTypeAll(typeof(CuesLibrary))[0]}");


            Test_AddAttributes();
            Test_Grant_GAs();
            Test_AddAttributeProcessors();
        }

        [Button]
        public void Test_ClearGAS() {
            asc.grantedGameplayAbilities.Clear();
            asc.attributesProcessors.Clear();
            asc.attributes.Clear();
        }

        [Button]
        public void Up() {
            // GetComponent<AbilitySystemComponentMirror>().Up();
        }
        [Button]
        public void Test_AbilityCreateInstance(int i) {
            var ga = asc.grantedGameplayAbilities[i].Instantiate(asc);
            ga.name += " i";

            foreach (var fx in ga.effects) {
                Debug.Log($"GA_Instance: fx.gameplayEffectTags {JsonUtility.ToJson(fx.gameplayEffectTags, true)}");
            }
            asc.GrantAbility(ga);
        }



    }
}



