using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarGraph  {

    // Fields
    GameObject[] verts;
    PriorityQueue pq;

    //Constructor
    public AStarGraph()
    {
        pq = new PriorityQueue(); //???
        //pq = this.gameObject.AddComponent<PriorityQueue>(); //?!?!?!?
        SetUpGraph();
    }

    // Properties
    public GameObject[] Verts
    { get { return verts; } }
    public PriorityQueue PQ
    { get { return pq; } }

    // Methods
    public void SetUpGraph()
    {
        verts = GameObject.FindGameObjectsWithTag("NavNodes");
    }

    public GameObject[] AStar(NavNode start, NavNode end)
    {
        // Boolean to monitor pathfinding success
        bool success = true;
        // Closed vertex list
        List<GameObject> closed = new List<GameObject>();
        // Actual path list
        List<GameObject> path = new List<GameObject>();
        // Create current vertex variable
        NavNode current = start;
        // Add the vertex to the queue
        pq.Enqueue(current);
        // Start the loop
        while (pq.Peek() != end)
        {
            // If a path could not be found...
            if (pq.Peek() == null)
            {
                // Give up
                success = false;
                break;
            }
            // Store and remove top vertex
            current = pq.Dequeue();
            // Add it to the closed list
            closed.Add(current.gameObject);
            // Heuristic variable
            float diagonal = 0;
            // Queue up neighbors of top vertex
            foreach (GameObject vert in current.Neighbors)
            {
                NavNode node = vert.GetComponent<NavNode>();
                // Here, "1" is the distance from the current vert to its neighbor. For this graph, it will always be 1 block away
                // ... not anymore! Thanks for the comment, freshman me. I hope you aren't lying
                float cost = current.DistFromStart + (current.transform.position - node.transform.position).sqrMagnitude;
                // Remove far away vertices
                if (cost < node.DistFromStart)
                {
                    if (pq.Contains(node))
                        pq.Remove(node);//THIS MAY BE CAUSING ISSUES... not 100% sure
                    if (closed.Contains(vert))
                        closed.Remove(vert);
                }
                // Adding to the priority queue
                if (!closed.Contains(vert) && !pq.Contains(node) && node.Walkable)
                {
                    node.DistFromStart = cost;
                    diagonal = (end.transform.position - vert.transform.position).sqrMagnitude; //Mathf.Pow(end.transform.position.x - vert.transform.position.x, 2) - Mathf.Pow(end.transform.position.y - vert.transform.position.y, 2);
                    node.Priority = node.DistFromStart + diagonal;
                    node.Parent = current;
                    node.Walkable = false;
                    pq.Enqueue(node);
                }
            }
        }
        // Get the actual path list... if it exists
        if (success)
        {
            current = end;
            while (current != start)
            {
                current = current.Parent;
                path.Add(current.gameObject);
            }
        }
        //Reset the walkable values and empty the PQ
        ResetGraph();
        //Reverse the path (because it's backwards by default
        path.Reverse();
        //return the path
        return path.ToArray();
    }

    public void ResetGraph()
    {
        foreach (GameObject vert in verts)
            vert.GetComponent<NavNode>().Walkable = true;
        // Empty the priority queue
        while (pq.Dequeue() != null) ;
    }
}
