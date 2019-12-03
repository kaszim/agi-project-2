using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flagcolor : MonoBehaviour
{
    [SerializeField] Texture flagTexture;
    // Start is called before the first frame update
    void Start()
    {
        Renderer render = gameObject.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.doubleSidedGI = false;
        mat.mainTexture = flagTexture;
        mat.enableInstancing = true;
        render.material = mat;
    }
}
