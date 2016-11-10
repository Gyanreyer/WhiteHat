using UnityEngine;
using System.Collections;

public class Terminal : MonoBehaviour {

    Ability thisAbility;

    Player player;

    public float useRadius = 1f;
    bool used;

    // Use this for initialization
    void Start () { 
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        Ability[] abilities = { new Invisibility(player) };//Array of all possible abilities, should move to some kind of global manager

        thisAbility = abilities[(int)Random.Range(0, abilities.Length)];//Pick a random ability from array
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 vecToPlayer = player.transform.position - transform.position;

        if(vecToPlayer.magnitude < useRadius && Input.GetKeyDown(KeyCode.E))
        {
            Activate();
        }
	}

    void Activate()
    {
        //Give ability to player
        player.AddActiveAbility(thisAbility);

        used = true;
        //Other stuff to deactivate, change this terminal's sprite
        

        Destroy(this);
    }
    
}
