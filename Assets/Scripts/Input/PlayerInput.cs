using UnityEngine;


/// <summary>
/// Some kind of input for a character.
/// </summary>
public abstract class PlayerInput : MonoBehaviour
{
	/// <summary>
	/// Gets the horizontal movement input -- the X is the right-ward input, and the Y is the forward input.
	/// </summary>
	public abstract Vector2 MovementInput { get; }
}