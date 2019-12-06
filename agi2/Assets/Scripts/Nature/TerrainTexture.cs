using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTexture : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Dimension (in pixels) of the texture mask image on the x-axis. The y-value is automatically calculated based on scale. No effect unless Start Mask is null.")]
    int _maskPixelsX = 32;
    [SerializeField]
    float _resourceGainPerGrassPixel = 1;
    [SerializeField]
    [Tooltip("Optional start mask. Overrides Mask Pixels X. Make sure that the texture dimensions match the board scale.")]
    Texture2D _startMask = null;
    Texture2D _textureMask = null;
    [SerializeField]
    LayerMask _resourceObjectLayer = 0;
    public float WorldScale { get; private set; } = 1;

    Vector2Int _maskDimensions;
    float _maskPixelsPerWorldUnit;
    MeshRenderer _terrainRenderer;
    Material _rendererMaterial;

    /// This script assumes that the terrain renderer is a default unity plane (with default size 10 units) attached to the same GameObject.
    const float PlaneSize = 10;

    /// Cache for sphere checks during resource consumption. Also defines the maximum number of objects to check each frame.
    Collider[] _physicsCache = new Collider[20];

    float _redResources;
    float _blueResources;
    float _redResourcesStart;
    float _blueResourcesStart;
    bool _redIsUp = true;

    void Update()
    {
        if (Networking.NetworkedGameObject.Player == Networking.Player.Red)
        {
            TankGUI.Instance.SetResourceFraction(_redResources / _redResourcesStart);
        }
        else
        {
            TankGUI.Instance.SetResourceFraction(_blueResources / _blueResourcesStart);
        }
    }

    void Start()
    {
        _terrainRenderer = GetComponent<MeshRenderer>();
        _rendererMaterial = _terrainRenderer.material;
        if (transform.parent != null)
        {
            WorldScale = transform.parent.lossyScale.x;
        }

        if (_startMask == null)
        {
            _maskDimensions = new Vector2Int(_maskPixelsX, Mathf.RoundToInt(_maskPixelsX * transform.localScale.z / transform.localScale.x));
            _textureMask = new Texture2D(_maskDimensions.x, _maskDimensions.y);
            // Set all mask pixels to white (grass)
            for (int i = 0; i < _maskDimensions.x; i++)
            {
                for (int j = 0; j < _maskDimensions.y; j++)
                {
                    _textureMask.SetPixel(i, j, Color.white);
                }
            }
        }
        else
        {
            _maskDimensions = new Vector2Int(_startMask.width, _startMask.height);
            _textureMask = new Texture2D(_maskDimensions.x, _maskDimensions.y);
            var pixels = _startMask.GetPixels();
            _textureMask.SetPixels(pixels);
        }
        _textureMask.wrapMode = TextureWrapMode.Clamp;
        _rendererMaterial.SetTexture("_MaskTex", _textureMask);

        // Calculate material properties based on object scale
        var defaultTexScale = _rendererMaterial.GetTextureScale("_GrassTex");
        var modifiedTexScale = new Vector2(transform.lossyScale.x / transform.lossyScale.z, 1) * defaultTexScale;
        _rendererMaterial.SetTextureScale("_GrassTex", modifiedTexScale);

        defaultTexScale = _rendererMaterial.GetTextureScale("_SandTex");
        modifiedTexScale = new Vector2(transform.lossyScale.x / transform.lossyScale.z, 1) * defaultTexScale;
        _rendererMaterial.SetTextureScale("_SandTex", modifiedTexScale);

        defaultTexScale = _rendererMaterial.GetTextureScale("_NoiseTex");
        modifiedTexScale = new Vector2(transform.lossyScale.x / transform.lossyScale.z, 1) * defaultTexScale;
        _rendererMaterial.SetTextureScale("_NoiseTex", modifiedTexScale);

        _maskPixelsPerWorldUnit = _maskDimensions.x / transform.lossyScale.x / PlaneSize;

        _textureMask.Apply();

        foreach (var item in Transform.FindObjectsOfType<ResourceObject>())
        {
            if (Divider.Instance.IsOnRedSide(item.transform.position))
            {
                _redResources += item.GetResourceValue();
            }
            else
            {
                _blueResources += item.GetResourceValue();
            }
        }
        for (int x = 0; x < _textureMask.width; x++)
        {
            for (int y = 0; y < _textureMask.height; y++)
            {
                if (y < _maskDimensions.y / 2f && _redIsUp)
                {
                    _redResources += _textureMask.GetPixel(x, y).r * _resourceGainPerGrassPixel;
                }
                else
                {
                    _blueResources += _textureMask.GetPixel(x, y).r * _resourceGainPerGrassPixel;
                }
            }
        }
        _redResourcesStart = _redResources;
        _blueResourcesStart = _blueResources;
        _redIsUp = Divider.Instance.IsOnRedSide(transform.TransformPoint(Vector3.forward));
    }

    /// Consume resources (including grass and resource objects) in the specified spherical area (in world-space).
    /// Return the resource gain from consuming these resources.
    public float ConsumeResourcesWorldSpace(Vector3 center, float radius, float opacity)
    {
        if (radius <= Mathf.Epsilon)
        {
            return 0;
        }
        int nResourceObjects = Physics.OverlapSphereNonAlloc(center, radius * 0.75f, _physicsCache, _resourceObjectLayer);
        float resourceGain = 0;
        for (int i = 0; i < nResourceObjects; i++)
        {
            ResourceObject resource = _physicsCache[i].GetComponent<ResourceObject>();
            if (resource != null && !resource.IsDepleted)
            {
                float gain = resource.ConsumeResource();
                resourceGain += gain;
                if (Divider.Instance.IsOnRedSide(resource.transform.position))
                {
                    _redResources -= gain;
                }
                else
                {
                    _blueResources -= gain;
                }
            }
        }
        // Consume grass
        Vector2Int boardPoint = WorldToTexturePoint(center);
        float boardRadius = radius * _maskPixelsPerWorldUnit;
        return resourceGain + FillCirclePixelSpace(boardPoint, boardRadius, opacity);
    }

    /// Consume the grass in a pixel-space circle defined by center and radius.
    /// Returns the resource gain from consuming the grass.
    float FillCirclePixelSpace(Vector2Int center, float radius, float opacity)
    {
        if (radius <= Mathf.Epsilon)
        {
            return 0;
        }
        float resourceGain = 0;
        int radiusCeil = Mathf.CeilToInt(radius);
        float sqrRad = radius * radius;
        for (int i = -radiusCeil; i <= radiusCeil; i++)
        {
            for (int j = -radiusCeil; j <= radiusCeil; j++)
            {
                float sqrDist = i*i + j*j;
                if (sqrDist <= sqrRad)
                {
                    int x = center.x + i;
                    int y = center.y + j;
                    Color a = _textureMask.GetPixel(x, y);
                    float distanceFactor = 1- sqrDist / sqrRad;
                    float change = distanceFactor * opacity;
                    Color b = a - new Color(change, 0, 0, 0);
                    if (b.r < 0)
                    {
                        b.r = 0;
                    }
                    float gain = (a.r - b.r) * _resourceGainPerGrassPixel;
                    resourceGain += gain;
                    if (y < _maskDimensions.y / 2f && _redIsUp)
                    {
                        _redResources -= gain;
                    }
                    _textureMask.SetPixel(x, y, b);
                }
            }
        }
        _textureMask.Apply();
        return resourceGain;
    }

    /// Transform a position from world space to mask texture pixel coordinates.
    Vector2Int WorldToTexturePoint(Vector3 worldPosition)
    {
        var transformedPosition = transform.InverseTransformPoint(worldPosition);
        transformedPosition.x = (transformedPosition.x / PlaneSize + 0.5f) * _maskDimensions.x;
        transformedPosition.z = (transformedPosition.z / PlaneSize + 0.5f) * _maskDimensions.y;
        Vector2Int pixelPosition = new Vector2Int(
            _maskDimensions.x - Mathf.CeilToInt(transformedPosition.x),
            _maskDimensions.y - Mathf.CeilToInt(transformedPosition.z)
        );
        return pixelPosition;
    }
}
