using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    /// Returns the joystick position in (x, y) coordinates in the interval [-1, 1].
    public Vector2 InputPosition { get; private set; }
    /// Returns the joystick position in (r, theta) polar coordinates. r is in the interval [0, 1]. Theta is in the interval [-pi, pi].
    public Vector2 InputPositionPolar { get; private set; }

    [SerializeField]
    GameObject joystickHandle = null;
    [SerializeField]
    float dragSpeed = 30;
    [SerializeField]
    GameObject compass = null;

    RectTransform rect;
    bool dragging = false;
    Vector3 dragTargetPosition;
    float maxDragDistance;
    Transform cameraTransform;

	public void OnDrag(PointerEventData eventData)
	{
        dragTargetPosition = Vector3.MoveTowards(Vector3.zero, rect.InverseTransformPoint(eventData.position), maxDragDistance);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
        dragTargetPosition = Vector3.MoveTowards(Vector3.zero, rect.InverseTransformPoint(eventData.position), maxDragDistance);
        dragging = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
        dragging = false;
        dragTargetPosition = Vector3.zero;
	}

	// Start is called before the first frame update
	void Start()
    {
        rect = GetComponent<RectTransform>();
        joystickHandle.transform.position = transform.position;
        dragTargetPosition = Vector3.zero;
        maxDragDistance = rect.rect.width / 2 - joystickHandle.GetComponent<RectTransform>().rect.width / 2;
        cameraTransform = Camera.main.transform;
    }

    public void UpdateTankRotationIndicator(float worldSpaceTankRotationY)
    {
        // NOTE: Not sure about how well this solution works in AR.
        compass.transform.rotation = Quaternion.Euler(new Vector3(0, 0, cameraTransform.eulerAngles.y - worldSpaceTankRotationY));
    }

    // Update is called once per frame
    void Update()
    {
        float transitionSpeedMult = dragging? 2 : 1;
        joystickHandle.transform.localPosition = Vector3.Lerp(joystickHandle.transform.localPosition, dragTargetPosition, 1 - Mathf.Exp(-transitionSpeedMult * dragSpeed * Time.deltaTime));
        Vector3 transformedPosition = compass.transform.InverseTransformPoint(joystickHandle.transform.position) * compass.transform.lossyScale.x;
        InputPosition = new Vector2
        (
            (transformedPosition.x / maxDragDistance) / transform.lossyScale.x,
            (transformedPosition.y / maxDragDistance) / transform.lossyScale.y
        );
        InputPositionPolar = new Vector2
        (
            InputPosition.magnitude,
            Mathf.Atan2(InputPosition.x, -InputPosition.y)
        );
    }


}
