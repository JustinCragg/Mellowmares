using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum MellowSize { SMALL, MEDIUM, LARGE };

public class Marshmellow : MonoBehaviour {
    public bool playerControlled = false;
    public bool trapped = false;
    public List<Marshmellow> marshmellows = new List<Marshmellow>();

    public MellowSize mellowSize = MellowSize.MEDIUM;

    public float maxhealth = 100;
    public float health = 100;

    public float speed = 5;
    float direction = 1;
    NavMeshAgent agent = null;

    public float jumpPower = 5;
    bool jumping = false;
    Rigidbody rgbd = null;

    Vector3 cameraVelocity = Vector3.zero;
    Camera mainCamera = null;
    Manager manager = null;

    bool morphing = false;
    int morphCount = 0;

    Vector3 safePos;

    Animator animator = null;

    void Start() {
        rgbd = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        animator = GetComponent<Animator>();

        if (playerControlled == true) {
            agent.enabled = false;
        }
        else {
            GetComponent<Collider>().isTrigger = true;
        }
    }

    void Update() {
        // Kills the marshmellow if they fall off the map
        if (transform.position.y <= -10) {
            die();
        }
        // Kills the marshmellow if they hit 0 health
        if (health <= 0) {
            die();
        }

        // If the marshmellow is the player
        if (playerControlled == true) {
            // If the player is not currently morphing
            if (morphing == false) {
                // If the marshmellow is large, enable splitting
                if (mellowSize != MellowSize.SMALL) {
                    if (Input.GetKeyDown(KeyCode.Return)) {
                        split();
                    }
                }

                // Horizontal Movement
                float hori = Input.GetAxis("Horizontal");
                animator.SetBool("Walking", true);
                if (hori < 0) {
                    direction = -1;
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                }
                else if (hori > 0) {
                    direction = 1;
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                else {
                    animator.SetBool("Walking", false);
                }

                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, Quaternion.Euler(0, manager.cameraRot * direction, 0), Time.deltaTime * manager.cameraRotSpeed);

                transform.position += new Vector3(hori * speed * Time.deltaTime, 0, 0);
            }
            // If the player is currently morphing, check if enough marshmellows are nearby
            else {
                morphCount = 0;
                int count = 0;
                for (int i = 0; i < marshmellows.Count; i++) {
                    if (count < 3) {
                        if (mellowSize == MellowSize.SMALL && marshmellows[i].mellowSize == MellowSize.SMALL) {
                            count++;
                            if (Vector3.Distance(transform.position, marshmellows[i].transform.position) <= 1.5f) {
                                morphCount++;
                            }
                        }
                        else if (mellowSize == MellowSize.MEDIUM && marshmellows[i].mellowSize == MellowSize.MEDIUM) {
                            count++;
                            if (Vector3.Distance(transform.position, marshmellows[i].transform.position) <= 1.5f) {
                                morphCount++;
                            }
                        }
                    }
                    else {
                        break;
                    }
                }
                // If three marshmellows are close enough, merge into a large marshmellow
                if (morphCount == 3) {
                    merge();
                }
            }

            // Handle Landing
            if (jumping == true) {
                if (rgbd.velocity == Vector3.zero) {
                    jumping = false;
                    animator.SetBool("Fall", false);
                }
            }

            // If the player is medium or small, allow morphing
            if (mellowSize != MellowSize.LARGE) {
                if (Input.GetKeyDown(KeyCode.Space) == true) {
                    morphing = true;
                    // Summons the other marshmellows
                    int count = 0;
                    for (int i = 0; i < marshmellows.Count; i++) {
                        if (count < 3) {
                            if (mellowSize == MellowSize.SMALL && marshmellows[i].mellowSize == MellowSize.SMALL) {
                                marshmellows[i].setDestination(transform.position);
                                count++;
                            }
                            else if (mellowSize == MellowSize.MEDIUM && marshmellows[i].mellowSize == MellowSize.MEDIUM) {
                                marshmellows[i].setDestination(transform.position);
                                count++;
                            }
                        }
                    }
                }
                else if (Input.GetKeyUp(KeyCode.Space) == true) {
                    morphing = false;
                }
            }

            // Moving Camera
            moveCamera();
        }
        // If the marshmellow is not the player and not morphing follow the player
        else {
            if (trapped == false) {
                if (agent.isOnOffMeshLink == false) {
                    safePos = transform.position;
                    safePos.z = 0;
                }
                if (manager.player.morphing == false) {
                    float offset = -manager.player.direction * 5;
                    Vector3 testing = manager.player.transform.position;
                    testing.x += offset;
                    setDestination(testing);
                }
            }
            else {
                if (Vector3.Distance(manager.player.transform.position, transform.position) <= 5) {
                    manager.player.marshmellows.Add(this);
                    trapped = false;
                }
            }
        }
    }

    void FixedUpdate() {
        // Enables jumping with physics
        animator.ResetTrigger("Jump");
        if (playerControlled == true) {
            if (morphing == false) {
                // Jumping
                if (Input.GetAxis("Vertical") != 0 && jumping == false) {
                    jumping = true;
                    animator.SetTrigger("Jump");
                    rgbd.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                }
            }
        }
    }

