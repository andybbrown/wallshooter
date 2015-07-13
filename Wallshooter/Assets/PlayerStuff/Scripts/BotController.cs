using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour {
    NetworkCharacter netChar;

    static Waypoint[] waypoints;

    public Waypoint destination;
    float waypointTargetDistance = 1f;
    float aggroRange = 20f;
    float targetInnaccuracy = 2f; // extra innaccuracy 

    float targettingCooldown = 0f;
    float targetAngleCriteria = 4f;
    TeamMember myTarget = null;

    // Use this for initialization
    void Start() {
        netChar = GetComponent<NetworkCharacter>();

        if (waypoints == null) {
            waypoints = GameObject.FindObjectsOfType<Waypoint>();
        }
        destination = GetClosestWaypoint();
    }


    // Update is called once per frame
    void Update() {
        DoDestination();
        DoDirection();
        DoRotation();

        if (targettingCooldown <= 0) {
            DoTargeting();
            targettingCooldown = 0.5f;
        } else {
            targettingCooldown -= Time.deltaTime;
        }
        DoFire();

    }


    Waypoint GetClosestWaypoint() {
        Waypoint closest = null;
        float dist = 0f;
        foreach (Waypoint w in waypoints) {
            if (closest == null || Vector3.Distance(transform.position, w.transform.position) < dist) {
                closest = w;
                dist = Vector3.Distance(transform.position, w.transform.position);
            }
        }

        return closest;
    }

    void DoFire() {
        if (myTarget == null)
            return;

        // Ignore vert height for shooting
        Vector3 targetPos = myTarget.transform.position;
        targetPos.y = transform.position.y;

        if (Vector3.Angle(transform.forward, targetPos - transform.position) < targetAngleCriteria) {

            Vector3 innAccuracyAngle = new Vector3(Random.Range(-targetInnaccuracy, targetInnaccuracy), Random.Range(-targetInnaccuracy, targetInnaccuracy), 0);
            // finds fireDir in local space
            Vector3 fireDir = Quaternion.Euler(-netChar.aimAngle, 0 , 0)  * Vector3.forward;

            fireDir = Quaternion.Euler(innAccuracyAngle) * fireDir;

            fireDir = transform.TransformDirection(fireDir); // converts from local to global 
            netChar.FireWeapon(transform.position + transform.up * 1.5f + transform.forward * 2f , fireDir); // offset view level to "chest" 1.5 meters up
        }

       
        
    }


    void DoDestination() {
        if (destination != null) {
            if (Vector3.Distance(transform.position, destination.transform.position) <= waypointTargetDistance) {

                if (destination.connectedWaypoints != null && destination.connectedWaypoints.Length > 0) {
                    // pick a connected waypoint
                    destination = destination.connectedWaypoints[Random.Range(0, destination.connectedWaypoints.Length)];
                } else {
                    destination = null;
                }
            }
        }
            
    }

    void DoDirection() {
        if (destination != null) {
            netChar.direction = destination.transform.position - transform.position;
            netChar.direction.y = 0;
            netChar.direction.Normalize();
        } else {
            netChar.direction = Vector3.zero;
        }
    }

    void DoTargeting() {
        // Do we have an enemy target in range?
        // VERY SLOW
        TeamMember closest = null;
        float dist = 0f;
        foreach (TeamMember tm in GameObject.FindObjectsOfType<TeamMember>()) {
            if (tm == GetComponent<TeamMember>()) {
                // How Zen! We found ourselves!
                // Loop to the next target.
                continue;
            }
            // check if on enemy team
            if (tm.teamID == 0 || tm.teamID != GetComponent<TeamMember>().teamID) {
                float tmDist = Vector3.Distance(tm.transform.position, transform.position);
                if (tmDist <= aggroRange) {
                    if (closest == null || tmDist < dist) {
                        dist = tmDist;
                        closest = tm;
                    }
                }
            }
        }

        myTarget = closest;
    }

    void DoRotation() {

        // let's figure out where we should be facing
        // by default - look where we are going.
        Vector3 lookDirection = netChar.direction;

        if (myTarget != null) {
            lookDirection = myTarget.transform.position - transform.position;

            // Find angle for aiming
            Vector3 localLookDir = transform.InverseTransformPoint(myTarget.transform.position);
            float targetAimAngle = Mathf.Atan2(localLookDir.y, localLookDir.z) * Mathf.Rad2Deg;
            netChar.aimAngle = targetAimAngle;
        } else {
            netChar.aimAngle = 0;
        }

        Quaternion lookRot = Quaternion.LookRotation(lookDirection);
        lookRot.eulerAngles = new Vector3(0, lookRot.eulerAngles.y, 0);
        transform.rotation = lookRot;
    }
	
}
