
using UnityEngine;
using System;

namespace GAS {
    [Serializable]
    public class InstantAbility : GameplayAbility {
        public override void Activate(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            base.Activate(source, target, activationGUID);
            for (int i = 0; i < effects.Count; i++) {
                target.ApplyGameplayEffect(source, target, effects[i], activationGUID);
            }
            DeactivateAbility(activationGUID);
        }
        public override void DeactivateAbility(string activationGUID = null) {
            isActive = false;
        }
    }

    [System.Serializable]
    public class ProjectileAbility : GameplayAbility {
        public GameObject projectilePrefab = null;
        public GameObject projectile = null;
        public string projectileName = "";

        public override void SerializeAdditionalData() { //Searches a projectile prefab by its name. Prefab must be in root level of a Resources folder. You can also use any other way of referencing to it here. e.g. A scriptableObject or some other list.
            base.SerializeAdditionalData();
            if (projectilePrefab != null) projectileName = projectilePrefab?.name;
        }
        public override void DeserializeAdditionalData() {
            base.DeserializeAdditionalData();
            projectilePrefab = Resources.Load<GameObject>(projectileName);
        }

        public override GameplayAbility Instantiate(AbilitySystemComponent asc) {
            ProjectileAbility newInstance = (ProjectileAbility)base.Instantiate(asc);
            newInstance.projectilePrefab = projectilePrefab;
            return newInstance;
        }

        public override void Activate(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            base.Activate(source, target, activationGUID);
            //We could also just add a prefab to it.
            if (projectilePrefab == null) projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            else { projectile = GameObject.Instantiate(projectilePrefab); }

            projectile.name = "projectile";
            projectile.transform.position = source.transform.position + source.transform.forward;
            projectile.transform.rotation = source.transform.rotation;

            var rb = projectile.AddComponent<Rigidbody>();
            rb.drag = 0;
            rb.useGravity = false;
            rb.AddForce(rb.transform.forward * 10f, ForceMode.VelocityChange);


            var projectileComponent = projectile.AddComponent<Projectile>();
            projectileComponent.OnHit += (hitAsc) => {
                // Debug.Log($"ProjectileAbility hitAsc.name {hitAsc.name}");
                effects.ForEach(ge => hitAsc.ApplyGameplayEffect(source, hitAsc, ge, activationGUID));
            };
            projectileComponent.source = source;

            base.DeactivateAbility(activationGUID);
        }
    }

    public class Projectile : MonoBehaviour {
        public float speed;
        public Action<AbilitySystemComponent> OnHit;
        public AbilitySystemComponent source;

        private void Start() {
            Destroy(this.gameObject, 30f);
        }
        private void OnCollisionEnter(Collision other) {
            if (other.gameObject.GetComponent<AbilitySystemComponent>() != null && other.gameObject.GetComponent<AbilitySystemComponent>() != source) {
                OnHit?.Invoke(other.gameObject.GetComponent<AbilitySystemComponent>());
                Destroy(gameObject);
            }
        }

    }

    [System.Serializable]
    public class TargetedProjectileAbility : GameplayAbility { //We could improve this by having the prefab be referenced here, and loaded from Resources for multiplayer. If we have a targeted projectile (like mobas and mmos, we just make the projectile follow the target)
        public override void Activate(AbilitySystemComponent source, AbilitySystemComponent target, string activationGUID) {
            base.Activate(source, target, activationGUID);

            //We could also just add a prefab to it.
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "projectile";
            projectile.transform.position = source.transform.position + source.transform.forward;
            projectile.transform.rotation = source.transform.rotation;
            var projectileComponent = projectile.AddComponent<TargetedProjectile>();
            projectileComponent.speed = 15f;
            projectileComponent.target = target.transform;
            projectileComponent.OnHit += () => effects.ForEach(ge => target.ApplyGameplayEffect(source, target, ge, activationGUID));

            base.DeactivateAbility();
        }
    }


    public class TargetedProjectile : MonoBehaviour {
        public float speed;
        public Transform target;
        public Action OnHit;

        public Rigidbody rb;

        public float t = 0;
        public float turnRate = 80f;

        private void Start() {
            Destroy(this.gameObject, 30f);
            if (this.GetComponent<Collider>() != null) Destroy(this.GetComponent<Collider>());

            rb = gameObject.AddComponent<Rigidbody>();
            rb.drag = 0;
            rb.useGravity = false;
            rb.velocity = transform.forward * speed;

            var trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = .3f;
            trail.startWidth = 1;
            trail.endWidth = 0;
        }
        private void FixedUpdate() {
            t += Time.fixedDeltaTime;

            if (target == null) return;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position), Time.fixedDeltaTime * turnRate * t);
            rb.velocity = transform.forward * speed;

            if (Vector3.Distance(this.transform.position, target.position) < 0.6f) {
                OnHit?.Invoke();
                Destroy(gameObject);
            }
        }

    }

}