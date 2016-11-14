using UnityEngine;
using System.Collections;
using System;

public class RobotAI : Vehicle {

    [Tooltip("Path the robot should follow when it's patroling. Use NavNode objects. Note that the robot will always start at the first node.")]
    public GameObject[] patrolRoute;
    [Tooltip("What the robot should do when it's done with its path route.")]
    public PathTypes pathType;
    [Tooltip("How close the robot should be to its node to go to the next one.")]
    public float minDistToNodeToContinue = 4f;

    #region Private Fields
    //Enum of possible states
    private enum RobotStates
    {
        Patroling,
        Inspecting,
        Searching,
        Hunting,
    }
    //Current camera state
    private RobotStates robotState = RobotStates.Patroling;
    //Enum of possible paths
    public enum PathTypes
    {
        ReverseWhenDone,
        LoopBackToFirst,
    }
    //Used for reverseWhenDone
    private bool increasingNodeIndex = true;
    //A reference to the player
    private GameObject playerObj;
    //Reference to the enemy manager
    private EnemyManager enemyMan;
    //A vector from here to the player
    private Vector3 vecToPlayer;
    //Current node in the path
    private GameObject currentNode;
    //Current node in the recovery path
    private GameObject currentRecoveryNode;
    //Time to trigger an alarm, in seconds
    private float timeToAlert = 2f;
    //Time spent staring at the player
    private float spottingTime = 0f;
    //Path that will be generated via A-star when the robot goes off-course looking for the player
    private GameObject[] recoveryRoute;
    //Bool that will be on when recovering from going off the patrol route
    private bool recovering = false;
    //The current index in the path
    private int currentNodeIndex = 0;
    //The current index in the recovery path
    private int currentRecoveryIndex = 0;
    //Point that the robot will go over to inspect
    private Vector2 inspectPoint = Vector2.zero;
    //Renderer for the FOV mesh
    private Renderer render;
    //Field of view for camera, mainly handles visualation but also returns if player is in sight
    private FieldOfView fov;
    //Prefab for the bullet
    public GameObject bulletPrefab;
    //Shots per second
    public float fireRate;

    private float fireTime = 0f;
    #endregion

    #region Unity Defaults
    override public void Start()
    {
        base.Start();
        force = Vector2.zero;
        currentNode = patrolRoute[0];
        currentRecoveryNode = null;
        fov = this.GetComponent<FieldOfView>();
        render = this.transform.GetChild(0).GetComponent<Renderer>();
        render.material.SetColor("_Color", new Color(221, 221, 221, 0.4f));
        playerObj = GameObject.FindGameObjectWithTag("Player");
        enemyMan = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
    }

    override public void Update()
    {
        //set view mesh color based on alert state
        switch (enemyMan.AlertState)
        {
            case EnemyManager.AlertStates.Patrol:
                render.material.SetColor("_Color", new Color(221, 221, 221, 0.4f));
                break;
            case EnemyManager.AlertStates.Alarmed:
                render.material.SetColor("_Color", new Color(255, 0, 0, 0.4f));
                break;
            case EnemyManager.AlertStates.Searching:
                render.material.SetColor("_Color", new Color(255, 255, 0, 0.4f));
                break;
        }
        //Seek the player if you can see him, attack if you're alerted
        if (fov.playerVisible)
        {
            force += Seek(new Vector2(playerObj.transform.position.x, playerObj.transform.position.y));
            //Update state based on alert state
            switch (enemyMan.AlertState)
            {
                case EnemyManager.AlertStates.Patrol:
                    robotState = RobotStates.Inspecting;
                    inspectPoint = new Vector2(playerObj.transform.position.x, playerObj.transform.position.y);
                    break;
                case EnemyManager.AlertStates.Alarmed:
                    robotState = RobotStates.Hunting;
                    break;
                case EnemyManager.AlertStates.Searching:
                    robotState = RobotStates.Hunting;
                    break;
            }
        }
        else//if you don't see the player, either patrol or search for the player if alerted
        {
            if (enemyMan.AlertState == EnemyManager.AlertStates.Patrol)
            {
                //Need to recover if you go off the path
                if (robotState == RobotStates.Inspecting)
                {
                    //now we're on the recovery path...
                    recovering = true;
                    //need to find closest node
                    recoveryRoute = enemyMan.GenerateAStarPath(enemyMan.FindClosestNode(this.transform.position).GetComponent<NavNode>(), patrolRoute[currentNodeIndex].GetComponent<NavNode>());
                    if (recoveryRoute.Length != 0)
                        currentRecoveryNode = recoveryRoute[0];
                    else
                        recovering = false; //NOTE: this is a hacky solution to what may be an issue with the AStar class. Still working on it...
                }
                robotState = RobotStates.Patroling;
            }
            else
                robotState = RobotStates.Searching;
        }
        //Handle states
        switch (robotState)
        {
            case RobotStates.Patroling:
                HandlePatroling();
                break;
            case RobotStates.Inspecting:
                HandleInspecting();
                break;
            case RobotStates.Searching:
                HandleSearching();
                break;
            case RobotStates.Hunting:
                HandleHunting();
                break;
        }
        //Do the vehicle update
        base.Update();
    }
    #endregion

