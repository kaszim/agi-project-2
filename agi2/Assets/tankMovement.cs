using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tankMovement : MonoBehaviour
{
    [SerializeField] private float maxWheelSpeed;
    [SerializeField] private float wheelDist;
    [SerializeField] private SliderControlInput slider;
    [SerializeField] private float epsilon;
    [SerializeField] private float zOffset;

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = slider.CurrentInputValues;
        input *= maxWheelSpeed;

        if (Mathf.Abs(input.x - input.y) < epsilon)
        {
            transform.position += transform.forward * input.x * Time.deltaTime;
        }
        else
        {
            float rotationDist = (wheelDist / 2) * (input.x + input.y)/(input.y - input.x);
            float angularVelocity = (input.y - input.x) / wheelDist;
            rotationDist *= -1f;
            angularVelocity *= -1f;

            Vector3 pivot = transform.position + transform.right * rotationDist;
            transform.RotateAround(pivot, transform.up, Mathf.Rad2Deg * angularVelocity * Time.deltaTime);
            
        }
    }
}
