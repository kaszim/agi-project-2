using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class BulletCollision : MonoBehaviour
{
    public Vector3 Direction { get; set; }
    public float Speed { get; set; }
    public float XLossyScale { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.Play("tankShoot");
        GetComponent<Rigidbody>().velocity = Direction * Speed * XLossyScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other) {
        if (!GetComponent<NetworkedGameObject>().IsOwned)
            return;
        switch (other.tag) {
            case "Destructable":
                other.GetComponent<DestroyMesh> ().Explode(transform.position);
                AudioManager.Instance.Play("impact");
                break;
            case "Player":
                if (other.GetComponent<NetworkedGameObject>().IsOwned)
                    return;
                other.GetComponent<TankMovement> ().TakeDamage();
                UnityClient.Instance.SendPacket(Packet.HitTank, other.GetComponent<NetworkedGameObject>().UID);
                AudioManager.Instance.Play("impact");
                break;
            default:
                break;
        }
        Destroy(gameObject);
    }
}
