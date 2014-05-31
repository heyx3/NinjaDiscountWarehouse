public class KillAfterTime : UnityEngine.MonoBehaviour
{
    public static System.Collections.IEnumerator WaitThenKill(UnityEngine.GameObject toKill, float time)
    {
        yield return new UnityEngine.WaitForSeconds(time);
        UnityEngine.GameObject.Destroy(toKill);
    }

    public float TimeTillDeath = 5.0f;
    void Start()
    {
        StartCoroutine(WaitThenKill(gameObject, TimeTillDeath));
    }
}