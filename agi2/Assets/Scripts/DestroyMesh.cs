using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMesh : MonoBehaviour
{
    public GameObject destructableObject; // The mesh with all the sliced parts

    private void OnMouseDown()
    {
        Explode(transform.position);
    }

    public void Explode(Vector3 origin)
    {
        UnityClient.Instance.SendPacket(Packet.Explode, transform.name);
        GameObject newObj = Instantiate(destructableObject, this.transform.position, this.transform.rotation);
        newObj.transform.SetParent(this.transform.parent);
        newObj.transform.localScale = this.transform.localScale;
        Rigidbody[] objects = newObj.GetComponentsInChildren<Rigidbody>();
        if (objects.Length != 0)
        {
            foreach (Rigidbody current in objects)
            {
                current.AddExplosionForce(100, origin, 4);
            }
        }
        Destroy(this.gameObject);
    }
}
