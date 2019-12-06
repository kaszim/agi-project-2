using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObject : MonoBehaviour
{
    
    [SerializeField]
    float _resourceValue = 1;
    [SerializeField]
    bool _valueScalesWithVolume = true;
    
    [Header("Depletion animation")]
    [SerializeField]
    float _depletionTime = 1;
    [SerializeField]
    AnimationCurve _depletionCurveWidth = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField]
    AnimationCurve _depletionCurveHeight = AnimationCurve.Linear(0, 1, 1, 0);

    /// Returns true if the objects resources have been spent
    public bool IsDepleted { get; private set; }

    // Animation curve variables
    float _depletionCurveValue = 0;
    Vector3 defaultScale;

    void Awake()
    {
        defaultScale = transform.localScale;
    }

    void Update()
    {
        // Scale according to depletion curve if depleted
        if (IsDepleted)
        {
            float width = _depletionCurveWidth.Evaluate(_depletionCurveValue / _depletionTime);
            float height = _depletionCurveHeight.Evaluate(_depletionCurveValue / _depletionTime);
            transform.localScale = new Vector3
            (
                width * defaultScale.x,
                height * defaultScale.y,
                width * defaultScale.z
            );
            _depletionCurveValue += Time.deltaTime;
        }
    }

    /// Consume the object resources, marking it as depleted, triggering its depletion animation and returning the resource value.
    public float ConsumeResource()
    {
        if (!IsDepleted)
        {
            IsDepleted = true;
            Destroy(gameObject, _depletionTime);
            if (_valueScalesWithVolume)
            {
                return _resourceValue * transform.localScale.x * transform.localScale.y * transform.localScale.z;
            }
            else
            {
                return _resourceValue;
            }
        }
        return 0;
    }

    public float GetResourceValue()
    {
        if (_valueScalesWithVolume)
        {
            return _resourceValue * transform.localScale.x * transform.localScale.y * transform.localScale.z;
        }
        else
        {
            return _resourceValue;
        }
    }
}
