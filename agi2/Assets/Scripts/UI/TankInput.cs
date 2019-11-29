using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankInput : MonoBehaviour
{
    public static TankInput Instance { get; private set; }
    public ShootButton ShootButton => _shootButton;
    public Joystick Joystick => _joystick;

    [SerializeField]
    ShootButton _shootButton = null;
    [SerializeField]
    Joystick _joystick = null;

    void Awake()
    {
        Instance = this;
    }

}
