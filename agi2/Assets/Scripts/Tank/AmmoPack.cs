using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class AmmoPack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && GetComponent<NetworkedGameObject>().IsOwned)
        {
            other.GetComponent <tankMovement>().Reload();
            Destroy(gameObject);
        }
    }
}
