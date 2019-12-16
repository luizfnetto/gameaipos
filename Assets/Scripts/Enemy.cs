
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 50f;

    RoomStateMachine stateMachine;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if(health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        
        Destroy(gameObject);
    }

    void Start()
    {
        stateMachine = GameObject.Find("StateMachineHolder").GetComponent<RoomStateMachine>();
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Player")
        {
            stateMachine.SetState(RoomStateMachine.StateId.GameOver);
        }
    }

    //stateMachine.SetState(RoomStateMachine.StateId.GameOver); TODO on contact with enemy
}
