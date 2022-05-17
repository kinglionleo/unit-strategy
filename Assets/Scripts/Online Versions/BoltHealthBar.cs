using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

public class BoltHealthBar : MonoBehaviour
{

    public Image healthBar;
    public Image healthBarBackground;
    public Image lifetimeBar;
    public float lerpSpeed;
    private BoltUnit myUnit;
    private Color32 redColor;

    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<BoltUnit>();
        redColor = new Color32(255, 79, 66, 255);
    }

    void FixedUpdate()
    {
        HealthBarFiller();
        LifeBarFiller();
        this.transform.rotation = Camera.main.transform.rotation;
    }

    void HealthBarFiller()
    {
        float current = myUnit.getCurrentHp();
        float max = myUnit.getMaxHp();
        healthBar.fillAmount = current/max;
        healthBarBackground.fillAmount = Mathf.Lerp(healthBarBackground.fillAmount, current / max, lerpSpeed);
        if (myUnit.entity.IsOwner)
        {
            healthBar.color = Color.Lerp(Color.red, Color.green, current / max);
        }
        else
        {
            healthBar.color = redColor;
        }
        
    }

    void LifeBarFiller()
    {
        // If the lifetime field of a unit is 0, it does not die on a timer, so we return
        if (myUnit.getLifetime() == 0) return;
        lifetimeBar.fillAmount = (myUnit.getLifetime() - (BoltNetwork.ServerTime - myUnit.getSpawnTime())) / myUnit.getLifetime();


    }
}
