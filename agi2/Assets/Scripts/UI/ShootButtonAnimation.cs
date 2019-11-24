using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootButtonAnimation : MonoBehaviour
{
    Vector3 _originalScale;
    float _originalOpacity;
    Image _image;
    public float Duration { get; set; } = 1;

    // Start is called before the first frame update
    void Awake()
    {
        _image = GetComponent<Image>();
        _originalScale = transform.localScale;
        _originalOpacity = _image.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        float lerpStep = 1 - Mathf.Exp(-5 / Duration * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, _originalScale * 5, lerpStep);
        _image.color = Color.Lerp(_image.color, new Color(1,1,1,0), lerpStep);
    }
}
