using System;
using UnityEngine;


/// <summary>
/// Handles player movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
	public Vector2 HorizontalMovementInput { get { return MyMovementController.MovementInput; } }

    public float Acceleration = 10.0f;
    public Vector3 Velocity = Vector3.zero;
    public float MaxSpeed = 20.0f;
	public float FrictionMultiplier = 0.92f;


	public Transform MyTransform { get; private set; }
	public CharacterController MyCharController { get; private set; }
	public PlayerInput MyMovementController { get; private set; }


    void Awake()
    {
        MyCharController = GetComponent<CharacterController>();
        MyTransform = transform;
		MyMovementController = GetComponent<PlayerInput>();
    }


	void FixedUpdate()
	{
		//Interpret movement input.
		Vector3 worldMovement = (MyTransform.right * HorizontalMovementInput.x) + (MyTransform.forward * HorizontalMovementInput.y);
		Vector3 accel = worldMovement * Acceleration;

		//Update the velocity.
		Vector3 newVel = Velocity + (accel * Time.fixedDeltaTime);


		//Apply friction.
		//If the player is accelerating, apply friction to the part of his velocity perpendicular to his acceleration.
		//Otherwise, if the player is moving, apply it to his whole velocity.
		//Otherwise, the player is standing still, so don't apply any friction.
		float accelSqr = accel.sqrMagnitude,
			  velSqr = newVel.sqrMagnitude;

		//'frictionDir' is the direction that gets NO friction.
		Vector3 frictionDir = new Vector3(float.NaN, float.NaN, float.NaN);
		if (accelSqr > 0.0f)
		{
			frictionDir = accel.normalized;
		}
		else if (velSqr > 0.0f)
		{
			frictionDir = Vector3.Cross(newVel.normalized, Vector3.up);
		}

		if (!float.IsNaN(frictionDir.x))
		{
			//Get the components of velocity parallel/perpendicular to the friction dir.
			Vector3 parallel = frictionDir * Vector3.Dot(newVel, frictionDir),
					perp = newVel - parallel;
			//Leave the parallel component alone, but apply friction to the perpendicular one.
			perp *= FrictionMultiplier;

			newVel = parallel + perp;
		}

		Velocity = newVel;


		//Constrain the velocity.
		if (Velocity.sqrMagnitude > MaxSpeed * MaxSpeed)
		{
			Velocity = Velocity.normalized * MaxSpeed;
		}

		//Apply the velocity.
		MyCharController.SimpleMove(Velocity * Time.fixedDeltaTime);
	}
}