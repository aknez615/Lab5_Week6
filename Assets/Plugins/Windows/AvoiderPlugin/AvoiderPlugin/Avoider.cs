using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class Avoider : MonoBehaviour
{
    public GameObject avoidTarget;
    public float avoidRange = 10f;
    public float speed = 3.5f;
    public bool showGizmos = true;

    private NavMeshAgent agent;
    private Vector3 lastDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (avoidTarget == null)
        {
            Debug.Log("Avoider: No target assigned to avoid");
        }
    }

    void Update()
    {
        if (avoidTarget == null) return;

        transform.LookAt(avoidTarget.transform);

        float distance = Vector3.Distance(transform.position, avoidTarget.transform.position);

        if (distance < avoidRange)
        {
            Vector3? escapePoint = GetEscapePoint();

            if (escapePoint.HasValue && escapePoint.Value != lastDestination)
            {
                lastDestination = escapePoint.Value;
                agent.speed = speed;
                agent.SetDestination(lastDestination);
            }
        }
    }

    Vector3? GetEscapePoint()
    {
        float sampleSize = 20f;
        float cellSize = 1f;

        var sampler = new PoissonDiscSampler(sampleSize, sampleSize, cellSize);
        List<Vector3> candidates = new List<Vector3>();

        foreach (Vector2 point in sampler.Samples())
        {
            Vector3 worldPoint = transform.position + new Vector3(point.x - sampleSize / 2f, 0, point.y - sampleSize / 2f);

            if (!IsVisibleToAvoidTarget(worldPoint))
            {
                candidates.Add(worldPoint);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates.OrderBy(p => Vector3.Distance(transform.position, p)).First();
    }

    bool IsVisibleToAvoidTarget(Vector3 point)
    {
        Vector3 direction = point - avoidTarget.transform.position;
        Ray ray = new Ray(avoidTarget.transform.position, direction.normalized);

        if (Physics.Raycast(avoidTarget.transform.position, direction.normalized, out RaycastHit hit, direction.magnitude))
        {
            return hit.collider.gameObject == gameObject;
        }

        return true;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || avoidTarget == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, avoidTarget.transform.position);
        Gizmos.DrawWireSphere(transform.position, avoidRange);
    }
}