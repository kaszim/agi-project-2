/*
    Script used for testing the terrain texture and resource consumption.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainTexture))]
public class TerrainMouseInteraction : MonoBehaviour
{
    [SerializeField]
    Transform _fuelIndicator = null;

    TerrainTexture _terrain;
    int _debugPaintCounter = 0;
    float _debugPaintTimer = 0;

    void Awake()
    {
        _terrain = GetComponent<TerrainTexture>();
    }

    void Update()
    {
        // Paint sand with your mouse (for debugging). The scale of the brush increases while you hold mouse 0. 
        if (Input.GetMouseButton(0))
        {
            _debugPaintTimer += Time.deltaTime;
            if (_debugPaintTimer > 0.05f)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var gain = _terrain.ConsumeResourcesWorldSpace(hit.point, 1f);
                    if (_fuelIndicator != null)
                    {
                        _fuelIndicator.localScale = _fuelIndicator.localScale + Vector3.up * gain * 0.01f;
                    }
                    print("Fuel gain: " + gain);
                }
                _debugPaintTimer = 0;
                _debugPaintCounter++;
            }
        }
        else
        {
            _debugPaintTimer = 0;
            _debugPaintCounter = 1;
        }        
    }
}
