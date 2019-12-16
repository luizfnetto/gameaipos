﻿
using UnityEngine;

public class ShootScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera camera;
    public ParticleSystem particles;
    public GameObject impactEffect;
    public RoomStateMachine stateMachine;

    // Update is called once per frame
    void Update()
    {
        if (stateMachine.GetCurrentState() != RoomStateMachine.StateId.GameOver) {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
                //stateMachine.SetState(RoomStateMachine.StateId.GameOver); TODO
            }
        }
    }

    void Shoot()
    {
        particles.Play();

        RaycastHit hit;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            if(hit.transform.name != "Player")
            {
                GameObject impactGo = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGo, 2f);
            }
            
        }

    }
    
}
