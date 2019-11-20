using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WheelControlSlider : Slider
{
    // Currently setting these values via SliderControlInput because otherwise we'll need a custom inspector and ugh...
    public float ElasticityStrength = 15;
    public float MaxReturnDelta = 0.2f;
    bool _sliderHandleGrabbed;
    float _targetValue;
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _sliderHandleGrabbed = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        _sliderHandleGrabbed = false;
    }

    protected override void Awake()
    {
        base.Awake();
        _targetValue = value;
    }

    protected override void Update()
    {
        base.Update();
        if (!_sliderHandleGrabbed && Application.isPlaying)
        {
            if (Mathf.Abs(value - _targetValue) <= MaxReturnDelta)
            {
                value = Mathf.Lerp(value, _targetValue, 1 - Mathf.Exp(-ElasticityStrength * Time.deltaTime));
            }
        }
    }
}
