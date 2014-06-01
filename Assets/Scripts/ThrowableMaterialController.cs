using UnityEngine;


public class ThrowableMaterialController : MonoBehaviour
{
	public static ThrowableMaterialController Instance { get; private set; }


	public Color Color1 = new Color(1.0f, 1.0f, 1.0f, 0.25f),
				 Color2 = new Color(0.0f, 0.0f, 0.0f, 0.01f);
	public float TimeSpeedScale = 1.0f;

	public Material ThrowableMat = null;


	void Awake()
	{
		if (Instance != null)
			throw new UnityException("More than one 'ThrowableMaterialController' is in play!");
		Instance = this;

		if (ThrowableMat != null)
		{
			//Modify the material so a copy is created.
			Color old = ThrowableMat.color;
			ThrowableMat.color = Color.black;
			ThrowableMat.color = Color.white;
			ThrowableMat.color = old;
		}
	}

	void Update()
	{
		if (ThrowableMat != null)
		{
			float oscillate_0_1 = 0.5f + (0.5f * Mathf.Sin(Time.time * TimeSpeedScale));
			oscillate_0_1 = Mathf.SmoothStep(0.0f, 1.0f, oscillate_0_1);
			ThrowableMat.color = new Color(Mathf.Lerp(Color1.r, Color2.r, oscillate_0_1),
										   Mathf.Lerp(Color1.g, Color2.g, oscillate_0_1),
										   Mathf.Lerp(Color1.b, Color2.b, oscillate_0_1),
										   Mathf.Lerp(Color1.a, Color2.a, oscillate_0_1));
		}
	}
}