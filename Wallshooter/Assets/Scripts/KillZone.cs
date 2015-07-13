using UnityEngine;
using System.Collections;

public class KillZone : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    void OnTriggerEnter(Collider other) {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null) {
            pv.RPC("TakeDamage", PhotonTargets.AllBuffered, 9999999f);
        }
    }
}
