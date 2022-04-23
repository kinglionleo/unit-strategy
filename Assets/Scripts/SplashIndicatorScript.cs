using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashIndicatorScript : MonoBehaviour
{
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void finishSplash() {
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    public void startSplash(float damageRadius) {
        this.transform.localScale = new Vector3(damageRadius, damageRadius, 1);
        this.transform.position = new Vector3(this.transform.position.x, 0.05f, this.transform.position.z);
        this.gameObject.SetActive(true);
        Invoke(nameof(finishSplash), 0.5f);
    }
}
