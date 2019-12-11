using UnityEngine;
using System.Collections;
using Networking;

public class TankColor : MonoBehaviour
{
    public Material Red;
    public Material Blue;
    // Use this for initialization
    void Start()
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        var isOwned = GetComponent<NetworkedGameObject>().IsOwned;
        Material mat = null;
        Player self = NetworkedGameObject.Player;
        Player other = NetworkedGameObject.Player == Player.Red ? Player.Blue : Player.Red;
        if (isOwned)
        {
            mat = PlayerToMaterial(self);
        }
        else
        {
            mat = PlayerToMaterial(other);
        }
        foreach (var renderer in renderers)
        {
            renderer.material = mat;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Material PlayerToMaterial(Player ply)
    {
        return ply == Player.Red ? Red : Blue;
    }
}
