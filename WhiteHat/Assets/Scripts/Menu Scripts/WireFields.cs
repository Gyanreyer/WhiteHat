using UnityEngine;
using System.Collections;

public class WireFields : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(0,0,-5 * Time.deltaTime);

        if(transform.position.z < -40)
        {
            transform.Translate(0,0,80);
        }
	}
}
