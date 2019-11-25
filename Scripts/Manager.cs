using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
    public GameObject marshLarge = null;
    public GameObject marshMedium = null;
    public GameObject marshSmall = null;

    public Marshmellow player = null;

    public Vector3 cameraOffset = new Vector3(5.0f, 1.5f, -10.0f);
    public float cameraRot = 15;
    public float cameraRotSpeed = 1.0f;
    public float cameraLag = 0.75f;
}
