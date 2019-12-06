using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameObject GameWorld => GameObject.FindWithTag("GameWorld");

    public GameObject TankPrefab;

    void Start()
    {
        UnityClient.Instance.OnGameStateChange += OnGameStateChange;
    }

    private void OnGameStateChange(GameState newState)
    {
        if (newState == GameState.Game)
        {
            var tank = Instantiate(TankPrefab, GameWorld.transform);
            var tm = tank.GetComponent<tankMovement>();
        }
    }
}
