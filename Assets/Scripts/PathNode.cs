using UnityEngine;


/// <summary>
/// Represents a point on a path for something to follow.
/// </summary>
public class PathNode : MonoBehaviour
{
    public PathNode Next = null,
                    Previous = null;
    private Transform thisT, prevT, nextT;

    /// <summary>
    /// The different movement paths that can be taken from one node to the next.
    /// </summary>
    public enum MoveStyles
    {
        Line,
        ArcLeft,
        ArcRight,
    }

    /// <summary>
    /// The movement style for moving from this node to the next one.
    /// </summary>
    public MoveStyles StyleToNext = MoveStyles.Line;

    /// <summary>
    /// Scales a PathFollower's speed.
    /// </summary>
    public float SpeedModifier = 1.0f;

	/// <summary>
	/// The axis of rotation for any movement styles that involve rotation (e.x. ArcLeft/ArcRight).
	/// </summary>
	public Vector3 RotationAxis = new Vector3(0, 1, 0);


    /// <summary>
    /// Allows access to this component's transform
    /// without the performance penalty that the "transform" property incurs.
    /// </summary>
    public Transform MyTransform { get { return thisT; } }

    /// <summary>
    /// Gets the next node in this path.
    /// </summary>
    /// <param name="moveBackwards">Whether or not to move backwards through the path.</param>
    public PathNode GetNextNode(bool moveBackwards) { return (moveBackwards ? Previous : Next); }
    /// <summary>
    /// Gets the previous node in this path.
    /// </summary>
    /// <param name="moveBackwards">Whether or not to move backwards through the path.</param>
    public PathNode GetPreviousNode(bool moveBackwards) { return (moveBackwards ? Next : Previous); }
    /// <summary>
    /// Gets the final node in this path.
    /// </summary>
    /// <param name="moveBackwards">Whether or not to move backwards through the path.</param>
    public PathNode GetPathEnd(bool moveBackwards)
    {
        return (moveBackwards ? (Previous == null ? this : Previous.GetPathEnd(true)) :
                                (Next == null ? this : Next.GetPathEnd(false)));
    }
    /// <summary>
    /// Gets the first node in this path.
    /// </summary>
    /// <param name="moveBackwards">Whether or not to move backwards through the path.</param>
    public PathNode GetPathStart(bool moveBackwards)
    {
        return (moveBackwards ? (Next == null ? this : Next.GetPathStart(true)) :
                                (Previous == null ? this : Previous.GetPathStart(false)));
    }


