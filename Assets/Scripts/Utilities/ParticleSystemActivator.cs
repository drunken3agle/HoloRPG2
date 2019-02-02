using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemActivator : MonoBehaviour {

    private void OnEnable()
    {
        Debug.Log("Enabling PS");
        GetComponent<ParticleSystem>().Play();
    }
}
