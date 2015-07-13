using UnityEngine;
using System.Collections;

public class WallTrigger : MonoBehaviour {

    public int pushing = 0;


    void OnTriggerEnter(Collider other) {
        
        if (other.GetComponent<NetworkCharacter>() != null) {
            pushing += 1;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.GetComponent<NetworkCharacter>() != null) {
            pushing -= 1;
        }
    }
}
