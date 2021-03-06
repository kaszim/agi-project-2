﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankGUI : MonoBehaviour
{
    public static TankGUI Instance { get; private set; }
    public ShootButton ShootButton => _shootButton;
    public Joystick Joystick => _joystick;
    public ResourceIconUI HealthUI => _healthUI;
    public ResourceIconUI AmmoUI => _ammoUI;

    [SerializeField]
    ShootButton _shootButton = null;
    [SerializeField]
    Joystick _joystick = null;
    [SerializeField]
    ResourceIconUI _healthUI = null;
    [SerializeField]
    ResourceIconUI _ammoUI = null;
    [SerializeField]
    Image _natureMeter = null;

    void Awake()
    {
        Instance = this;
    }

    public void SetResourceFraction(float fraction)
    {
        _natureMeter.fillAmount = fraction;
        if (fraction > 0.5f)
        {
            _natureMeter.color = Color.Lerp(Color.yellow, Color.green, (fraction - 0.5f) * 2);
        }
        else
        {
            _natureMeter.color = Color.Lerp(Color.red, Color.yellow, fraction * 2);
        }
    }

}
