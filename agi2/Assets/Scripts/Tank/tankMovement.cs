using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;

public class tankMovement : MonoBehaviour {
    //Tank movement related
    [SerializeField] private float movementSpeed = 5;
    [SerializeField] private float turnSpeed = 1;
    [SerializeField] private float epsilon;

    //Bullet related
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Vector3 bulletOrigin;

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
        float targetY = transform.rotation.eulerAngles.y + rotateSign * Time.deltaTime * input.x * rotationSpeed;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetY, transform.rotation.eulerAngles.z);
        rigidbody.MovePosition(transform.position + transform.forward * input.x * Time.deltaTime * movementSpeed * transform.localScale.x);
        TankInput.Instance.Joystick.UpdateTankRotationIndicator(transform.rotation.eulerAngles.y);
        //Animate the tankTracks by moving the uvs
        var offset = (tankRenderer.material.mainTextureOffset.x + input.x * Time.deltaTime * movementSpeed * 0.05f) % 1f;
        tankRenderer.material.mainTextureOffset = new Vector2(offset, tankRenderer.material.mainTextureOffset.y);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject activeBullet = Instantiate(bullet, transform.TransformPoint(bulletOrigin), Quaternion.identity);
        activeBullet.GetComponent <Rigidbody>().velocity = transform.forward * bulletSpeed;
        AudioManager.Instance.Play("tankShoot");
    }

}
