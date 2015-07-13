using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	// This component is only enabled for "my player" (i.e. the character belonging to the local client machine).
	// Remote players figures will be moved by a NetworkCharacter, which is also responsible for sending "my player's"
	// location to other computers.


    NetworkCharacter netChar;

	// Use this for initialization
	void Start () {
        netChar = GetComponent<NetworkCharacter>();
	}
	
	// Update is called once per frame
	void Update () {

		// WASD forward/back & left/right movement is stored in "direction"
		netChar.direction = transform.rotation * new Vector3( Input.GetAxis("Horizontal") , 0, Input.GetAxis("Vertical") );

		// This ensures that we don't move faster going diagonally
        if (netChar.direction.magnitude > 1f) {
            netChar.direction = netChar.direction.normalized;
		}


		// If we're on the ground and the player wants to jump, set
		// verticalVelocity to a positive number.
		// If you want double-jumping, you'll want some extra code
		// here instead of just checking "cc.isGrounded".
		if( Input.GetButton("Jump")) {
            netChar.isJumping = true;
        } else {
            netChar.isJumping = false;
        }
        AdjustAimAngle();


        if (Input.GetButton("Fire1")) {
            netChar.FireWeapon(Camera.main.transform.position, Camera.main.transform.forward);
            
        }


        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
            
        }

	}

    void AdjustAimAngle()
    {
        Camera myCamera = this.GetComponentInChildren<Camera>();

        if (myCamera == null)
        {
            Debug.LogError("No camera found on the character!");
            return;
        }

        // Quaternion.FromToRotation()
        float camX = myCamera.transform.rotation.eulerAngles.x;
        float aimAngle = 0;
        if (camX <= 90f)
        {
            aimAngle = -camX;
        } else {
            aimAngle = 360 - camX;
        }

        netChar.aimAngle = aimAngle;


    }

}
