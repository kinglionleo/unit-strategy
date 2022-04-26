using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltSplashIndicatorScript : MonoBehaviour
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
        Debug.Log("called Finish splash");
        Destroy(this.gameObject);
    }

    public void startSplash(float damageRadius, BoltUnit enemy) {
        Debug.Log("called start splash");
        this.transform.localScale = new Vector3(damageRadius, damageRadius, 1);
        //this.transform.localScale = new Vector3(20, 20, 1);
        this.transform.position = new Vector3(enemy.transform.position.x, this.transform.position.y, enemy.transform.position.z);
        this.gameObject.SetActive(true);
        Invoke(nameof(finishSplash), 0.5f);
    }
}
