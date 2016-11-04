using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Script based on helpful tutorial here: https://www.youtube.com/watch?v=rQG9aUWarwE
public class FieldOfView : MonoBehaviour {

    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask playerMask;//Layer mask for player, the way this is set right now you could probably detect multiple players for multiplayer if we wanted to do that
    public LayerMask obstacleMask;//Layer mask for obstacles so that they'll be ignored/treated differently when in view radius than target

    public bool playerVisible = false;//Whether player is visible to this enemy
    public Transform lastKnownPlayerPos;//Last know transform of player, reset to null after a bit if player escapes

    public float meshResolution;//Number of points on mesh per degree, higher=smoother

    public int edgeResolveIterations;//Number of iterations per frame to resolve drawing on edges, higher = smoother look
    public float edgeDistanceThreshhold;
    
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("FindTargetsWithDelay",.2f);
    }
    void LateUpdate()
    {
        DrawFieldOfView();
    }

    //Checks for new targets every given amount of time rather than every single frame
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }



    //Loop through targets that are within radius and see if they're visible within view arc
    void FindVisibleTargets()
    {
        playerVisible = false;

        //Get collider of object within view distance that's also on the player layer - so basically check if the player is in radius
        Collider2D targetInViewRadius = Physics2D.OverlapCircle(new Vector2(transform.position.x,transform.position.y),viewRadius,playerMask);

        //If we got something, do further checks to see if player is within actual view
        if (targetInViewRadius)
        {
            Transform player = targetInViewRadius.transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            //Not 100% sure how transform.forward will work with 2D, guess we'll find out
            if (Vector3.Angle(transform.up, dirToPlayer) < viewAngle / 2)
            {
                float distToPlayer = Vector3.Distance(transform.position, player.position);//Could be used for alert level

                //Send out a raycast to player, if it didn't hit an obstacle then they're officially within view
                if (!Physics2D.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask))
                {
                    playerVisible = true;//Not sure if we need this but I'll have it for now
                    lastKnownPlayerPos = player;
                }
            }
        }
    }

    //Return a direction vector from a given angle
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }

        return new Vector3(Mathf.Cos((angleInDegrees+90)*Mathf.Deg2Rad),Mathf.Sin((angleInDegrees+90) * Mathf.Deg2Rad), 0);
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);//Number of rays to cast per degree

        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for(int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            //If last cast hit an edge and new one didn't or vice versa, we have an edge! Find the point of the edge and add it as a vertice to reduce jitter without tons of unneeded raycasts
            if(i > 0)
            {
                bool edgeDistThreshholdExceeded = Mathf.Abs(oldViewCast.dist - newViewCast.dist) > edgeDistanceThreshhold;//Whether two casts hit but they hit different objects, meaning there is an edge here

                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistThreshholdExceeded))
                {    
                    EdgeInfo edge = FindEdge(oldViewCast,newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;//Number of vertices we're using to form mesh, +1 to include our position
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount-2)*3];//Holds indices for vertices to make mesh, every three indices is a triangle so it's a lot like our model generation HW

        vertices[0] = Vector3.zero;
        
        for(int i = 0; i < vertexCount-1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;//First vert is always origin at our pos
                triangles[i * 3 + 1] = i + 2;//Add other two verts for triangle
                triangles[i * 3 + 2] = i + 1;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    //Take the two points where one cast hit and other didn't and then move inward to zero in as close as possible on the edge
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;

        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        //Bisect the angle between min and max, if they both hit then set that as new min, otherwise set as new max, then repeat to zero in on edge - more loops = more visually accurate
        for(int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistThreshholdExceeded = Mathf.Abs(minViewCast.dist - newViewCast.dist) > edgeDistanceThreshhold;

            if (newViewCast.hit == minViewCast.hit && !edgeDistThreshholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint,maxPoint);

    }


    //Function sends out raycast and returns info
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle,true);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);//Send raycast in given direction

        if(hit)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);//If hit, return info with point where it hit
        }
        else
        {
            return new ViewCastInfo(false,transform.position+dir*viewRadius,viewRadius,globalAngle);//If didn't hit, return info with default point at edge of radius
        }
    }

    //Stores info about raycasts
    public struct ViewCastInfo
    {
        public bool hit;//Whether raycast hit something
        public Vector2 point;//End pt where hit something or stopped
        public float dist;
        public float angle;

        //Constructor for the struct to set values
        public ViewCastInfo(bool _hit,Vector2 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}
