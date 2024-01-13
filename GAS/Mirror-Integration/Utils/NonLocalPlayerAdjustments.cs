using System.Collections;
using System.Collections.Generic;
using GAS;
using Mirror;
using UnityEngine;

public class NonLocalPlayerAdjustments : NetworkBehaviour {
    void Start() {
        if (!this.isOwned) {
            GetComponentInChildren<Camera>().GetComponent<AudioListener>().enabled = false;
            GetComponentInChildren<Camera>().transform.tag = "Untagged";
            GetComponentInChildren<Camera>().gameObject.SetActive(false);

            GetComponent<PlayerController>().enabled = false;
        }
    }
}
