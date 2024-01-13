using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    public class DestroySoonReceiver : MonoBehaviour {

        private void Start() {
        }

        public void OnDestroySoon() {
            // Debug.Log($"DestroySoon:");
            // StopAllCoroutines();
            StartCoroutine(AnimateV3(Vector3.one * 1.6f, Vector3.one, 0.16f));
            StartCoroutine(AnimateV3(Vector3.one * 1f, Vector3.zero, 0.5f));
        }


        IEnumerator AnimateV3(Vector3 origin, Vector3 target, float duration) {
            yield return new WaitForSeconds(1.6f);
            float journey = 0f;
            while (journey <= duration) {
                journey = journey + Time.deltaTime;
                float percent = Mathf.Clamp01(journey / duration);

                transform.localScale = Vector3.Lerp(origin, target, percent);

                yield return null;
            }
        }
    }
}
