using UnityEngine;
using System.Linq;


/// <summary>
/// Tracks the position/velocity/acceleration of this component's GameObject by looking at the delta position.
/// Holds on to the most recent position and velocity readings ("logs") in buffers.
/// NOTE: The logs are NOT stored in chronological order; the tracker simply wraps around the end of the buffer when it runs out of spaces.
/// </summary>
public class KinematicsTracker : MonoBehaviour
{
	public const int LogBufferSize = 300;

	/// <summary>
	/// Performs the "%" operator for "n" as expected for negative numbers, unlike the actual behavior of the "%" operator.
	/// </summary>
	private static int WrapNegative(int n)
	{
		while (n < 0) n += LogBufferSize;
		return n;
	}
	/// <summary>
	/// Gets the absolute value of each of the given vector's components.
	/// </summary>
	private static Vector3 Abs(Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}


	public enum KinematicTypes
	{
		Translation,
		Rotation,
	}
	public KinematicTypes TypeToTrack = KinematicTypes.Translation;


	[System.Serializable]
	public class DebugData
	{
		public Vector2 GUIPosOffset = Vector2.zero;
		public bool LogAverageVel = false;
		public bool LogAverageAccel = false;
		public float Duration = 1.0f;
	}
	public DebugData Debugging = new DebugData();


	/// <summary>
	/// The next spot in the log buffer to be written to.
	/// </summary>
	public int LogCounter { get; private set; }
	/// <summary>
	/// The recorded delta time for each log entry.
	/// </summary>
	public float[] DeltaTimeLogs { get; private set; }
	/// <summary>
	/// The recorded forward vectors for each log entry.
	/// </summary>
	public Vector3[] ForwardLogs { get; private set; }
	/// <summary>
	/// The recorded right vectors for each log entry.
	/// </summary>
	public Vector3[] RightLogs { get; private set; }
	/// <summary>
	/// The most recently-recorded positions. NOT in simple chronological order.
	/// </summary>
	public Vector3[] PositionLogs { get; private set; }
	/// <summary>
	/// The most recently-recorded velocities. NOT in simple chronological order.
	/// </summary>
	public Vector3[] VelocityLogs { get; private set; }
	/// <summary>
	/// The most recently-recorded accelerations. NOT in simple chronological order.
	/// </summary>
	public Vector3[] AccelerationLogs { get; private set; }
	/// <summary>
	/// The sum total amount of time represented by all the log entries this tracker currently has.
	/// </summary>
	public float MaxLogDuration { get; private set; }

	public Transform MyTransform { get; private set; }


	/// <summary>
	/// Given the log entry to get (0-based, from most recent entry to oldest entry),
	/// gets the index in the log arrays representing that entry.
	/// </summary>
	public int GetLogIndex(int descendingChronological)
	{
		if (descendingChronological >= LogBufferSize)
			throw new System.IndexOutOfRangeException("Argument must be less than " + LogBufferSize.ToString() + ", but it was " + descendingChronological.ToString() + "!");

		//The most recent entry is before the current counter value.
		return WrapNegative(LogCounter - 1 - descendingChronological) % LogBufferSize;
	}
	/// <summary>
	/// Clamps the given time duration between 0 and the length of the log buffer's sampling so far (i.e. the total interval of time that the logs represent).
	/// </summary>
	public float ClampDuration(float duration)
	{
		return Mathf.Clamp(duration, 0.0f, MaxLogDuration);
	}
	/// <summary>
	/// Gets the number of log entries needed to cover the given time duration.
	/// "duration" does not need to be clamped with "ClampDuration()".
	/// </summary>
	public int GetNumbLogs(float duration)
	{
		int start = GetLogIndex(0);
		float counter = 0.0f;
		int i;
		for (i = 0; i < LogBufferSize; ++i)
		{
			if (counter > duration || DeltaTimeLogs[WrapNegative(start - i)] == -1.0f)
				break;

			int logIndex = WrapNegative(start - i);
			counter += DeltaTimeLogs[logIndex];
		}

		return i;
	}

	/// <summary>
	/// Gets the average delta time value over the given number of seconds.
	/// </summary>
	public float GetAverageDeltaTime(float duration)
	{
		int start = GetLogIndex(0);

		if (DeltaTimeLogs[start] == -1.0f) return 0.00001f;

		//Get the number of logs for this duration.
		duration = ClampDuration(duration);
		int logs = GetNumbLogs(duration);
		if (logs == 0) return DeltaTimeLogs[start];

		//Calculate the average.
		float sum = 0.0f;
		for (int logCount = 0; logCount < logs; ++logCount)
			sum += DeltaTimeLogs[WrapNegative(start - logCount)];
		return sum / (float)logs;
	}

	public struct RotationValue { public Vector3 Forward, Right; }
	/// <summary>
	/// Gets the forward vector of this object at the specified number of seconds in the past.
	/// </summary>
	public RotationValue GetForwardVectorAtTime(float timeAgo)
	{
		timeAgo = ClampDuration(timeAgo);
		RotationValue val = new RotationValue();

		float counter = 0.0f;
		for (int i = 0; i < LogBufferSize; ++i)
		{
			if (counter >= timeAgo)
			{
				int index = GetLogIndex(i);
				val.Forward = ForwardLogs[index];
				val.Right = RightLogs[index];
				return val;
			}
			counter += DeltaTimeLogs[GetLogIndex(i)];
		}

		val.Forward = ForwardLogs[GetLogIndex(LogBufferSize - 1)];
		val.Right = RightLogs[GetLogIndex(LogBufferSize - 1)];
		return val;
	}

