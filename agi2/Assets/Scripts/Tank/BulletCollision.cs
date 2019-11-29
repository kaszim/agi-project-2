using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class BulletCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other) {
        switch (other.tag) {
            case "Destructable":
                other.GetComponent<DestroyMesh> ().Explode(transform.position);
                break;
            case "Player":
                if(other.GetComponent<NetworkedGameObject> ().Owner)
                    return;
                other.GetComponent<tankMovement> ().TakeDamage();
                break;
            default:
                break;
        }
        Destroy(gameObject);
    }
}
