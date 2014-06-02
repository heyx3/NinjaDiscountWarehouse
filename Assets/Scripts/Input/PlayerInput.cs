using UnityEngine;


/// <summary>
/// Some kind of input for a character.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public abstract class PlayerInput : MonoBehaviour
{
	/// <summary>
	/// Gets the horizontal movement input -- the X is the right-ward input, and the Y is the forward input.
	/// </summary>
	public abstract Vector2 MovementInput { get; }

	public CharacterController CharContr { get; private set; }
	public Transform MyTransform { get; set; }


	protected virtual void Awake()
	{
		CharContr = GetComponent<CharacterController>();
		MyTransform = transform;
	}
}