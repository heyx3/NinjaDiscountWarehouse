using UnityEngine;


/// <summary>
/// The controller for the tutorial steps.
/// </summary>
public class TutorialController : MonoBehaviour
{
	public enum States
	{
		TurnTowardsBarrel,
		LookAboveBarrel,
		NodAtBarrel,
		ThrowBarrel,
	}
	public enum Side
	{
		Left,
		Right,
	}

	public States CurrentState { get; private set; }
	public Side CurrentSide { get; private set; }

	public int SidesCompleted { get; private set; }


	public Transform LeftObjectsContainer, RightObjectsContainer;
	public HumanBehavior PlayerBehavior;

	public Transform container, activeChild,
					  turnTowardsText, lookAboveText, nodText, throwText;


	private void ChangeState(States newState)
	{
		switch (newState)
		{
			case States.TurnTowardsBarrel:
				turnTowardsText.gameObject.SetActive(true);
				lookAboveText.gameObject.SetActive(false);
				nodText.gameObject.SetActive(false);
				throwText.gameObject.SetActive(false);
				activeChild = turnTowardsText;
				break;

			case States.LookAboveBarrel:
				turnTowardsText.gameObject.SetActive(false);
				lookAboveText.gameObject.SetActive(true);
				nodText.gameObject.SetActive(false);
				throwText.gameObject.SetActive(false);
				activeChild = lookAboveText;
				break;

			case States.NodAtBarrel:
				turnTowardsText.gameObject.SetActive(false);
				lookAboveText.gameObject.SetActive(false);
				nodText.gameObject.SetActive(true);
				throwText.gameObject.SetActive(false);
				activeChild = nodText;
				break;

			case States.ThrowBarrel:
				turnTowardsText.gameObject.SetActive(false);
				lookAboveText.gameObject.SetActive(false);
				nodText.gameObject.SetActive(false);
				throwText.gameObject.SetActive(true);
				activeChild = throwText;
				break;

			default: throw new System.NotImplementedException();
		}

		CurrentState = newState;
	}
	private void ChangeSide(Side newSide)
	{
		switch (newSide)
		{
			case Side.Left:
				LeftObjectsContainer.gameObject.SetActive(true);
				RightObjectsContainer.gameObject.SetActive(false);
				container = LeftObjectsContainer;
				break;

			case Side.Right:
				LeftObjectsContainer.gameObject.SetActive(false);
				RightObjectsContainer.gameObject.SetActive(true);
				container = RightObjectsContainer;
				break;

			default: throw new System.NotImplementedException();
		}

		turnTowardsText = container.FindChild("Turn Towards");
		lookAboveText = container.FindChild("Look Above");
		nodText = container.FindChild("Nod");
		throwText = container.FindChild("Throw");

		ChangeState(States.TurnTowardsBarrel);
	}


	void Awake()
	{
		ChangeSide(Side.Left);

		SidesCompleted = 0;
	}


	void Update()
	{
		Vector3 lookDir = PlayerBehavior.FaceTracker.ForwardLogs[PlayerBehavior.FaceTracker.GetLogIndex(0)].normalized;
		Vector3 pos = PlayerBehavior.MyTransform.position;


		switch (CurrentState)
		{
			case States.TurnTowardsBarrel:

				Vector3 towardsBarrel = lookAboveText.position - pos;

				Vector2 towardsBarrelHorz = new Vector2(towardsBarrel.x, towardsBarrel.z);
				Vector2 lookDirHorz = new Vector2(lookDir.x, lookDir.z);

				if (Mathf.Abs(Vector2.Dot(towardsBarrelHorz.normalized, lookDirHorz.normalized)) > 0.9f)
					ChangeState(States.LookAboveBarrel);

				break;

			case States.LookAboveBarrel:

				Vector3 toBarrel = lookAboveText.position - pos;

				if (Mathf.Abs(Vector3.Dot(toBarrel.normalized, lookDir)) > 0.9f)
					ChangeState(States.NodAtBarrel);

				break;

			case States.NodAtBarrel:

				if (PlayerBehavior.IsLevitating)
					ChangeState(States.ThrowBarrel);

				break;

			case States.ThrowBarrel:

				if (!PlayerBehavior.IsLevitating)
				{
					SidesCompleted += 1;

					if (SidesCompleted == 2)
					{
						FadeToGameScene fader = FindObjectOfType<FadeToGameScene>();
						fader.StartFade(0.25f);
						fader.OnFadedOut += (s, e) => { Application.LoadLevel("GameScene"); };
						fader.OnFadedIn += (s, e) => { Destroy(FindObjectOfType<FadeToGameScene>().gameObject); };
						gameObject.SetActive(false);
					}
					else ChangeSide(Side.Right);
				}

				break;

			default: throw new System.NotImplementedException();
		}
	}
}