using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainConsumer : MonoBehaviour
{
    TerrainTexture terrain;
    [SerializeField] private float terrainRemovalRadius = 1;

    // Start is called before the first frame update
    void Start()
    {
        terrain = Transform.FindObjectOfType<TerrainTexture>();
    }

    // Update is called once per frame
    void Update()
    {
        // Consume terrain around the tank
        if (terrain)
        {
            terrain.ConsumeResourcesWorldSpace(transform.position, terrainRemovalRadius * terrain.WorldScale, Time.deltaTime * 10);
        }
    }
}
