using UnityEngine;


/// <summary>
/// Fades the screen to black, then loads the scene 'GameScene'.
/// </summary>
public class FadeToGameScene : MonoBehaviour
{
	public float FadeSpeed { get; set; }
	public float CurrentFadeValue { get; set; }
	
	/// <summary>
	/// Raised when this object is done fading the screen out and just started fading back in.
	/// </summary>
	public event System.EventHandler OnFadedOut;
	/// <summary>
	/// Raised when this object is done fading the screen in and is about to stop animating.
	/// </summary>
	public event System.EventHandler OnFadedIn;


	private Renderer rndr;


	void Awake()
	{
		rndr = renderer;
		FadeSpeed = 0.0f;
		CurrentFadeValue = 0.0001f;

		DontDestroyOnLoad(gameObject);
	}
	void Update()
	{
		//Update the fade value.
		CurrentFadeValue += FadeSpeed * Time.deltaTime;

		if (CurrentFadeValue >= 1.0f)
		{
			CurrentFadeValue = 0.9999f;
			FadeSpeed = -FadeSpeed;
			if (OnFadedOut != null) OnFadedOut(this, new System.EventArgs());
		}
		if (CurrentFadeValue <= 0.0f)
		{
			CurrentFadeValue = 0.0001f;
			FadeSpeed = 0.0f;
			if (OnFadedIn != null) OnFadedIn(this, new System.EventArgs());
		}

		// Modify the renderer's material's alpha based on the fade value.
		Color col = rndr.material.color;
		col.a = CurrentFadeValue;
		rndr.material.color = col;
	}

	public void StartFade(float speed)
	{
		FadeSpeed = speed;
		CurrentFadeValue = 0.0001f;
	}
}