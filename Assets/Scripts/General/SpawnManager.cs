using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    
    [SerializeField] GameObject[] spawnPoints;

    private List<int> usedSpawnPoints;

    private void Awake() => Instance = this;
    

    public Transform GetSpawnPoint()
    {
        int spawnPointIndex = 0;
        if (usedSpawnPoints == null)
        {
            usedSpawnPoints = new List<int>();
        }
        else
        {
            while (usedSpawnPoints.Contains(spawnPointIndex))
            {
                spawnPointIndex = Random.Range(0, spawnPoints.Length);
            }
        }
        
        usedSpawnPoints.Add(spawnPointIndex);
        return spawnPoints[spawnPointIndex].transform;
    }
}
