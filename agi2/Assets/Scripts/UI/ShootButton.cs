using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(EventSystem))]
public class ShootButton : MonoBehaviour
{
    public UnityEvent OnShootClick { get; private set; } = new UnityEvent();
    [SerializeField]
    ShootButtonAnimation _animationObjectPrefab = null;
    [SerializeField]
    float _animationDuration = 1;

    public void TriggerOnShoot()
    {
        OnShootClick.Invoke();
        if (_animationObjectPrefab)
        {
            Vector3 position;
            if (Input.touchCount <= 1)
            {
                position = Input.mousePosition;
            }
            else
            {
                position = Input.GetTouch(1).position;
            }
            var instObj = Instantiate(_animationObjectPrefab, position, new Quaternion());
            instObj.transform.SetParent(transform);
            instObj.Duration = _animationDuration;
            Destroy(instObj, _animationDuration);
        }
    }

}
