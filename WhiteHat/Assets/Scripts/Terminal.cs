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

    private bool abilityIsActive;

    private Animator spriteAnimator;

    Player player;

    public float useRadius = 1f;
    
    // Use this for initialization
    void Start () { 
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        spriteAnimator = GetComponent<Animator>();

        //Determine whether ability is active or passive, for now don't worry about it
        //abilityIsActive = Random.value <= .5f;
        abilityIsActive = true;

        if (abilityIsActive)
        {
            thisAbility = (ActiveAbilities)(int)Random.Range(1, ActiveAbilitiesUsesOrDuration.Length);//Pick a random ability from array
            spriteAnimator.Play("orange");
        }
        else
        {
            //Blah blah blah, figure out later but make ability passive
            spriteAnimator.Play("blue");
        }


            
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

        //Change this terminal's sprite to indicate it's off
        spriteAnimator.Play("off");

        Destroy(this);//Can't interact with this anymore ever because just deleting this script
    }
    
}