	/// <summary>
	/// Gets the average of the given buffer over the given number of seconds.
	/// </summary>
	public Vector3 GetAverage(float duration, Vector3[] logBuffer)
	{
		//Get the number of logs for this duration.
		duration = ClampDuration(duration);
		int logs = GetNumbLogs(duration);
		if (logs == 0) return logBuffer[GetLogIndex(0)];

		int start = GetLogIndex(0);

		//Calculate the average.
		Vector3 sum = Vector3.zero;
		for (int logCount = 0; logCount < logs; ++logCount)
			sum += logBuffer[WrapNegative(start - logCount)];
		return sum / (float)logs;
	}
	public Vector3 GetAverageVelocity(float duration) { return GetAverage(duration, VelocityLogs); }
	public Vector3 GetAverageAcceleration(float duration) { return GetAverage(duration, AccelerationLogs); }

	/// <summary>
	/// Gets the net change in position during the given period.
	/// </summary>
	public Vector3 GetDeltaPos(float duration)
	{
		duration = ClampDuration(duration);
		int logs = GetNumbLogs(duration);
		if (logs == 0) return Vector3.zero;

		return PositionLogs[GetLogIndex(0)] - PositionLogs[GetLogIndex(logs - 1)];
	}
	/// <summary>
	/// Gets the total amount traveled (ignoring direction/sign) over the given period.
	/// </summary>
	public Vector3 GetAmountTraveled(float duration)
	{
		//Get the number of logs to count.
		duration = ClampDuration(duration);
		int logs = GetNumbLogs(duration);
		if (logs == 0) return Vector3.zero;

		//Integrate with straight-forward Euler integration.
		//This should be basically as accurate as checking the delta position (within floating-point error).
		Vector3 total = Vector3.zero;
		for (int i = 0; i < logs; ++i)
		{
			int index = GetLogIndex(i);
			total += Abs(VelocityLogs[index]) * DeltaTimeLogs[index];
		}
		return total;
	}


	void Awake()
	{
		MyTransform = transform;

		LogCounter = 0;

		ForwardLogs = new Vector3[LogBufferSize];
		RightLogs = new Vector3[LogBufferSize];
		PositionLogs = new Vector3[LogBufferSize];
		VelocityLogs = new Vector3[LogBufferSize];
		AccelerationLogs = new Vector3[LogBufferSize];
		DeltaTimeLogs = new float[LogBufferSize];
		MaxLogDuration = 0.0f;

		for (int i = 0; i < LogBufferSize; ++i)
		{
			ForwardLogs[i] = Vector3.forward;
			RightLogs[i] = Vector3.right;
			PositionLogs[i] = Vector3.zero;
			VelocityLogs[i] = Vector3.zero;
			AccelerationLogs[i] = Vector3.zero;
			DeltaTimeLogs[i] = -1.0f;
		}
	}

	void FixedUpdate()
	{
		int previous = WrapNegative(LogCounter - 1);

		//Update the amount of time this tracker has logged right now.
		if (DeltaTimeLogs[LogCounter] > 0.0f)
			MaxLogDuration -= DeltaTimeLogs[LogCounter];
		MaxLogDuration += Time.deltaTime;

		DeltaTimeLogs[LogCounter] = Time.deltaTime;
		ForwardLogs[LogCounter] = MyTransform.forward;
		RightLogs[LogCounter] = MyTransform.right;

		switch (TypeToTrack)
		{
			case KinematicTypes.Translation:
				PositionLogs[LogCounter] = MyTransform.position;
				break;

			case KinematicTypes.Rotation:
				PositionLogs[LogCounter] = MyTransform.eulerAngles;
				break;

			default: throw new System.NotImplementedException();
		}

		VelocityLogs[LogCounter] = (PositionLogs[LogCounter] - PositionLogs[previous]) / Time.deltaTime;
		AccelerationLogs[LogCounter] = (VelocityLogs[LogCounter] - VelocityLogs[previous]) / Time.deltaTime;

		LogCounter = (LogCounter + 1) % LogBufferSize;
	}

	void OnGUI()
	{
		System.Text.StringBuilder strng = new System.Text.StringBuilder();

		if (Debugging.LogAverageVel)
		{
			strng.Append("Velocity: ");
			strng.Append(GetAverageVelocity(Debugging.Duration).ToString());
			strng.AppendLine();
		}
		if (Debugging.LogAverageAccel)
		{
			strng.Append("Acceleration: ");
			strng.Append(GetAverageAcceleration(Debugging.Duration).ToString());
			strng.AppendLine();
		}

		GUI.Label(new Rect(Debugging.GUIPosOffset.x, Debugging.GUIPosOffset.y, Screen.width - Debugging.GUIPosOffset.x, Screen.height - Debugging.GUIPosOffset.y),
				  strng.ToString());
	}
}