    // Merges a number of marshmellows together into a larger marshmellow
    void merge() {
        // Create a large marshmellow
        Marshmellow newPlayer = null;
        if (mellowSize == MellowSize.SMALL) {
            newPlayer = Instantiate(manager.marshMedium, transform.position, transform.rotation).GetComponent<Marshmellow>();
        }
        else if (mellowSize == MellowSize.MEDIUM) {
            newPlayer = Instantiate(manager.marshLarge, transform.position, transform.rotation).GetComponent<Marshmellow>();
        }
        manager.player = newPlayer;
        newPlayer.playerControlled = true;

        // Updates the other marshmellows
        newPlayer.marshmellows = marshmellows;

        int count = 0;
        List<int> removing = new List<int>();
        for (int i = 0; i < marshmellows.Count; i++) {
            if (count < 3) {
                if (mellowSize == MellowSize.SMALL && marshmellows[i].mellowSize == MellowSize.SMALL) {
                    Destroy(marshmellows[i].gameObject);
                    removing.Add(i);
                    count++;
                }
                else if (mellowSize == MellowSize.MEDIUM && marshmellows[i].mellowSize == MellowSize.MEDIUM) {
                    Destroy(marshmellows[i].gameObject);
                    removing.Add(i);
                    count++;
                }
            }
        }
        for (int i = 0; i < removing.Count; i++) {
            removing[i] -= i;
            marshmellows.RemoveAt(removing[i]);
        }
        Destroy(gameObject);
    }

    // Splits a marshmellow into smaller marshmellows
    void split() {
        // Creates a new marshmellow for the player
        Marshmellow newPlayer = null;
        if (mellowSize == MellowSize.LARGE) {
            newPlayer = Instantiate(manager.marshMedium, transform.position, transform.rotation).GetComponent<Marshmellow>();
        }
        else if (mellowSize == MellowSize.MEDIUM) {
            newPlayer = Instantiate(manager.marshSmall, transform.position, transform.rotation).GetComponent<Marshmellow>();
        }
        manager.player = newPlayer;
        newPlayer.playerControlled = true;

        // Updates the other marshmellows
        newPlayer.marshmellows = marshmellows;

        // Creates other new marshmellows
        for (int i = 0; i < 3; i++) {
            Marshmellow temp = null;
            if (mellowSize == MellowSize.LARGE) {
                temp = Instantiate(manager.marshMedium, transform.position, transform.rotation).GetComponent<Marshmellow>();
            }
            else if (mellowSize == MellowSize.MEDIUM) {
                temp = Instantiate(manager.marshSmall, transform.position, transform.rotation).GetComponent<Marshmellow>();
            }
            newPlayer.marshmellows.Add(temp);
        }
        Destroy(gameObject);
    }

    // Handles when a player dies and inhabits another marshmellow
    public void die() {
        // If there is another marshmellow to inhabit
        if (marshmellows.Count != 0) {
            Marshmellow temp = marshmellows[0];
            Marshmellow newPlayer = null;
            switch (temp.mellowSize) {
                case MellowSize.SMALL:
                    newPlayer = Instantiate(manager.marshSmall, temp.safePos, temp.transform.rotation).GetComponent<Marshmellow>();
                    break;
                case MellowSize.MEDIUM:
                    newPlayer = Instantiate(manager.marshMedium, temp.safePos, temp.transform.rotation).GetComponent<Marshmellow>();
                    break;
                case MellowSize.LARGE:
                    newPlayer = Instantiate(manager.marshLarge, temp.safePos, temp.transform.rotation).GetComponent<Marshmellow>();
                    break;
            }
            manager.player = newPlayer;
            newPlayer.playerControlled = true;
            newPlayer.GetComponent<Collider>().enabled = true;

            // Updates the other marshmellows
            newPlayer.marshmellows = marshmellows;
            marshmellows.RemoveAt(0);

            // Destroy old marshmellow
            Destroy(temp.gameObject);
            Destroy(gameObject);
        }
        else {

        }
    }

    public void squash() {
        GameObject mellow = Instantiate(manager.marshMedium, transform.position, transform.rotation).gameObject;
        Vector3 temp = mellow.transform.localScale;
        temp.y *= 0.2f;
        mellow.transform.localScale = temp;

        mellow.layer = LayerMask.NameToLayer("Debris");
        mellow.tag = "Untagged";

        mellow.GetComponent<Collider>().enabled = true;
        Destroy(mellow.GetComponent<NavMeshAgent>());
        Destroy(mellow.GetComponent<Marshmellow>());

        die();
    }

    void moveCamera() {
        Vector3 cameraOffset = manager.cameraOffset;
        cameraOffset.x *= direction;
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, transform.position + cameraOffset, ref cameraVelocity, manager.cameraLag);
    }

    void setDestination(Vector3 destination) {
        if (destination != agent.destination) {
            agent.SetDestination(destination);
        }
    }
}
