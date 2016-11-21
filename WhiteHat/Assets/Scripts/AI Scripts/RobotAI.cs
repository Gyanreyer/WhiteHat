using UnityEngine;
using System.Collections;

public class RobotAI : Vehicle {

    [Tooltip("Path the robot should follow when it's patroling. Use NavNode objects. Note that the robot will always start at the first node.")]
    public GameObject[] patrolRoute;
    [Tooltip("What the robot should do when it's done with its path route.")]
    public PathTypes pathType;
    [Tooltip("How close the robot should be to its node to go to the next one.")]
    public float minDistToNodeToContinue = 4f;
    [Tooltip("How aggressively the robots will steer around when an alert is raised.")]
    public float huntingForceScale = 4f;

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
    //current node in hunt path
    private GameObject currentHuntNode;
    //Time to trigger an alarm, in seconds
    private float timeToAlert = 2f;
    //Time spent staring at the player
    private float spottingTime = 0f;
    //Path that will be generated via A-star when the robot goes off-course looking for the player
    [SerializeField]
    private GameObject[] recoveryRoute;
    //Path for when the robot has to get to the player on an alert
    [SerializeField]
    private GameObject[] huntingRoute;
    //Bool that will be on when recovering from going off the patrol route
    private bool recovering = false;
    //Bool that will be on when following the hunt path
    private bool hunting = false;
    //The current index in the path
    private int currentNodeIndex = 0;
    //The current index in the recovery path
    private int currentRecoveryIndex = 0;
    //current index in hunt path
    private int currentHuntIndex = 0;
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
    //current fire time
    private float fireTime = 0f;
    //temporary thing for making inspecting not look stupid
    private Vector3 addedOffsetPoint = Vector3.zero;
    #endregion
    #region Properties
    public float TimeToAlert
    {
        set { timeToAlert = value; }
    }
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
            //store the vec to the player
            vecToPlayer = playerObj.transform.position - this.gameObject.transform.position;
            //Get forward dot for distance
            float forwardDot = Vector3.Dot(this.transform.up, vecToPlayer);
            //increase time staring at player, modified inversely by distance
            spottingTime += (Time.deltaTime / forwardDot) + enemyMan.SpotTimeAddedConstant;
            //check to see if you should trigger an alarm
            if (spottingTime >= timeToAlert || enemyMan.AlertState == EnemyManager.AlertStates.Alarmed || enemyMan.AlertState == EnemyManager.AlertStates.Searching)
                enemyMan.TriggerAlarm();
            //seek the player
            force += Seek(new Vector2(playerObj.transform.position.x, playerObj.transform.position.y));
            //Update state based on alert state
            switch (enemyMan.AlertState)
            {
                case EnemyManager.AlertStates.Patrol:
                    robotState = RobotStates.Inspecting;
                    inspectPoint = new Vector2(playerObj.transform.position.x, playerObj.transform.position.y);
                    break;
                case EnemyManager.AlertStates.Alarmed:
                    //Update last known loc
                    enemyMan.LastKnownLocation = playerObj.transform.position;
                    robotState = RobotStates.Hunting;
                    break;
                case EnemyManager.AlertStates.Searching:
                    robotState = RobotStates.Hunting;
                    break;
            }
        }
        else//if you don't see the player, either patrol or search for the player if alerted
        {
            //reset spotting time
            spottingTime = 0;
            //Update state based on alert state
            switch (enemyMan.AlertState)
            {
                case EnemyManager.AlertStates.Patrol:
                    //Need to recover if you were searching for the player
                    if (robotState == RobotStates.Searching)
                    {
                        //now we're on the recovery path...
                        recovering = true;
                        //need to find closest node
                        recoveryRoute = enemyMan.GenerateAStarPath(enemyMan.FindClosestNode(this.transform.position).GetComponent<NavNode>(), patrolRoute[currentNodeIndex].GetComponent<NavNode>());
                        if (recoveryRoute.Length != 0)
                            currentRecoveryNode = recoveryRoute[0];
                        else
                            recovering = false;//failsafe in case things go horribly wrong
                    }
                    robotState = RobotStates.Patroling;
                    break;
                case EnemyManager.AlertStates.Alarmed:
                    if (!Physics2D.Raycast(this.transform.position, vecToPlayer, 100, wallLayer))
                    {
                        if (robotState == RobotStates.Patroling || robotState == RobotStates.Inspecting)
                        {
                            //if you were minding your own business when you were alerted...find a path to the player's last known loc if you can't see them already
                            huntingRoute = enemyMan.GenerateAStarPath(enemyMan.FindClosestNode(this.transform.position).GetComponent<NavNode>(), enemyMan.ClosestNodeToLastKnownPlayerLoc.GetComponent<NavNode>());//whew, this is kinda messy
                            hunting = true;
                            if (huntingRoute.Length != 0)
                                currentHuntNode = huntingRoute[0];
                            else
                                hunting = false;
                        }
                    }
                    else
                    {
                        hunting = false;
                    }
                    robotState = RobotStates.Hunting;
                    break;
                case EnemyManager.AlertStates.Searching:
                    robotState = RobotStates.Searching;
                    break;
            }
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
    #region Methods
    private void HandleInspecting()
    {
        //Seek inspect point
        force += Seek(new Vector2(inspectPoint.x, inspectPoint.y));
        //Update mesh color based on alert state
        if (enemyMan.AlertState == EnemyManager.AlertStates.Patrol)
            render.material.SetColor("_Color", new Color(255, 255, 0, 0.4f));
        else
            render.material.SetColor("_Color", new Color(255, 0, 0, 0.4f));
    }

    private void HandleSearching()
    {
        //Think I'll probably redo this but I'm not sure if I'll need a Searching enum...
        if (hunting)
        {
            FollowHuntRoute();
        }
        else
        {
            //Seek last known location
            force += Seek(enemyMan.LastKnownLocation + addedOffsetPoint);
            if (Vector3.SqrMagnitude(this.transform.position - (enemyMan.LastKnownLocation + addedOffsetPoint)) < Mathf.Pow(minDistToNodeToContinue, 2))
                addedOffsetPoint = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 0);
        }
    }

    private void HandleHunting()
    {
        if(hunting)
        {
            FollowHuntRoute();
        }
        else
        {
            //Seek last known location
            force += Seek(enemyMan.LastKnownLocation + addedOffsetPoint) * huntingForceScale;
            if (Vector3.SqrMagnitude(this.transform.position - (enemyMan.LastKnownLocation + addedOffsetPoint)) < Mathf.Pow(minDistToNodeToContinue, 2))
                addedOffsetPoint = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 0);
        }
        //Shoot player if within sight range
        if (fov.playerVisible)
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

    private void FollowHuntRoute()
    {
        //if dist to current node < dist needed
        if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.y), new Vector2(currentHuntNode.transform.position.x, currentHuntNode.transform.position.y)) < minDistToNodeToContinue)
        {
            //look for the next node
            ++currentHuntIndex;
            //if you've hit the last one,
            if (currentHuntIndex >= huntingRoute.Length)
            {
                //stop recovering
                hunting = false;
                //get outta here
                return;
            }
            //seek the next node
            currentHuntNode = huntingRoute[currentHuntIndex];
        }
        //follow the path
        force += Seek(new Vector2(currentHuntNode.transform.position.x, currentHuntNode.transform.position.y));
    }

    protected override void CalcSteeringForces()
    {
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
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, this.transform.position + (this.transform.up * 2), Quaternion.identity);
        bullet.GetComponent<Bullet>().setUp(vecToPlayer);
    }
    #endregion
}
