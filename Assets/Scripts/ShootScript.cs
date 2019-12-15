
using UnityEngine;

public class ShootScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera camera;
    public ParticleSystem particles;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
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
        }

    }
    
}
