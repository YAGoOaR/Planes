using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITurret : MonoBehaviour
{
    [SerializeField] Teams.Team enemyTeam = Teams.Team.Allies;
    GameObject currentEnemy = null;
    Rigidbody2D enemyRB = null;
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
        enemyRB = currentEnemy?.GetComponent<Rigidbody2D>();

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
        float projectileVelocity = gunsController.MaxVelocity;

        Vector2 orthogonalPart = Vector3.Project(enemyRB.velocity, Vector2.Perpendicular(delta));
        float orthogonalLen = orthogonalPart.magnitude;

        float closingSpeed = Mathf.Sqrt(Mathf.Pow(projectileVelocity, 2) - Mathf.Pow(orthogonalLen, 2));

        Vector2 tangentialPart = delta.normalized * closingSpeed;
        Vector2 leadVector = tangentialPart + orthogonalPart;

        float interCeptTime = delta.magnitude / tangentialPart.magnitude;
        Vector3 predictedMovement = Vector3.Project(interCeptTime * orthogonalPart, enemyRB.velocity);

        predictedPos = currentEnemy.transform.position + predictedMovement;

        if (!(leadVector.magnitude > 0)) return delta;
        return leadVector;
    }

    private void OnDrawGizmos()
    {
        if (predictedPos == Vector2.zero) return;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(predictedPos, 1);
    }
}
