using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    [SerializeField] float size = 2;
    public float distThreshold = 30;
    [SerializeField] Teams.Team targetTeam = Teams.Team.Enemies;
    Transform player;
    GameManager gameHandler;
    private void Start()
    {
        gameHandler = GameManager.Instance;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObj = gameHandler.Player;
            if (playerObj == null) return;
            player = playerObj.GetComponent<Hull>().hull.transform;
            return;
        } 
        Transform enemy = Teams.Instance.FindClosestToMe(targetTeam, player.position)?.transform;
        if (enemy == null) return;
        Vector3 delta = enemy.position - player.position;
        float dist = delta.magnitude;
        bool active = dist > distThreshold;
        transform.localScale = active ? Vector3.one * size : Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, delta.normalized));
    }
}
