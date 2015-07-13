using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {
	
	public float hitPoints = 100f;

    public bool bleeds = false;
	private float currentHitPoints;

    private bool mouseLocked = false;
	// Use this for initialization
	void Start () {
		currentHitPoints = hitPoints;
	}
	
	[PunRPC]
	public void TakeDamage(float amt) {
		currentHitPoints -= amt;
		if (currentHitPoints <= 0) {
			Die();
		}
	}

    void OnGUI() {
        if (GetComponent<PhotonView>().isMine) {
            if (gameObject.tag == "Player") {
                if (GUI.Button(new Rect(Screen.width - 150, 0, 150, 20), "Suicide")) {
                    
                    Die();
                }

                if (mouseLocked) {
                    if (GUI.Button(new Rect(Screen.width - 150, 20, 150, 20), "Esc to Unlock")) {

                        Cursor.lockState = CursorLockMode.Locked;
                        UnityEngine.Cursor.visible = false;
                    }
                } else {
                    if (GUI.Button(new Rect(Screen.width - 150, 20, 150, 20), "Lock Mouse")) {
                        mouseLocked = true;
                        Cursor.lockState = CursorLockMode.Locked;
                        UnityEngine.Cursor.visible = false;
                    }
                }

                if (GUI.Button(new Rect(Screen.width - 150, 40, 150, 20), "Health Hack")) {
                    PhotonView pv = this.GetComponent<PhotonView>();
                    if (pv == null) {
                        Debug.Log("Y U NO HAVE PHOTON VIEW?");
                    }
                    pv.RPC("TakeDamage", PhotonTargets.AllBuffered, -1000.0f);
                }


                GUI.Box(new Rect(Screen.width - 150, Screen.height - 70, 150, 30), "Health :" + currentHitPoints);
            }


        }
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            mouseLocked = false;
            Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }
	
	void Die() {
		if (GetComponent<PhotonView>().instantiationId == 0) { 
			Destroy(gameObject);
		} else {
			if (GetComponent<PhotonView>().isMine) {
                NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();

                if (gameObject.tag == "Player" ) {
                    
                    nm.standbyCamera.SetActive(true);
                    nm.respawnTimer = 2f;
                } else if (gameObject.tag == "Bot") {
                    nm.botSpawnTimer = 2f;
                }

				PhotonNetwork.Destroy(gameObject);
			}
		}
	}
}
