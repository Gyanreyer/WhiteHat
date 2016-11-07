using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public Vector3 direction;
    public float moveSpeed;

    private Rigidbody2D rigidBody;
	
    public void setUp(Vector3 dir)
    {
        direction = dir;

        rigidBody.velocity = direction.normalized * moveSpeed;
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(gameObject);
    }

}

