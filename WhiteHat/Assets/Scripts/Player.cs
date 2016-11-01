using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float moveSpeed = 5;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //Move the player with WASD
        transform.position += new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0).normalized*moveSpeed*Time.deltaTime;


        //Rotate to face mouse
        Vector3 mousePos = Input.mousePosition;

        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

        transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(mousePos.y-transform.position.y,mousePos.x-transform.position.x)*Mathf.Rad2Deg - 90);

	}
}
