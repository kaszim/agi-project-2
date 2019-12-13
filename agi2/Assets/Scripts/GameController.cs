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
            Transform spawnPoint = (NetworkedGameObject.Player == Player.Blue) ? GameObject.FindGameObjectWithTag("BlueBase").transform : GameObject.FindGameObjectWithTag("RedBase").transform;
            var tank = Instantiate(TankPrefab, GameWorld.transform);
            tank.transform.position = spawnPoint.position;
            var tm = tank.GetComponent<TankMovement>();
        }
    }
}
