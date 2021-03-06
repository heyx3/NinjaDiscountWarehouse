﻿using UnityEngine;


/// <summary>
/// Follows a path described by PathNode objects.
/// </summary>
public class PathFollower : MonoBehaviour
{
    /// <summary>
    /// The current node. This PathFollower is currently moving to the node after this one.
    /// </summary>
    public PathNode Current = null;

    public bool MovingBackwards = false;
    public float BaseSpeed = 20.0f;

    /// <summary>
    /// Allows access to this object's transform without the
    /// performance penalty that the "transform" property incurs.
    /// </summary>
    public Transform MyTransform { get; private set; }
	/// <summary>
	/// The current velocity from this component's pathing behavior.
	/// </summary>
	public Vector3 Velocity { get; private set; }

    /// <summary>
    /// Raised if this follower hits the end of its path and changes direction.
    /// </summary>
    public event System.EventHandler OnPathEnd;


    void Awake()
    {
        MyTransform = transform;

		Velocity = Vector3.zero;

        if (Current != null)
            MyTransform.position = Current.transform.position;
    }

    void FixedUpdate()
    {
		Vector3 oldPos = MyTransform.position;


        if (Current != null && (Current.Next != null || Current.Previous != null))
        {
            //Move towards the next node.
            Vector3 newPos;
            float moveDist = BaseSpeed * Time.fixedDeltaTime;
            if (Current.MoveTowardsNext(MyTransform.position, moveDist, MovingBackwards, out newPos))
            {
                //First, get the next target node.
                Current = Current.GetNextNode(MovingBackwards);
				bool hitEnd = Current.GetNextNode(MovingBackwards) == null;
                if (hitEnd)
                {
                    MovingBackwards = !MovingBackwards;
                    if (OnPathEnd != null) OnPathEnd(this, new System.EventArgs());
                }

                //Since this cube just snapped to a new node, there may be a bit of movement left.
				if (!hitEnd)
				{
					float extraMovement = (moveDist / Current.SpeedModifier) -
										  Vector3.Distance(MyTransform.position, newPos);
					if (extraMovement > 0.0f)
					{
						Vector3 newNewPos;
						Current.MoveTowardsNext(newPos, extraMovement, MovingBackwards, out newNewPos);
						MyTransform.position = newNewPos;
					}
				}
            }
            else
            {
                MyTransform.position = newPos;
            }
        }


		Velocity = (MyTransform.position - oldPos) / Time.deltaTime;
    }
}