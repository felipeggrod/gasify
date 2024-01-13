using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GAS;
using EasyButtons;

namespace GAS {
    public class UI_TextFromASC : MonoBehaviour {
        public AbilitySystemComponent asc;
        public PlayerController playerController;
        public Text text;
        public float updateCooldown;

        // Start is called before the first frame update
        void Start() {
            text = GetComponent<Text>();
            asc = GetComponentInParent<AbilitySystemComponent>();
            playerController = asc.GetComponent<PlayerController>();

            if (playerController) playerController.OnSelectAbility += () => UpdateUI();
            asc.OnAttributeChanged += (x, y, z, w) => UpdateUI();
            asc.OnTagsChanged += (x, y, z, w) => UpdateUI();
            asc.OnGameplayEffectsChanged += (x) => UpdateUI();
            asc.OnGameplayAbilityActivated += (x, ak) => UpdateUI();
            asc.OnGameplayAbilityDeactivated += (x, ak) => UpdateUI();
            asc.OnGameplayAbilityGranted += (x) => UpdateUI();
            asc.OnGameplayAbilityUngranted += (x) => UpdateUI();

            UpdateUI();
            updateCooldown = 0.1f;
            Invoke("UpdateUI", 0.3f);
        }

        void Update() {
            if (updateCooldown > 0) updateCooldown -= Time.deltaTime;
        }

        [Button]
        void UpdateUI() {
            if (text == null) {
                Debug.Log($"UpdateUI: text is null!");
                return;
            }
            if (updateCooldown > 0) return;
            if (updateCooldown == 0) updateCooldown = 0.02f;
            text.text = "";

            //ATTRIBUTES
            text.text += $" Attributes: \n";
            text.text += "<color=#5FFF40>";
            // foreach (var att in asc.attributes) {
            //     text.text += $"{att.attributeName}: {att.GetValue()} \n";
            // }
            foreach (var att in asc.attributes) {
                string line = $"{att.name + ": " + att.GetValue()}" + "    -    ";
                text.text += line;
                if (asc.attributes.IndexOf(att) % 2 == 1 && asc.attributes.Last() != att) text.text += "\n";
            }
            text.text += "</color>";


            //TAGS
            text.text += $"\n Tags:\n<color=#F5FF40>";
            foreach (var tag in asc.tags) {
                text.text += $"{tag.name},";
                // if (asc.tags.IndexOf(tag) % 3 == 2) text.text += "\n";
            }
            text.text += "</color>";



            //GEs, time remaining
            text.text += $"\n Gameplay Effects: \n";
            text.text += "<color=#008EFF>";
            text.text += $"[{string.Join(", ", asc.appliedGameplayEffects.Select(x => x.name))}] \n";
            text.text += "</color>";

            // //GAs, cooldownRemaining, canActivate?
            var activeString = "<color=#008EFF>ACTIVE </color>";
            text.text += $"Gameplay Abilities: \n";
            foreach (var ga in asc?.grantedGameplayAbilities) {
                if (playerController != null && playerController.isActiveAndEnabled && asc.grantedGameplayAbilities.IndexOf(ga) == playerController.selectedAbilityIndex) text.text += "<color=#008EFF>";

                if (ga != null) text.text += $"{(ga.isActive ? activeString : "")}{asc?.grantedGameplayAbilities?.IndexOf(ga)}: {ga?.name} \n";
                // if (asc.grantedGameplayAbilities.IndexOf(ga) % 3 == 2) text.text += "\n";

                if (playerController != null && playerController.isActiveAndEnabled && asc.grantedGameplayAbilities.IndexOf(ga) == playerController.selectedAbilityIndex) text.text += "</color>";
            }
            text.text += $"\n";
        }
    }
}