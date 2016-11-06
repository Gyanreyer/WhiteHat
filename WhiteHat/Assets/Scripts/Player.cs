using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    enum PlayerState
    {
        idle,
        running,
        dead
    }

    public float moveSpeed = 5;
    private Camera mainCamera;
    private Rigidbody2D rigidBody;

    private GameObject legs;

    private Vector2 velocity;

    private float lerpTime = 0;

    private PlayerState state;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody2D>();

        legs = GameObject.Find("Legs");
	}
	
	// Update is called once per frame
	void Update () {
        //Move the player with WASD, sets velocity to apply to rigidbody in FixedUpdate
        velocity = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical")).normalized*moveSpeed;

        legs.transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(velocity.y, velocity.x)*Mathf.Rad2Deg - 90);
        legs.transform.position = transform.position + new Vector3(0,0,1);

        if(velocity == Vector2.zero)
        {
            lerpTime += Time.deltaTime;
        }
        else
        {
            lerpTime = .1f;
        }

        

        mainCamera.transform.position=  Vector3.Lerp(mainCamera.transform.position,transform.position - new Vector3(0,0,10),lerpTime);

        //Rotate to face mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,0));

        transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(mousePos.y-transform.position.y,mousePos.x-transform.position.x)*Mathf.Rad2Deg - 90);
	}

    void FixedUpdate()
    {
        rigidBody.velocity = velocity;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        //If got hit by a bullet, die
        if (other.gameObject.tag == "Bullet")
        {
            Debug.Log("Player Died");

            
        }
    }
}
