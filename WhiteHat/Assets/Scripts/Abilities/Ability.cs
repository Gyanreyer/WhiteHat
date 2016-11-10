using UnityEngine;
using System.Collections;

public abstract class Ability {

    protected Player player;

    protected int usesLeft;

    public abstract void Use();

}
