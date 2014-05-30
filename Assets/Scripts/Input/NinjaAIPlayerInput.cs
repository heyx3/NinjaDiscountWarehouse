using UnityEngine;


/// <summary>
/// The player input for an AI-controlled ninja.
/// </summary>
public class NinjaAIPlayerInput : PlayerInput
{
	public override Vector2 MovementInput { get { return moveInput; } }
	private Vector2 moveInput = Vector2.zero;


	void Update()
	{
		//TODO: For now, just seek to the player.
	}
}