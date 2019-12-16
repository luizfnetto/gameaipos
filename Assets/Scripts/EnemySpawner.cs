using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform target;
    public GameObject enemyPrefab;
    private List<Transform> spawnpoints = new List<Transform>();
    private int _spawnpointsCount;
    private bool _stop = false;

    private bool _timerDisparado = false;
    private float _timer = 0.0f;

    public float _timeUntilNextEnemy = 2f;
    private float _nextEnemyCounter;

    void Start()
    {
        _nextEnemyCounter = _timeUntilNextEnemy;
        target = PlayerManager.instance.player.transform;
        foreach (Transform child in transform)
        {
            spawnpoints.Add(child);
        }
        _spawnpointsCount = spawnpoints.Count;
        Debug.Log(_spawnpointsCount);
    }

    // Update is called once per frame
    void Update()
    {
        if (_timerDisparado && !_stop)
        {
            _nextEnemyCounter += Time.deltaTime;
            if (_nextEnemyCounter > _timeUntilNextEnemy)
            {
                _nextEnemyCounter = 0;
                Spawn();
            }

        }
    }


    public void StartSpawning()
    {
        // Se ja esta sendo processado, retorna
        if (_timerDisparado)
            return;

        // Delay de 3 segundos antes de comecar o hacking
        _timer += Time.deltaTime;
        if (_timer > 3)
        {
            _timer = 0;
            _timerDisparado = true;
            Debug.Log("Start Spawn");
        }
    }

    public void StopSpawning()
    {
        Debug.Log("Stop Spawn");
        _stop = true;
    }

    private void Spawn()
    {
        Debug.Log("Spawn");

        int randomPoint = Random.Range(0, _spawnpointsCount - 1);
        Debug.Log(randomPoint);
        Vector3 direction = (target.position - spawnpoints[randomPoint].position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(-direction.x, 0, -direction.z));
        Instantiate(enemyPrefab, spawnpoints[randomPoint].position, lookRotation, transform);
    }

}
