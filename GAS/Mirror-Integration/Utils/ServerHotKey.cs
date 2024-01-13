using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace GAS {
    public class ServerHotKey : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {

        }
        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.F1)) GetComponent<NetworkManager>().StartHost();
            if (Input.GetKeyDown(KeyCode.F2)) GetComponent<NetworkManager>().StartClient();
            if (Input.GetKeyDown(KeyCode.F3)) GetComponent<NetworkManager>().StartServer();
            if (Input.GetKeyDown(KeyCode.F4)) GetComponent<NetworkManager>().StopClient();
        }
    }
}
