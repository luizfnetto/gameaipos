using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour
{
    public float lookRadius = 10f;
    Transform target;
    NavMeshAgent agent;
    public float stoppingDistance = 2f;
    public float MoveSpeed = 4f;

    // Start is called before the first frame update
    void Start()
    {
        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);
        
        if (distance <= lookRadius && distance > stoppingDistance)
        {
            // walk towards player

            FaceTarget();
            transform.position -= transform.forward * MoveSpeed * Time.deltaTime;
        }
        if (distance <= stoppingDistance)
        {
            //face the target
            FaceTarget();
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    float distance = Vector3.Distance(target.position, transform.position);

    //    if(distance <= lookRadius)
    //    {
    //        agent.SetDestination(target.position);

    //        if(distance <= agent.stoppingDistance)
    //        {
    //            //face the target
    //            FaceTarget();
    //        }

    //    }
    //}

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(-direction.x, 0, -direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
