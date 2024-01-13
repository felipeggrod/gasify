using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GAS;
using System;

namespace GAS {
    [System.Serializable]
    public class PlayerController : MonoBehaviour {
        public AbilitySystemComponent asc;
        public bool selfCastIfNoTarget = true;
        public float moveSpeed = 10f;
        public float rotateSpeed = 100f;
        public List<AbilitySystemComponent> targets = new List<AbilitySystemComponent>();
        public Collider targetChecker;
        public Material enemyMaterial, targetedMaterial;

        public AttributeName movementSpeed;
        public GameplayTag jumpTag;

        public int selectedAbilityIndex;
        public Action OnSelectAbility;

        private void Awake() {
            asc = GetComponent<AbilitySystemComponent>();
            asc.OnAttributeChanged += (attributeName, oldValue, newValue, ge) => { if (attributeName == movementSpeed) moveSpeed = newValue; };
            asc.OnTagsInstant += (tags, source, target, applicationGuid) => { if (tags.Contains(jumpTag)) GetComponent<Rigidbody>().AddForce(Vector3.up * 10, ForceMode.VelocityChange); };
        }


        void Update() {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            //Scrollable GA selection and click to activate
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) ScrollAbility(1);
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) ScrollAbility(-1);
            if (Input.GetKeyDown(KeyCode.Mouse0)) TryActivateAbilityCommand(selectedAbilityIndex);

            // Things like the movement itself, dont really need to be an ability or effect. But still can use the GAS. 
            // Here we are using attributes for movement speed. We hooked moveSpeed variable to the MovementSpeed Attribute. It reassigns the value whenever the attribute changes, so we dont have to get the value again every Update.
            if (vertical != 0f) {
                transform.position += transform.forward * vertical * moveSpeed * Time.deltaTime;
            }
            if (horizontal != 0f) {
                transform.Rotate(Vector3.up, horizontal * rotateSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.Q)) transform.position += transform.right * -1 * moveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.E)) transform.position += transform.right * 1 * moveSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space)) Jump();
            if (Input.GetKeyDown(KeyCode.LeftShift)) Dash();

            if (Input.GetKeyDown(KeyCode.Alpha0)) TryActivateAbilityCommand(0);
            if (Input.GetKeyDown(KeyCode.Alpha1)) TryActivateAbilityCommand(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryActivateAbilityCommand(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryActivateAbilityCommand(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TryActivateAbilityCommand(4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TryActivateAbilityCommand(5);
            if (Input.GetKeyDown(KeyCode.Alpha6)) TryActivateAbilityCommand(6);
            if (Input.GetKeyDown(KeyCode.Alpha7)) TryActivateAbilityCommand(7);
            if (Input.GetKeyDown(KeyCode.Alpha8)) TryActivateAbilityCommand(8);
            if (Input.GetKeyDown(KeyCode.Alpha9)) TryActivateAbilityCommand(9);
        }

        private void FixedUpdate() {
            targets.ForEach(x => { if (x != null) x.GetComponentInChildren<Renderer>().material = enemyMaterial; });


            targets.Clear();
            var colliders = Physics.OverlapBox(targetChecker.bounds.center, targetChecker.bounds.extents).ToList();
            colliders.Remove(targetChecker);
            colliders.RemoveAll(x => x.GetComponent<AbilitySystemComponent>() == null);
            colliders.RemoveAll(x => x.GetComponent<AbilitySystemComponent>() == asc); //Remove this controller's asc if present
            targets = colliders.Select(col => col.GetComponent<AbilitySystemComponent>()).ToList();
            targets.ForEach(x => x.GetComponentInChildren<Renderer>().material = targetedMaterial);

        }

        public void TryActivateAbilityCommand(int i) {
            //Get targets from any asc inside targetChecker collider.

            //Self cast if no target...
            if (selfCastIfNoTarget && targets.Count == 0) targets.Add(asc);

            //If targeted projectile ability, just get all enemies and put them as targets...
            if (asc.grantedGameplayAbilities[i] is TargetedProjectileAbility) {
                targets = FindObjectsOfType<AbilitySystemComponent>().ToList();
                targets.Remove(GetComponent<AbilitySystemComponent>());
                // Debug.Log($"targets: {Helpers.StringFromList(targets.Select(x => x.name))}");
            }


            //Cast on server if using mirror component, else just call it normally
            foreach (var target in targets) {
                asc.TryActivateAbility(i, target);

            }

            if (targets.Contains(asc)) targets.Remove(asc);

        }

        public void ScrollAbility(int scrollQuantity) {
            if ((selectedAbilityIndex == 0 && scrollQuantity < 0) || (selectedAbilityIndex > asc.grantedGameplayAbilities.Count - 2 && scrollQuantity > 0)) return;
            selectedAbilityIndex += scrollQuantity;
            OnSelectAbility?.Invoke();
        }

        public void Jump() {
            asc.TryActivateAbility("Jump", asc);
        }

        public void Dash() {
            asc.TryActivateAbility("Dash", asc);
        }
    }
}