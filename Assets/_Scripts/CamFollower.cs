using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollower : MonoBehaviour
{
    private Quaternion camRotation;
    // Start is called before the first frame update
    void Start()
    {
        camRotation = GetComponentInParent<Transform>().rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(camRotation.x - 90, camRotation.y, 0);
    }
}
