using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoltHealthBar : MonoBehaviour
{

    public Image healthBar;
    public Image healthBarBackground;
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
        this.transform.rotation = Camera.main.transform.rotation;
    }

    void HealthBarFiller()
    {
        float current = myUnit.getCurrentHealth();
        float max = myUnit.getMaxHealth();
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
}
