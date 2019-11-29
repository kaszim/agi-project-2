using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;

public class tankMovement : MonoBehaviour {
    //Tank movement related
    [SerializeField] private float maxWheelSpeed;
    [SerializeField] private float wheelDist;
    [SerializeField] private SliderControlInput slider;
    [SerializeField] private float epsilon;

    //Bullet related
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Vector3 bulletOrigin;

    //Track related
    private GameObject tankTracks;
    private Renderer tankRenderer;

    NetworkedGameObject networkedGameObject;

    // Start is called before the first frame update
    void Start()
    {
        string path = this.name;
        Debug.Log(path + "/Tracks/Track");
        tankTracks = GameObject.Find(this.name + "/Tracks/Track"); // Hierarcy reference to the tracks.
        tankRenderer = tankTracks.GetComponent<Renderer>();
        networkedGameObject = GetComponent<NetworkedGameObject>();
        slider.OnShootClick.AddListener(Shoot);
    }

    // Update is called once per frame
    void Update()
    {
        // If the object is networked and we are not the owner
        if (networkedGameObject != null && networkedGameObject.enabled && !networkedGameObject.Owner)
        {
            // We are not the owner of this object, therefore disable movement script
            this.enabled = false;
            slider.OnShootClick.RemoveListener(Shoot);
        }

        //Tankmovement
        Vector2 input = slider.CurrentInputValues;
        float realX = input.x;
        float realY = input.y;
        AudioManager.Instance.TankSound(input); //Update tank engine sound
        input *= maxWheelSpeed;

        //If the sliders are not in the deadzone 
        if(Mathf.Abs(realX) >= 0.001F || Mathf.Abs(realY) >= 0.001F)
        {
            //Animate the tankTracks by moving the uvs. Negated value needed to simulate correct direction
            int direction = realX < 0 ? -1 : 1;

            // Prevents the tracks from not animating when sliders are maxed out.
            float inputSpeed = Mathf.Abs(input.x) > Mathf.Abs(input.y) ? input.x : input.y;
            if (inputSpeed == maxWheelSpeed || inputSpeed < 0.002) inputSpeed = maxWheelSpeed - 0.01F;

            float offsetTexture = direction * (Mathf.Abs(inputSpeed * 0.4f) % 2F) / 2; // Offset is between 0 and 1
            tankRenderer.material.mainTextureOffset += new Vector2(offsetTexture * Time.deltaTime, 0);
        }

        if (Mathf.Abs(input.x - input.y) < epsilon)
        {
            transform.position += transform.forward * input.x * Time.deltaTime;
        } else {
            float rotationDist = (wheelDist / 2) * (input.x + input.y) / (input.y - input.x);
            float angularVelocity = (input.y - input.x) / wheelDist;
            rotationDist *= -1f;
            angularVelocity *= -1f;

            Vector3 pivot = transform.position + transform.right * rotationDist;
            transform.RotateAround(pivot, transform.up, Mathf.Rad2Deg * angularVelocity * Time.deltaTime);

        }
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
