using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Divider : MonoBehaviour
{
    private static Divider instance;
    public static Divider Instance { get { return instance; } }

    // Start is called before the first frame update
    void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }

        // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsOnRedSide(Vector3 position) {
        return Vector3.Dot(position, transform.forward) > 0;
    }

    public bool AmIRedPlayer() {
        return true; //TODO check if I actually am red player or not
    }
}
