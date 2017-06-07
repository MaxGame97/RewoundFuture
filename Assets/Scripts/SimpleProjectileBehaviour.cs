using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleProjectileBehaviour : MonoBehaviour {
    
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float lifeTime;

    // Use this for initialization
    void Start()
    {
        // Set the projectile to destroy itself after the specified lifetime
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update ()
    {
        // Translate the projectile forward, based on its speed
        transform.Translate(Vector2.up * projectileSpeed * Time.deltaTime);
	}
}