    private void HandleInspecting()
    {
        //Seek inspect point
        force += Seek(new Vector2(inspectPoint.x, inspectPoint.y));
        //Update mesh color based on alert state
        if (enemyMan.AlertState == EnemyManager.AlertStates.Patrol)
            render.material.SetColor("_Color", new Color(255, 255, 0, 0.4f));
        else
            render.material.SetColor("_Color", new Color(255, 0, 0, 0.4f));
        //avoid walls
    }

    private void HandleSearching()
    {

    }

    private void HandleHunting()
    {
        //Seek player
        force += Seek(new Vector2(playerObj.transform.position.x, playerObj.transform.position.y));
        //Shoot player
        ShootPlayer();
    }

    private void HandlePatroling()
    {
        if (recovering)
            FollowRecoveryRoute();
        else
            FollowPatrolRoute();
    }

    private void FollowRecoveryRoute()
    {
        //if dist to current node < dist needed
        if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.y), new Vector2(currentRecoveryNode.transform.position.x, currentRecoveryNode.transform.position.y)) < minDistToNodeToContinue)
        {
            //look for the next node
            ++currentRecoveryIndex;
            //if you've hit the last one,
            if (currentRecoveryIndex >= recoveryRoute.Length)
            {
                //stop recovering
                recovering = false;
                //get outta here
                return;
            }
            //seek the next node
            currentRecoveryNode = recoveryRoute[currentRecoveryIndex];
        }
        //follow the path
        force += Seek(new Vector2(currentRecoveryNode.transform.position.x, currentRecoveryNode.transform.position.y));
    }

    private void FollowPatrolRoute()
    {
        //if dist to current node < dist needed
        if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.y), new Vector2(currentNode.transform.position.x, currentNode.transform.position.y)) < minDistToNodeToContinue)
        {
            //Increment or decrement node index
            if (pathType == PathTypes.LoopBackToFirst)
                ++currentNodeIndex;
            else if (pathType == PathTypes.ReverseWhenDone)
            {
                if (increasingNodeIndex)
                    ++currentNodeIndex;
                else
                    --currentNodeIndex;
            }
            //keep within max
            if (currentNodeIndex == patrolRoute.Length)
            {
                if (pathType == PathTypes.LoopBackToFirst)
                    currentNodeIndex = 0;
                else if (pathType == PathTypes.ReverseWhenDone)
                {
                    --currentNodeIndex;
                    increasingNodeIndex = false;
                }
            }
            else if (currentNodeIndex < 0)
            {
                ++currentNodeIndex;
                increasingNodeIndex = true;
            }
            //Set current node to next
            currentNode = patrolRoute[currentNodeIndex];
        }
        //seek next node
        force += Seek(new Vector2(currentNode.transform.position.x, currentNode.transform.position.y));
    }

    protected override void CalcSteeringForces()
    {
        /*
        force += SeparationV2() * separationWeight;
        force += WallFollow(wallRaycastDist, wallNormalDist) * wallFollowWeight;
        */

        //Keep the force from going out of bounds
        force = Vector3.ClampMagnitude(force, maxForce);
        //Apply net force
        ApplyForce(force);
        //Reset the force for the next frame
        force = Vector3.zero;
    }

    private void ShootPlayer()
    {
        if (fireTime < 1 / fireRate)
        {
            fireTime += Time.deltaTime;
            return;
        }

        fireTime = 0;
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, transform.position - new Vector3(0, 0, 2), Quaternion.identity);
        bullet.GetComponent<Bullet>().setUp(vecToPlayer);

    }
}
