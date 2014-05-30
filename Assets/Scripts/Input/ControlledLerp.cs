using System;
using UnityEngine;


/// <summary>
/// Represents a lerp between two values, whose interpolant is controlled by some input axis.
/// If no input is applied, the value starts moving towards a default value.
/// </summary>
[System.Serializable]
public class ControlledLerp
{
	/// <summary>
	/// The second derivative of the current value.
	/// </summary>
	public float CurrentAccel;
	/// <summary>
	/// The first derivative of the current value.
	/// </summary>
	public float CurrentVelocity;
	/// <summary>
	/// The current value between the min and max values.
	/// </summary>
	public float CurrentValue;

	/// <summary>
	/// The bottom of the range for the lerp.
	/// </summary>
	public float MinValue;
	/// <summary>
	/// The top of the range for the lerp.
	/// </summary>
	public float MaxValue;
	/// <summary>
	/// The default value that this instance will seek to if no input is provided.
	/// </summary>
	public float DefaultValue;

	/// <summary>
	/// If the difference between the current value and the default value is less than this,
	///    the current value will be snapped to the default value.
	/// </summary>
	public float DefaultValueSnapDist;

	/// <summary>
	/// The strength of the acceleration from the input axis.
	/// </summary>
	public float InputAccelScale;
	/// <summary>
	/// The strength of the acceleration from seeking to the default value when there is no input.
	/// </summary>
	public float RestAccelScale;


	/// <summary>
	/// Pass NaN for "defaultVal" to set it to the midpoint of the min and max.
	/// </summary>
	public ControlledLerp(float inputAccel, float restAccel,
						  float min, float max, float defaultVal = Single.NaN)
	{
		if (Single.IsNaN(defaultVal))
			defaultVal = (min + max) * 0.5f;

		MinValue = min;
		MaxValue = max;
		DefaultValue = defaultVal;

		CurrentValue = DefaultValue;
		CurrentAccel = 0.0f;
		CurrentVelocity = 0.0f;

		InputAccelScale = inputAccel;
		RestAccelScale = restAccel;
	}

	/// <summary>
	/// Updates the value of this lerp given the delta time and the input value (centered at 0.0).
	/// </summary>
	public void Update(float elapsedTime, float input)
	{
		//Calculate acceleration.
		CurrentAccel = input * InputAccelScale;
		if (CurrentAccel == 0.0f)
		{
			CurrentAccel = RestAccelScale * (DefaultValue - CurrentValue);
		}
		
		//Calculate velocity.
		//If the input is outside the range, make sure the velocity isn't pushing away from the DefaultValue.
		CurrentVelocity += elapsedTime * CurrentAccel;
		if ((CurrentValue < MinValue || CurrentValue > MaxValue) &&
			Mathf.Sign(CurrentVelocity) != Mathf.Sign(DefaultValue - CurrentValue))
		{
			CurrentVelocity = 0.0f;
		}

		//Update the current value as long as the player is inside the range, or moving towards the inside.
		float newValue = CurrentValue + (elapsedTime * CurrentVelocity);
		if ((newValue >= MinValue && newValue <= MaxValue) ||
			Mathf.Sign(input) == Mathf.Sign(DefaultValue - CurrentValue))
		{
			CurrentValue = newValue;
		}

		//Snap to the default value if the current value is close enough.
		if (input == 0.0f && Mathf.Abs(CurrentValue - DefaultValue) < DefaultValueSnapDist)
		{
			CurrentValue = DefaultValue;
			CurrentVelocity = 0.0f;
		}
	}
}