    void Awake()
    {
        thisT = transform;
        if (Next != null) nextT = Next.transform;
        else nextT = null;
        if (Previous != null) prevT = Previous.transform;
        else prevT = null;
    }

    
    void OnDrawGizmos()
    {
        thisT = transform;
        if (Next != null) nextT = Next.transform;
        if (Previous != null) prevT = Previous.transform;


        Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        for (int i = 0; i < 2; ++i)
        {
            PathNode node = (i == 0 ? Next : Previous);
            if (node == null) continue;

            Transform tr = node.transform;

            MoveStyles style;
			Vector3 rotAxis;
            if (i == 0)
            {
                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
				rotAxis = RotationAxis.normalized;
                style = StyleToNext;
            }
            else
            {
                Gizmos.color = new Color(0.75f, 0.75f, 0.75f, 0.75f);
				rotAxis = node.RotationAxis.normalized;
                switch (node.StyleToNext)
                {
                    case MoveStyles.Line: style = MoveStyles.Line; break;
                    case MoveStyles.ArcRight: style = MoveStyles.ArcLeft; break;
                    case MoveStyles.ArcLeft: style = MoveStyles.ArcRight; break;

                    default: throw new System.NotImplementedException();
                }
            }


            //Draw the path the special cube will take.
            switch (style)
            {
                //Just draw the straight-line path.
                case MoveStyles.Line:
                    Gizmos.DrawLine(transform.position, tr.position);
                    break;

                //Split the arc into segments and draw each segment.
                case MoveStyles.ArcLeft:
                case MoveStyles.ArcRight:

                    Vector3 center = (thisT.position + tr.position) * 0.5f;
                    float radius = Vector3.Distance(center, thisT.position);
                    bool flip = (style == MoveStyles.ArcRight);

                    Vector3 startDelta = thisT.position - center;
                    int segments = Mathf.RoundToInt(1 + Mathf.Log(2000.0f * radius, 2.0f));

                    float angleIncrement = 180.0f / (float)segments;
                    for (int j = 0; j < segments; ++j)
                    {
                        float angleStart = j * angleIncrement * (flip ? -1.0f : 1.0f),
                              angleEnd = (j + 1) * angleIncrement * (flip ? -1.0f : 1.0f);

                        Vector3 start = center + (Quaternion.AngleAxis(angleStart, rotAxis) * startDelta),
                                end = center + (Quaternion.AngleAxis(angleEnd, rotAxis) * startDelta);

                        Gizmos.DrawLine(start, end);
                    }

                    break;

                default: throw new System.NotImplementedException();
            }
        }

        Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.75f);
        Gizmos.DrawSphere(thisT.position, 0.75f);
    }
    
    /// <summary>
    /// Moves along the path from this node to the next (or previous) one,
    /// given input data such as current position and movement speed.
    /// Returns whether the next node was reached.
    /// </summary>
    /// <param name="baseMoveDist">The farthest this function can move from the given current position.
    /// This value will be multiplied by this node's speed modifier.</param>
    /// <param name="reverseDirection">If true, moves towards the previous node instead of the next one.</param>
    /// <param name="newPos">Gets set to the new position after the movement.</param>
    public bool MoveTowardsNext(Vector3 current, float baseMoveDist, bool reverseDirection, out Vector3 newPos)
    {
        //Default output values to appease the compiler.
        newPos = current;
        bool hitNode = false;


        //Calculate data that depends on whether the direction is reversed.
        Transform destination;
        MoveStyles style;
		Vector3 rotAxis;
        if (reverseDirection)
        {
            destination = prevT;
			rotAxis = Previous.RotationAxis.normalized;
            switch (Previous.StyleToNext)
            {
                case MoveStyles.Line:
                    style = MoveStyles.Line;
                    break;
                case MoveStyles.ArcLeft:
                    style = MoveStyles.ArcRight;
                    break;
                case MoveStyles.ArcRight:
                    style = MoveStyles.ArcLeft;
                    break;

                default: throw new System.NotImplementedException();
            }
            baseMoveDist *= Previous.SpeedModifier;
        }
        else
        {
            destination = nextT;
            style = StyleToNext;
			rotAxis = RotationAxis.normalized;
            baseMoveDist *= SpeedModifier;
        }


        //Compute the movement.
        switch (style)
        {
            case MoveStyles.Line:

                //If the current position is close enough, snap to the destination node.
                if (Vector3.Distance(current, destination.position) <= baseMoveDist)
                {
                    newPos = destination.position;
                    hitNode = true;
                }
                //Otherwise, move straight towards the destination like normal.
                else
                {
                    newPos = current + (baseMoveDist * (destination.position - current).normalized);
                    hitNode = false;
                }

                break;


            case MoveStyles.ArcLeft:
            case MoveStyles.ArcRight:

                bool flip = (style == MoveStyles.ArcRight);
                float diameter = Vector3.Distance(thisT.position, destination.position);

                //Get the max angle change for this frame based on the movement speed and delta time.
                float arcCircumference = Mathf.PI * diameter * 0.5f;
                float maxAngleDelta = 180.0f * (baseMoveDist / arcCircumference);
                if (flip) maxAngleDelta *= -1.0f;

                //If the destination is close enough to move there this frame, snap to it.
                if (Vector3.Distance(destination.position, current) < baseMoveDist)
                {
                    newPos = destination.position;
                    hitNode = true;
                    return hitNode;
                }
                
                //Otherwise, rotate towards it.
                Vector3 center = (thisT.position + destination.position) * 0.5f;
                Vector3 fromCenter = current - center;
                fromCenter = fromCenter.normalized * diameter * 0.5f;
                newPos = center + (Quaternion.AngleAxis(maxAngleDelta, rotAxis) * fromCenter);
                hitNode = false;

                break;


            default: throw new System.NotImplementedException();
        }

        return hitNode;
    }
}