using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    private Vector3 direction;
    public float moveSpeed;

    private Rigidbody2D rigidBody;
	
    public void setUp(Vector3 startPos, Vector3 dir)
    {
        transform.position = startPos;
        direction = dir;

        rigidBody.velocity = direction.normalized * moveSpeed;
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }


}

