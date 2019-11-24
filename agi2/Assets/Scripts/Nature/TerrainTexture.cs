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

    Vector2Int _maskDimensions;
    float _maskPixelsPerWorldUnit;
    MeshRenderer _terrainRenderer;
    Material _rendererMaterial;

    /// This script assumes that the terrain renderer is a default unity plane (with default size 10 units) attached to the same GameObject.
    const float PlaneSize = 10;
    /// Extra margin for the grass fade radius, relative to the resource consumption radius.
    const float GrassFadeMargin = 1.25f;

    /// Cache for sphere checks during resource consumption. Also defines the maximum number of objects to check each frame.
    Collider[] physicsCache = new Collider[20];

    void Start()
    {
        _terrainRenderer = GetComponent<MeshRenderer>();
        _rendererMaterial = _terrainRenderer.material;

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
        var modifiedTexScale = new Vector2(transform.lossyScale.x, transform.lossyScale.z) * defaultTexScale;
        _rendererMaterial.SetTextureScale("_GrassTex", modifiedTexScale);

        defaultTexScale = _rendererMaterial.GetTextureScale("_SandTex");
        modifiedTexScale = new Vector2(transform.lossyScale.x, transform.lossyScale.z) * defaultTexScale;
        _rendererMaterial.SetTextureScale("_SandTex", modifiedTexScale);

        defaultTexScale = _rendererMaterial.GetTextureScale("_NoiseTex");
        modifiedTexScale = new Vector2(transform.lossyScale.x, transform.lossyScale.z) * defaultTexScale;
        _rendererMaterial.SetTextureScale("_NoiseTex", modifiedTexScale);

        _maskPixelsPerWorldUnit = _maskDimensions.x / transform.lossyScale.x / PlaneSize;

        _textureMask.Apply();
    }

    /// Consume resources (including grass and resource objects) in the specified spherical area (in world-space). 
    /// Return the resource gain from consuming these resources.
    public float ConsumeResourcesWorldSpace(Vector3 center, float radius)
    {
        // Consume resource objects (TODO: use a separate physics layer for resources?)
        int nResourceObjects = Physics.OverlapSphereNonAlloc(center, radius, physicsCache);
        float resourceGain = 0;
        for (int i = 0; i < nResourceObjects; i++)
        {
            ResourceObject resource = physicsCache[i].GetComponent<ResourceObject>();
            if (resource != null)
            {
                resourceGain += resource.ConsumeResource();
            }
        }
        // Consume grass
        Vector2Int boardPoint = WorldToTexturePoint(center);
        int boardRadius = Mathf.RoundToInt(radius * _maskPixelsPerWorldUnit);
        return resourceGain + FillCirclePixelSpace(boardPoint, boardRadius, 1);
    }

    /// Consume the grass in a pixel-space circle defined by center and radius.
    /// Returns the resource gain from consuming the grass.
    float FillCirclePixelSpace(Vector2Int center, int radius, float opacity)
    {
        float resourceGain = 0;
        // The radius is slightly inflated to make the grass texture better match the resource object consumption
        float sqrRad = radius * radius * GrassFadeMargin;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                float sqrDist = i*i + j*j;
                if (sqrDist <= sqrRad)
                {
                    int x = center.x + i;
                    int y = center.y + j;
                    Color a = _textureMask.GetPixel(x, y);
                    float distanceFactor = (1-sqrDist / sqrRad);
                    float lerpAmount = distanceFactor * opacity;
                    Color b = Color.Lerp(a, Color.black, lerpAmount);
                    resourceGain += (a.r - b.r) * _resourceGainPerGrassPixel;
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
