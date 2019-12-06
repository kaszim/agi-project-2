using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceIconUI : MonoBehaviour
{
    public enum IconAlignment { centered, left, right }
    
    [SerializeField]
    Image iconPrefab = null;
    [SerializeField]
    IconAlignment aligment = IconAlignment.centered;
    [SerializeField]
    float spacing = 0;

    public int ResourceValue { get; set; }
    List<Image> activeIcons = new List<Image>();

    // Start is called before the first frame update
    void Start()
    {
        UpdateIcons();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateIcons();
    }

    void UpdateIcons()
    {
        for (int i = 0; i < ResourceValue; i++)
        {
            Image icon;
            if (activeIcons.Count <= i)
            {
                icon = Instantiate(iconPrefab, transform);
                activeIcons.Add(icon);
            }
            else
            {
                icon = activeIcons[i];
            }
            var offset = i * (icon.rectTransform.rect.width * icon.transform.localScale.x + spacing);
            if (aligment == IconAlignment.centered)
            {
                offset -= (ResourceValue - 1) * 0.5f * (icon.rectTransform.rect.width * icon.transform.localScale.x + spacing);
            }
            else if (aligment == IconAlignment.right)
            {
                offset = -offset;
            }
            activeIcons[i].gameObject.SetActive(true);
            icon.transform.localPosition = offset * Vector3.right;
        }
        for (int i = ResourceValue; i < activeIcons.Count; i++)
        {
            activeIcons[i].gameObject.SetActive(false);
        }
    }
}
