using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderControlInput : MonoBehaviour
{
    [SerializeField]
    WheelControlSlider _leftSlider = null;
    [SerializeField]
    WheelControlSlider _rightSlider = null;
    [SerializeField]
    ShootButton _shootButton = null;
    [SerializeField]
    [Tooltip("Controls the maximum distance at which the slider will return to its default value.")]
    float _maxReturnDelta = 0.2f;
    [SerializeField]
    [Tooltip("Controls the speed at which the slider returns to its default value.")]
    float _elasticityStrength = 15;

    /// Returns the current input values in the range [-1, 1].
    public Vector2 CurrentInputValues { get; private set; }
    public EventTrigger OnShootButtonClick { get; private set; }

    /// Event which triggers when the shoot-button is clicked
    public UnityEvent OnShootClick => _shootButton.OnShootClick;
    
    void Update()
    {
        if (_leftSlider != null && _rightSlider != null)
        {
            CurrentInputValues = new Vector2(_leftSlider.value - 0.5f, _rightSlider.value - 0.5f) * 2;
            _leftSlider.ElasticityStrength = _elasticityStrength;
            _leftSlider.MaxReturnDelta = _maxReturnDelta;
            _rightSlider.ElasticityStrength = _elasticityStrength;
            _rightSlider.MaxReturnDelta = _maxReturnDelta;
        }
    }

}
