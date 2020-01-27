using UnityEngine;


public class LoopController : MonoBehaviour 
{
    [System.Serializable]
    public struct Waypoint
    {
        public Vector3 Position;
        public Vector3 Rotation; //final rotation
        public int Frames; //(time) number of frames to reach this waypoint; if 0, then teleport immediately
        public int Delay; //number of frames of staying and doing nothing before starting moving to the next waypoint
    }
        
    public int CurWP = 0; //current waypoint number - waypoint the object is currently standing on (in the Editor)
    [Header("Waypoints")]
    public Waypoint[] Waypoints;


    Vector3 dPos, dRot;    
    int f, d;
    //
    Transform tr;


	void Start () 
	{
        tr = transform;

        if ((CurWP > Waypoints.Length - 1) || (CurWP < 0))
        {
            Debug.LogErrorFormat("{0}: wrong CurWP set in the editor (out of array bounds)", gameObject);
            return;
        }

        if (Waypoints[CurWP].Position != tr.localPosition)
        {
            Debug.LogErrorFormat("{0}'s position does not match it's current waypoint! Fix an error!", gameObject);
            tr.localPosition = Waypoints[CurWP].Position;
        }

        if (Waypoints[CurWP].Rotation != tr.localEulerAngles)
        {
            Debug.LogErrorFormat("{0}'s rotation does not match it's current waypoint! Fix an error!", gameObject);
            tr.localEulerAngles = Waypoints[CurWP].Rotation;
        }

        //start moving to the next waypoint
        PickNextWP();
    }	

	void FixedUpdate () 
	{
		if (d > 0)
        {
            d--;
            return;
        }

        tr.localPosition += dPos;
        tr.localEulerAngles += dRot;
        f--;

        if (f == 0)
        {
            PickNextWP();
        }			
	}

    void PickNextWP()
    {
        if (CurWP == Waypoints.Length - 1)
            CurWP = 0;
        else
            CurWP++;

        Waypoint wp = Waypoints[CurWP];
        d = wp.Delay;
        f = wp.Frames;
        if (f == 0)
        {
            tr.localPosition = wp.Position;
            tr.localEulerAngles = wp.Rotation;
            PickNextWP();
        }
        else
        {
            dPos = (wp.Position - tr.localPosition) / f;
            dRot = (wp.Rotation - tr.localEulerAngles) / f;
        }
    }
}

