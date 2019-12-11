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
    [SerializeField] private int health = 5;

    //Bullet related
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;

    //ammo
    private int ammo = 5;
    [SerializeField] private int ammoLimit;

    //Track related
    private GameObject tankTracks;
    private Renderer tankRenderer;


    NetworkedGameObject networkedGameObject;
    new Rigidbody rigidbody;

    [SerializeField] GameObject upperBody = null;
    [SerializeField] Transform bulletOrigin = null;

    // Start is called before the first frame update
    void Start()
    {
        networkedGameObject = GetComponent<NetworkedGameObject>();
        // If the object is networked and we are not the owner
        if (networkedGameObject != null && networkedGameObject.enabled && !networkedGameObject.IsOwned)
        {
            // We are not the owner of this object, therefore disable movement script
            this.enabled = false;
            return;
        }
        string path = this.name;
        tankTracks = GameObject.Find(this.name + "/Tracks/Track"); // Hierarcy reference to the tracks.
        tankRenderer = tankTracks.GetComponent<Renderer>();
        rigidbody = GetComponent<Rigidbody>();
        TankGUI.Instance.ShootButton.OnShootClick.AddListener(Shoot);
        ammo = ammoLimit;
    }

    // Update is called once per frame
    void Update()
    {
        //Tankmovement
        Vector2 input = TankGUI.Instance.Joystick.InputPositionPolar;
        AudioManager.Instance.TankSound(new Vector2(input.x, input.x)); //Update tank engine sound
        float rotateSign = Mathf.Sign(input.y);
        float rotationSpeed = (1 - Mathf.Abs((input.y) / Mathf.PI)) * 2000 * turnSpeed;
        float rotationDelta = rotateSign * input.x * rotationSpeed;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + rotationDelta * Time.deltaTime, 0);
        float movementDelta = input.x * movementSpeed * transform.lossyScale.x;
        if (Mathf.Abs(movementDelta) > 0.01f)
        {
            transform.position += transform.forward * movementDelta * Time.deltaTime;
        }
        // Since we currently move the tank by setting the position directly, we can force the velocity to 0 to reduce physics errors in AR.
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

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

        // Update GUI components
        TankGUI.Instance.Joystick.UpdateTankRotationIndicator(transform.rotation.eulerAngles.y);
        TankGUI.Instance.AmmoUI.ResourceValue = ammo;
        TankGUI.Instance.HealthUI.ResourceValue = health;
    }

    void Shoot()
    {
        if(ammo > 0){
            Ray ray;
            if (Input.touchCount <= 1)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(1).position);
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 target = hit.point;
                upperBody.transform.LookAt(target);
                Vector3 rotation = upperBody.transform.rotation.eulerAngles;
                rotation.x = 0;
                rotation.z = 0;
                Vector3 bulletRotation = rotation;
                rotation.y -= 90;
                upperBody.transform.rotation = Quaternion.Euler(rotation);
                GameObject activeBullet = Instantiate(bullet, bulletOrigin.position, Quaternion.Euler(bulletRotation));
                if (transform.parent != null)
                {
                    Vector3 tempScale = activeBullet.transform.localScale;
                    activeBullet.transform.SetParent(transform.parent);
                    activeBullet.transform.localScale = Vector3.Scale(tempScale, transform.localScale);
                }
                var bulletComponent = activeBullet.GetComponent<BulletCollision>();
                bulletComponent.Direction = bulletComponent.transform.forward;
                bulletComponent.Speed = bulletSpeed;
                bulletComponent.XLossyScale = transform.lossyScale.x;
                ammo--;
            }
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }

    /// Returns true if the ammo increased from this action.
    public bool Reload()
    {
        if (ammo < ammoLimit)
        {
            ammo = ammoLimit;
            return true;
        }
        return false;
    }

    public void TakeDamage()
    {
        health--;
        if(health == 0 && networkedGameObject != null && networkedGameObject.enabled){
            if(networkedGameObject.IsOwned){
                Winscreen.Instance.ActivateLose();
            }
            else{
                Winscreen.Instance.ActivateWin();
            }
        }
    }
}
