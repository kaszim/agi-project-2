using Networking;
using UnityEngine;

public class NetworkedBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        if (GetComponent<NetworkedGameObject>() == null)
        {
            Debug.LogError("NetworkedBehaviour attached to non NetworkedGameobject GameObject.");
        }
    }
}
