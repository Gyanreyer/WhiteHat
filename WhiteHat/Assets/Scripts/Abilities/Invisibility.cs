using UnityEngine;
using System.Collections;

public class Invisibility : Ability {

    public Invisibility(Player p)
    {
        player = p;
    }

	// Update is called once per frame
	public override void Use ()
    {
        if (usesLeft <= 0) return;

        usesLeft--;

        //Other code here
        player.gameObject.layer = 0;
	}
}
