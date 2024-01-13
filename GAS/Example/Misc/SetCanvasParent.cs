using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace GAS {
    public class SetCanvasParent : MonoBehaviour {
        // Start is called before the first frame update
        // public string parentName = "WorldSpaceCanvasUI";
        void Start() {
            // Debug.Log($"SetCanvasParent");
            // transform.SetParent(transform.parent.Find("WorldSpaceCanvasUI").transform);

            transform.SetParent(transform.parent.GetComponentInChildren<GridLayoutGroup>().transform);
            GetComponent<RawImage>().CrossFadeAlpha(1, 0.16f, false);

            StartCoroutine(AnimateV3(Vector3.one * 1.6f, Vector3.one, 0.16f));
        }

        [EasyButtons.Button]
        void OnDestroySoon() {
            // Debug.Log($"OnDestroySoon: {name}");
            StartCoroutine(OnDestroySoonCOROUTINE());
            // OnDestroySoonASYNC()
        }

        async void OnDestroySoonASYNC() {
            // await Task.Delay(1000);
            GetComponent<RawImage>()?.CrossFadeAlpha(0, 0.6f, false);
            await Task.Delay(1000);
            Destroy(gameObject);
        }

        IEnumerator OnDestroySoonCOROUTINE() {
            GetComponent<RawImage>()?.CrossFadeAlpha(0, 2.6f, false);
            yield return new WaitForSeconds(5);
            Destroy(gameObject);

        }

        IEnumerator AnimateV3(Vector3 origin, Vector3 target, float duration) {
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
