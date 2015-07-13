using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour {

    public float selfDestructTime = 1.0f;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        selfDestructTime -= Time.deltaTime;

        if (selfDestructTime <= 0)
        {
            Die();
        }
	}


    private void Die()
    {
        PhotonView pv = GetComponent<PhotonView>();
        if (pv == null || GetComponent<PhotonView>().instantiationId == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
