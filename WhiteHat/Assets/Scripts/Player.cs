using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float moveSpeed = 5;
    private Camera mainCamera;
    private Rigidbody2D rigidBody;

    private Vector2 velocity;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        //Move the player with WASD, sets velocity to apply to rigidbody in FixedUpdate
        velocity = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0).normalized*moveSpeed;

        //Rotate to face mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,0));

        transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(mousePos.y-transform.position.y,mousePos.x-transform.position.x)*Mathf.Rad2Deg - 90);
	}

    void FixedUpdate()
    {
        rigidBody.velocity = velocity;
    }
}
