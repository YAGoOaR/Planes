using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITurret : MonoBehaviour
{
    [SerializeField] Teams.Team enemyTeam = Teams.Team.Allies;
    GameObject currentEnemy = null;
    GunsController gunsController;
    Rigidbody2D rb;

    float forgetDistanceThreshold;
    float forgetBias = 50;

    public enum AITurretState
    {
        idle,
        attacking,
    }

    Teams teamsInstance;
    AITurretState state = AITurretState.idle;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        teamsInstance = Teams.Instance;
        gunsController = GetComponent<GunsController>();
        forgetDistanceThreshold = gunsController.MaxShootDistance + forgetBias;
    }

    void Update()
    {
        switch (state)
        {
            case AITurretState.idle:
                FindSomethingToDo();
                break;
            case AITurretState.attacking:
                Attack();
                break;
            default:
                break;
        }
    }

    void FindSomethingToDo()
    {
        currentEnemy = teamsInstance.FindClosestToMe(enemyTeam, transform.position)?.gameObject;
        if (currentEnemy != null)
        {
            float dist = (currentEnemy.transform.position - transform.position).magnitude;
            if (gunsController.CanShoot(dist)) state = AITurretState.attacking;
        }
    }

    void Attack()
    {
        if (currentEnemy == null) { state = AITurretState.idle; return; }
        Vector3 delta = currentEnemy.transform.position - transform.position;
        float dist = delta.magnitude;
        if (dist > forgetDistanceThreshold)
        {
            state = AITurretState.idle;
            return;
        }
        gunsController.TryShoot(dist);
        transform.right = -(SimplePredict()).normalized;
    }

    Vector2 predictedPos = Vector2.zero;
    Vector3 SimplePredict()
    {
        Vector3 delta = currentEnemy.transform.position - transform.position;
        Rigidbody2D enemyRB = currentEnemy.GetComponent<Rigidbody2D>();
        float vel = gunsController.MaxVelocity;

        Vector2 ort = Vector3.Project(enemyRB.velocity, Vector2.Perpendicular(delta));
        float ortLen = ort.magnitude;
        Vector2 tang = delta.normalized * (Mathf.Sqrt(Mathf.Pow(vel, 2) - Mathf.Pow(ortLen, 2)));
        Vector2 leadVec = tang + ort;

        predictedPos = currentEnemy.transform.position + Vector3.Project((delta.magnitude / tang.magnitude) * ort, enemyRB.velocity);

        if (!(leadVec.magnitude > 0)) return delta;
        return leadVec;
    }

    private void OnDrawGizmos()
    {
        if (predictedPos == Vector2.zero) return;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(predictedPos, 1);
    }
}
