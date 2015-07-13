using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;

	Animator anim;
	
	FXManager fxManger;
	private float cooldown = 0;

	WeaponData weaponData;

	float realAimAngle = 0;

	public float aimAngle = 0;

	// Note! Only our local char will use this.
	// Remove give us absolute positions
	public float speed = 10f;		// The speed at which I run
	public float jumpSpeed = 6f;	// How much power we put into our jump. Change this to jump higher.

	// Bookkeeping variables
	[System.NonSerialized]
	public Vector3 direction = Vector3.zero;	// forward/back & left/right
	[System.NonSerialized]
	public bool isJumping = false;
	float verticalVelocity = 0;		// up/down

	CharacterController cc;
	bool gotFirstUpdate = false;

    private Vector3 groundVelocity = Vector3.zero;

	// Use this for initialization
	void Start() {
		
		
		initAnim();
	}

	void initAnim() {
		if (anim != null)
			return;
		anim = GetComponent<Animator>();
		if (anim == null) {
			Debug.LogError("ZOMG, you forgot to put an Animator component on this character prefab!");
		}

		cc = GetComponent<CharacterController>();
		if (cc == null) {
			Debug.LogError("No character controller found!");
		}

		fxManger = GameObject.FindObjectOfType<FXManager>();
		if (fxManger == null)
		{
			Debug.LogError("Couldn't find an FXManager!!!!!");
		}


	}

	void Update() {
		
		cooldown -= Time.deltaTime;
	}


	// FixedUpdate is called once per physics loop
	// Do all MOVEMENT and other physics stuff here.
	void FixedUpdate () {
		if( photonView.isMine ) {
			DoLocalMovement();
		}
		else {
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
			anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), realAimAngle, 0.1f));
		}
	}


	void DoLocalMovement() {

		// "direction" is the desired movement direction, based on our player's input


        if (cc.isGrounded || Mathf.Abs(verticalVelocity) > jumpSpeed * 0.75f) {
            groundVelocity = direction;
        } else {
            if (direction != Vector3.zero)
                groundVelocity = Vector3.Lerp(groundVelocity, direction, 0.1f);
        }

        Vector3 dist = groundVelocity * speed * Time.deltaTime;


		if (isJumping) {
			isJumping = false;
			if (cc.isGrounded) {
				verticalVelocity = jumpSpeed;
			}
		}

		if (cc.isGrounded && verticalVelocity < 0) {
			// We are currently on the ground and vertical velocity is
			// not positive (i.e. we are not starting a jump).

			// Ensure that we aren't playing the jumping animation
			anim.SetBool("Jumping", false);

			// Set our vertical velocity to *almost* zero. This ensures that:
			//   a) We don't start falling at warp speed if we fall off a cliff (by being close to zero)
			//   b) cc.isGrounded returns true every frame (by still being slightly negative, as opposed to zero)
			verticalVelocity = Physics.gravity.y * Time.deltaTime;
		} else {
			// We are either not grounded, or we have a positive verticalVelocity (i.e. we ARE starting a jump)

			// To make sure we don't go into the jump animation while walking down a slope, make sure that
			// verticalVelocity is above some arbitrary threshold before triggering the animation.
			// 75% of "jumpSpeed" seems like a good safe number, but could be a standalone public variable too.
			//
			// Another option would be to do a raycast down and start the jump/fall animation whenever we were
			// more than ___ distance above the ground.
			if (Mathf.Abs(verticalVelocity) > jumpSpeed * 0.75f) {
				anim.SetBool("Jumping", true);
			}

			// Apply gravity.
			verticalVelocity += Physics.gravity.y * Time.deltaTime;
		}

		// Add our verticalVelocity to our actual movement for this frame
		dist.y = verticalVelocity * Time.deltaTime;


		// Set our animation "Speed" parameter. This will move us from "idle" to "run" animations,
		// but we could also use this to blend between "walk" and "run" as well.
		anim.SetFloat("Speed", direction.magnitude);

		// Adjust aim angle animation
		anim.SetFloat("AimAngle", aimAngle);

		// Apply the movement to our character controller (which handles collisions for us)
		cc.Move(dist);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

		initAnim();

		if(stream.isWriting) {
			// This is OUR player. We need to send our actual position to the network.

			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("Speed"));
			stream.SendNext(anim.GetBool("Jumping"));
			stream.SendNext(anim.GetFloat("AimAngle"));
		}
		else {
			// This is someone else's player. We need to receive their position (as of a few
			// millisecond ago, and update our version of that player.

			// to compensate for lag- update position to old real position first
			//transform.position = realPosition;
			//transform.rotation = realRotation;



			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
			anim.SetFloat("Speed", (float)stream.ReceiveNext());
			anim.SetBool("Jumping", (bool)stream.ReceiveNext());
			realAimAngle = (float)stream.ReceiveNext();

			if (gotFirstUpdate == false)
			{
				transform.position = realPosition;
				transform.rotation = realRotation;
				gotFirstUpdate = true;
				anim.SetFloat("AimAngle", realAimAngle);
			}

		}

	}


	// 
	public void FireWeapon(Vector3 origin, Vector3 dir) {

		if (weaponData == null) {
			weaponData = gameObject.GetComponentInChildren<WeaponData>();
			if (weaponData == null) {
				Debug.LogError("Weapon Data not found!");
				return;
			}

		}

		if (cooldown > 0) {
			return;
		}
		Ray ray = new Ray(origin, dir);
		Transform hitTransform;
		Vector3 hitPoint;

		hitTransform = FindClosestHitObject(ray, out hitPoint);

		if (hitTransform != null) {
			// do special effect @ hitlocation hitInfo.point
			Debug.Log("We hit : " + hitTransform.name);
			Health h = hitTransform.GetComponent<Health>();

			while (h == null && hitTransform.parent) {
				hitTransform = hitTransform.parent;
				h = hitTransform.GetComponent<Health>();
			}

			// once we reach here, hitTransform may not be the same one we started with....

			if (h != null) {
				//h.TakeDamage(damage);
				PhotonView pv = h.GetComponent<PhotonView>();
				if (pv != null) {

					TeamMember tm = hitTransform.GetComponent<TeamMember>();
					TeamMember myTm = this.GetComponent<TeamMember>();
					if (tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0 || tm.teamID != myTm.teamID) {
						pv.RPC("TakeDamage", PhotonTargets.AllBuffered, weaponData.damage);
                        if (h.bleeds)
                            fxManger.GetComponent<PhotonView>().RPC("BloodFX", PhotonTargets.All, hitTransform.position);
					}
				} else {
					Debug.LogError("Expected Photon View!");
				}

			}

			if (fxManger != null) {
				DoGunFx(hitPoint);

			}

		} else {
			if (fxManger != null) {
				hitPoint = Camera.main.transform.position + Camera.main.transform.forward * 100f;
				DoGunFx(hitPoint);
			}
		}

		cooldown = weaponData.firerate;

	}



	void DoGunFx(Vector3 hitPoint)
	{
		fxManger.GetComponent<PhotonView>().RPC("SniperBulletFX", PhotonTargets.All, weaponData.transform.position, hitPoint);
	}

	
	Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {
		RaycastHit[] hits = Physics.RaycastAll(ray);
		Transform closestHit = null;
		float distance = 0;
		hitPoint = Vector3.zero;
		foreach (RaycastHit hit in hits) {
			if(hit.transform != this.transform && (closestHit == null || hit.distance < distance)) {
				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}
		}
		return closestHit;
	}

}
