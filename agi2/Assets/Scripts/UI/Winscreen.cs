using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Winscreen : MonoBehaviour
{
    public GameObject winObject;
    public GameObject loseObject;


    private static Winscreen instance;
    public static Winscreen Instance{
        get {
            return instance;
        }
    }

    void Awake(){
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateWin(){
        GetComponent <Image>().enabled = true;
        winObject.SetActive(true);

    }

    public void ActivateLose(){
        GetComponent <Image>().enabled = true;
        loseObject.SetActive(true);
    }

}
