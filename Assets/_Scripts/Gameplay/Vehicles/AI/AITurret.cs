using UnityEngine;

public class AITurret : MonoBehaviour
{
    [SerializeField] Teams.Team enemyTeam = Teams.Team.Allies;
    Health currentEnemy = null;
    Rigidbody2D enemyRB = null;
    GunsController gunsController;

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
        currentEnemy = teamsInstance.FindClosestToMe(enemyTeam, transform.position);
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

        transform.right = -(SimplePredict()).normalized;
        if (currentEnemy.HP <= 0) return;
        gunsController.TryShoot(dist);
    }

    Vector2 predictedPos = Vector2.zero;
    Vector3 SimplePredict()
    {
        Vector3 delta = currentEnemy.transform.position - transform.position;
        float projectileVelocity = gunsController.MaxVelocity;

        Vector2 tangentialMovement = Vector3.Project(enemyRB.velocity, Vector2.Perpendicular(delta));
        float tangentialLen = tangentialMovement.magnitude;

        float closingSpeed = Mathf.Sqrt(Mathf.Pow(projectileVelocity, 2) - Mathf.Pow(tangentialLen, 2));

        Vector2 orthogonalMovement = delta.normalized * closingSpeed;
        Vector2 leadVector = orthogonalMovement + tangentialMovement;

        float interCeptTime = delta.magnitude / orthogonalMovement.magnitude;
        Vector3 predictedMovement = Vector3.Project(interCeptTime * tangentialMovement, enemyRB.velocity);

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
