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

    NetworkedGameObject networkedGameObject;

    // Start is called before the first frame update
    void Start()
    {
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
        AudioManager.Instance.TankSound(input); //Update tank engine sound
        input *= maxWheelSpeed;

        if (Mathf.Abs(input.x - input.y) < epsilon) {
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
