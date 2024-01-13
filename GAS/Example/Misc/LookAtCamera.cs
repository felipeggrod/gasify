using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    public class LookAtCamera : MonoBehaviour {
        Transform c;

        // Use this for initialization
        IEnumerator Start() {
            yield return new WaitForSeconds(0.3f);
            c = Camera.main.transform;

            // transform.rotation = transform.parent.rotation;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;

            yield return new WaitForEndOfFrame();
        }

        // Update is called once per frame
        void Update() {
            // Rotate the camera every frame so it keeps looking at the target
            if (c != null) transform.LookAt(c);
        }

    }
}