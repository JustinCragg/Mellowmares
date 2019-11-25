using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Biscuit : MonoBehaviour {
    public List<Vector3> patrolPoints = new List<Vector3>();
    public int currentTarget = 0;

    Manager manager = null;

    NavMeshAgent agent = null;

    Vector3 destination;
    public bool chasing = false;

    public float moveSpeed = 5;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        setDestination(patrolPoints[currentTarget]);

        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
    }

    void Update() {
        Ray ray = new Ray(transform.position + transform.forward, manager.player.transform.position - transform.position);
        RaycastHit hit;
        chasing = false;
        Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.red);
        if (Physics.Raycast(ray, out hit, 20.0f)){
            Marshmellow mellow = hit.transform.GetComponent<Marshmellow>();
            if (mellow != null) {
                if (mellow.playerControlled == true) {
                    //chasing = true;
                }
            }
        }
        
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1, LayerMask.GetMask("Marshmellow"))) {
            //whomp-esque animation goes here
        }


        /// dummied out chasing mechanic
        /*
        if (chasing == true) {
            setDestination(manager.player.transform.position);

            if (Vector3.Distance(transform.position, destination) <= 2) {
                Debug.Log("ATTACK!");
                manager.player.squash();
            }
        }
        else 
         */

        {
            setDestination(patrolPoints[currentTarget]);
            if (Vector3.Distance(patrolPoints[currentTarget], transform.position) < 2.5f) {
                currentTarget++;
                if (currentTarget >= patrolPoints.Count) {
                    currentTarget = 0;
                }
                setDestination(patrolPoints[currentTarget]);
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        Vector3 dir = destination - transform.position;
        if (dir == Vector3.zero) {
            return;
        }
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, moveSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected() {
        for (int i = 0; i < patrolPoints.Count; i++) {
            Gizmos.color = new Color(1 - (i + 1) / (float)patrolPoints.Count, 0, (i+1) / (float)patrolPoints.Count);
            Gizmos.DrawSphere(patrolPoints[i], 0.5f);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if( collision.gameObject.tag == "Marshmellow") {
            manager.player.squash(); // BE ADVISED might accident'y kill player on contact with npcs
        }
    }

    void setDestination(Vector3 target) {
        if (target != destination) {
            target.y = transform.position.y;
            destination = target;
        }
    }
}
