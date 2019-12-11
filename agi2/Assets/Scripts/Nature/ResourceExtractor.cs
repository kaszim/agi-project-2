using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceExtractor : MonoBehaviour
{
    [SerializeField]
    float fuelPerAmmoPack = 100;
    [SerializeField]
    float maxExtractionRadius = 1f;
    [SerializeField]
    float extractionRadiusGrowthRate = 0.002f;
    [SerializeField]
    AmmoPack ammoPackPrefab = null;
    TerrainTexture terrain = null;
    float lastMoveTime;
    float currentFuel = 0;

    Vector3 lastPosition;
    float pendingResetCounter = 0;

    // Delta position distance (in scaled world space) for detecting a move
    const float MoveDistanceDelta = 10f;
    // Time before resetting the radius after moving (used to increase robustness in case of errors in AR tracking)
    const float PendingMoveTime = 1;

    private AmmoPack currentAmmoPack; //The current ammo pack in scene

    // Start is called before the first frame update
    void Start()
    {
        terrain = Transform.FindObjectOfType<TerrainTexture>();
        lastMoveTime = Time.time;
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Check if within my territory
        if ((Divider.Instance.IsOnRedSide(transform.position) && NetworkedGameObject.Player == Player.Red) || (!Divider.Instance.IsOnRedSide(transform.position) && NetworkedGameObject.Player == Player.Blue))
                ConsumeResources();
        else {
            //TODO: Display some kind of red ring or something to indicate that the resource extractor cannot be placed here
        }
    }

    void ConsumeResources()
    {
        // Check if we have moved the extractor. If so, reset the radius.
        Vector3 currentPosition = transform.position;
        if (Vector3.SqrMagnitude((currentPosition - lastPosition) / terrain.WorldScale) > MoveDistanceDelta * MoveDistanceDelta)
        {
            pendingResetCounter += Time.deltaTime;
            if (pendingResetCounter > 1)
            {
                lastMoveTime = Time.time;
                lastPosition = currentPosition;
            }
        }
        else
        {
            pendingResetCounter = 0;
            // Consume resources
            if (currentAmmoPack == null) {
                float timeSinceMove = Time.time - lastMoveTime;
                float radius = Mathf.Min(maxExtractionRadius, extractionRadiusGrowthRate * timeSinceMove);
                currentFuel += terrain.ConsumeResourcesWorldSpace(lastPosition, radius * terrain.WorldScale, Time.deltaTime * 10);
            }
        }
        // Spend resources to create ammo packs
        if (currentFuel > fuelPerAmmoPack)
        {
            currentFuel -= fuelPerAmmoPack;
            print("Spawn ammo");
            CreateAmmoPack();
        }
    }

    void CreateAmmoPack()
    {
        if (ammoPackPrefab)
        {
            var spawnPosition = transform.position;
            var angle = Random.Range(0, Mathf.PI * 2);
            var distance = 10;
            spawnPosition += new Vector3(Mathf.Sin(angle) * distance, 0, Mathf.Cos(angle) * distance) * terrain.WorldScale;
            spawnPosition.y = terrain.transform.position.y;
            currentAmmoPack = GameObject.Instantiate(ammoPackPrefab, spawnPosition, Quaternion.identity);
            if (terrain.transform.parent != null)
            {
                var tempScale = currentAmmoPack.transform.localScale;
                currentAmmoPack.transform.SetParent(terrain.transform.parent);
                currentAmmoPack.transform.localScale = tempScale;
            }
        }
    }
}
