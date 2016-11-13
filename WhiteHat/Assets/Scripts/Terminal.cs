using UnityEngine;
using System.Collections;

public class Terminal : MonoBehaviour {

    ActiveAbilities thisAbility;//Ability that this terminal gives

    //Each float corresponds with ability at that index
    private float[] ActiveAbilitiesUsesOrDuration = {
        0,//None
        5//,//Invisible, lasts 5 seconds
        //5//Dash, lasts 5 seconds
    };

    Player player;

    public float useRadius = 1f;

    
    // Use this for initialization
    void Start () { 
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        thisAbility = (ActiveAbilities)(int)Random.Range(1, ActiveAbilitiesUsesOrDuration.Length);//Pick a random ability from array
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
        player.AddActiveAbility(thisAbility, ActiveAbilitiesUsesOrDuration[(int)thisAbility]);

        //Other stuff to deactivate, change this terminal's sprite
        

        Destroy(this);//Can't interact with this anymore ever because just deleting this script
    }
    
}
