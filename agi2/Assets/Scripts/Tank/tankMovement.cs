using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;

public class tankMovement : MonoBehaviour {
    //Tank movement related
    [SerializeField] private float movementSpeed = 5;
    [SerializeField] private float turnSpeed = 1;
    [SerializeField] private float epsilon;

    //Tank related

    //Bullet related
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Vector3 bulletOrigin;

    //ammo
    private int ammo;
    [SerializeField] private int ammoLimit;

    //Track related
    private GameObject tankTracks;
    private Renderer tankRenderer;


    NetworkedGameObject networkedGameObject;
    new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        string path = this.name;
        Debug.Log(path + "/Tracks/Track");
        tankTracks = GameObject.Find(this.name + "/Tracks/Track"); // Hierarcy reference to the tracks.
        tankRenderer = tankTracks.GetComponent<Renderer>();
        networkedGameObject = GetComponent<NetworkedGameObject>();
        rigidbody = GetComponent<Rigidbody>();
        TankInput.Instance.ShootButton.OnShootClick.AddListener(Shoot);
    }

    // Update is called once per frame
    void Update()
    {
        // If the object is networked and we are not the owner
        if (networkedGameObject != null && networkedGameObject.enabled && !networkedGameObject.Owner)
        {
            // We are not the owner of this object, therefore disable movement script
            this.enabled = false;
            TankInput.Instance.ShootButton.OnShootClick.RemoveListener(Shoot);
        }

        //Tankmovement
        Vector2 input = TankInput.Instance.Joystick.InputPositionPolar;
        AudioManager.Instance.TankSound(new Vector2(input.x, input.x)); //Update tank engine sound
        float rotateSign = Mathf.Sign(input.y);
        float rotationSpeed = (1 - Mathf.Abs((input.y) / Mathf.PI)) * 2000 * turnSpeed;
        float rotationDelta = rotateSign * input.x * rotationSpeed;
        if (Mathf.Abs(rotationDelta) > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + rotationDelta * Time.deltaTime, 0);
        }
        float movementDelta = input.x * movementSpeed * transform.lossyScale.x;
        if (Mathf.Abs(movementDelta) > 0.01f)
        {
            transform.position += transform.forward * movementDelta * Time.deltaTime;
        }        
        TankInput.Instance.Joystick.UpdateTankRotationIndicator(transform.rotation.eulerAngles.y);
        //Animate the tankTracks by moving the uvs
        var offset = (tankRenderer.material.mainTextureOffset.x + input.x * Time.deltaTime * movementSpeed * 0.05f) % 1f;
        tankRenderer.material.mainTextureOffset = new Vector2(offset, tankRenderer.material.mainTextureOffset.y);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R))    //debug reload
        {
            Reload();
        }
    }

    void Shoot()
    {
        if(ammo > 0){
            GameObject activeBullet = Instantiate(bullet, transform.TransformPoint(bulletOrigin), Quaternion.identity);
            if (transform.parent != null)
            {
                Vector3 tempScale = activeBullet.transform.localScale;
                activeBullet.transform.SetParent(transform.parent);
                activeBullet.transform.localScale = Vector3.Scale(tempScale, transform.localScale);
            }
            activeBullet.GetComponent <Rigidbody>().velocity = transform.forward * bulletSpeed * transform.lossyScale.x;
            AudioManager.Instance.Play("tankShoot");
            ammo--;
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }

    public void Reload()
    {
        ammo = ammoLimit;
    }

    public void TakeDamage() {
        //TODO take damage
    }
}
