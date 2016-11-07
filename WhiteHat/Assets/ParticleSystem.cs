using UnityEngine;
using System.Collections;

public class ParticleSystem : MonoBehaviour {

    public float lifeTime= 1;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, lifeTime);
        transform.eulerAngles += new Vector3(-90,0,0);
	}
	
}
