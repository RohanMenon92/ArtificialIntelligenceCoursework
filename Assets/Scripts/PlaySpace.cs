using MLAgents;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlaySpace : Area
{
    [Header("Pig Area Objects")]
    public GameObject enemyAgent;
    public GameObject ground;
    public Material successMaterial;
    public Material failureMaterial;
    public TextMeshPro scoreText;

    [Header("Prefabs")]
    public GameObject targetPrefab;
    public GameObject wallPrefab;

    [HideInInspector]
    public int numTargets;
    [HideInInspector]
    public int numWalls;
    [HideInInspector]
    public float spawnRange;

    private List<GameObject> spawnedTargets;
    private List<GameObject> spawnedWalls;

    // A list of (position, radius) tuples of occupied spots in the area
    private List<Tuple<Vector3, float>> occupiedPositions;


    private Renderer groundRenderer;
    private Material groundMaterial;

    private int notGroundLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        groundRenderer = ground.GetComponent<Renderer>();

        groundMaterial = groundRenderer.material;

        notGroundLayerMask = ~LayerMask.GetMask("ground");
    }

    #region RESET_REGION
    public override void ResetArea()
    {
        base.ResetArea();
        occupiedPositions = new List<Tuple<Vector3, float>>();
        ResetAgent();
        ResetTargets();
        ResetWalls();
    }

    /// <summary>
    /// Reset the agent
    /// </summary>
    private void ResetAgent()
    {
        // Reset location and rotation
        RandomlyPlaceObject(enemyAgent, spawnRange, 10);
    }

    /// <summary>
    /// Resets all truffles in the area
    /// </summary>
    private void ResetTargets()
    {
        if (spawnedTargets != null)
        {
            // Destroy any enemies remaining from the previous run
            foreach (GameObject spawnedEnemies in spawnedTargets.ToArray())
            {
                Destroy(spawnedEnemies);
            }
        }

        spawnedTargets = new List<GameObject>();

        for (int i = 0; i < numTargets; i++)
        {
            // Create a new truffle instance and place it randomly
            GameObject truffleInstance = Instantiate(targetPrefab, transform);
            RandomlyPlaceObject(truffleInstance, spawnRange, 50);
            spawnedTargets.Add(truffleInstance);
        }
    }

    /// <summary>
    /// Resets all stumps in the area
    /// </summary>
    private void ResetWalls()
    {
        if (spawnedWalls != null)
        {
            // Destroy any stumps remaining from the previous run
            foreach (GameObject spawnedWall in spawnedWalls.ToArray())
            {
                Destroy(spawnedWall);
            }
        }

        spawnedWalls = new List<GameObject>();

        for (int i = 0; i < numWalls; i++)
        {
            // Create a new stump instance and place it randomly
            GameObject wallInstance = Instantiate(wallPrefab, transform);
            RandomlyPlaceObject(wallInstance, spawnRange, 50);
            spawnedWalls.Add(wallInstance);
        }
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        
    }


    #region OBJECT_PLACEMENT
    private void RandomlyPlaceObject(GameObject objectToPlace, float range, float maxAttempts)
    {
        // Temporarily disable collision
        objectToPlace.GetComponent<Collider>().enabled = false;

        // Calculate test radius 10% larger than the collider extents
        float testRadius = GetColliderRadius(objectToPlace) * 1.1f;

        // Set a random rotation
        objectToPlace.transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f));

        // Make several attempts at randomly placing the object
        int attempt = 1;
        while (attempt <= maxAttempts)
        {
            Vector3 randomLocalPosition = new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range));
            randomLocalPosition.Scale(transform.localScale);

            //if (!Physics.CheckSphere(transform.position + randomLocalPosition, testRadius, notGroundLayerMask))
            if (CheckIfPositionIsOpen(transform.position + randomLocalPosition, testRadius))
            {
                objectToPlace.transform.localPosition = randomLocalPosition;
                occupiedPositions.Add(new Tuple<Vector3, float>(objectToPlace.transform.position, testRadius));
                break;
            }
            else if (attempt == maxAttempts)
            {
                Debug.LogError(string.Format("{0} couldn't be placed randomly after {1} attempts.", objectToPlace.name, maxAttempts));
                break;
            }

            attempt++;
        }

        // Enable collision
        objectToPlace.GetComponent<Collider>().enabled = true;
    }

    /// <summary>
    /// Gets a local space radius that draws a circle on the X-Z plane around the boundary of the collider
    /// </summary>
    /// <param name="obj">The game object to test</param>
    /// <returns>The local space radius around the collider</returns>
    private static float GetColliderRadius(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();

        Vector3 boundsSize = Vector3.zero;
        if (col.GetType() == typeof(MeshCollider))
        {
            boundsSize = ((MeshCollider)col).sharedMesh.bounds.size;
        }
        else if (col.GetType() == typeof(BoxCollider))
        {
            boundsSize = col.bounds.size;
        }

        boundsSize.Scale(obj.transform.localScale);
        return Mathf.Max(boundsSize.x, boundsSize.z) / 2f;
    }

    private bool CheckIfPositionIsOpen(Vector3 testPosition, float testRadius)
    {
        foreach (Tuple<Vector3, float> occupied in occupiedPositions)
        {
            Vector3 occupiedPosition = occupied.Item1;
            float occupiedRadius = occupied.Item2;
            if (Vector3.Distance(testPosition, occupiedPosition) - occupiedRadius <= testRadius)
            {
                return false;
            }
        }

        return true;
    }
    #endregion
}
