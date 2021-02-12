using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float lifetime;

    // Start is called before the first frame update
    void Start()
    {
        // Destroys object after specified time
        Destroy(gameObject, lifetime);
    }
}
