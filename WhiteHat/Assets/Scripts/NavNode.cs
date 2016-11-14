using UnityEngine;
using UnityEditor;//REMOVE THIS WHEN DONE WITH THE DRAW GIZMOS THING
using System.Collections;
using System.Collections.Generic;

public class NavNode : MonoBehaviour {

    [Tooltip("List of connected nodes. Must be set via inspector with drag and drop.")]
    public List<GameObject> neighbors;

    #region Protected fields
    protected NavNode parent;
    protected int dist;
    protected int priority;
    protected bool walkable;
    #endregion

    #region Properties
    public NavNode Parent
    {
        get { return parent; }
        set { parent = value; }
    }
    public int DistFromStart
    {
        get { return dist; }
        set { dist = value; }
    }
    public List<GameObject> Neighbors
    { get { return neighbors; } }
    public bool Walkable
    { get { return walkable; } set { walkable = value; } }
    public int Priority
    { get { return priority; } set { priority = value; } }
    #endregion

    #region Unity Defaults
    void Start()
    {
        dist = 0;
        priority = 0;
        walkable = true;
    }

    /// <summary>
    /// Super duper helpful for setting up AI paths. Comment out if it bothers you.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        foreach (GameObject n in neighbors)
            Gizmos.DrawLine(this.transform.position, n.transform.position);
    }
    #endregion
}
