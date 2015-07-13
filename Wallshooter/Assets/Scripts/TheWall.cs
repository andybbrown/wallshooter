using UnityEngine;
using System.Collections;

public class TheWall : MonoBehaviour {

    public WallTrigger greenTrigger;
    public WallTrigger redTrigger;
    Vector3 realPosition = Vector3.zero;
    float wallSpeed = 0.5f;
    Rigidbody rb;
    PhotonView pv;
	// Use this for initialization

    float nextUpdate = 0.1f;
	void Start () {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
	}

    void FixedUpdate() {
        if (PhotonNetwork.isMasterClient) {
            if (greenTrigger.pushing > redTrigger.pushing) {
                rb.MovePosition(transform.position + transform.right * Time.deltaTime * wallSpeed);
            }
            if (greenTrigger.pushing < redTrigger.pushing) {
                rb.MovePosition(transform.position - transform.right * Time.deltaTime * wallSpeed);
            }

            nextUpdate -= Time.deltaTime;
            if (nextUpdate <= 0) {
                nextUpdate = 0.1f;
                pv.RPC("UpdateLocation", PhotonTargets.All, transform.position);
            }

        } else {
            rb.MovePosition(Vector3.Lerp(transform.position, realPosition, 0.4f));
            //rb.MovePosition(realPosition);
        }
    }



    [PunRPC]
    public void UpdateLocation(Vector3 loc) {
        realPosition = loc;
    }

    [PunRPC]
    public void TakeDamage(float amt) {
        
    }
}
