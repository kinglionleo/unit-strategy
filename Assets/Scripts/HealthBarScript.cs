using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{

    public Image healthBar;
    public Image healthBarBackground;
    public float lerpSpeed;
    private Unit myUnit;

    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        HealthBarFiller();
    }

    void HealthBarFiller()
    {
        float current = myUnit.getCurrentHealth();
        float max = myUnit.getMaxHealth();
        healthBar.fillAmount = current/max;
        healthBarBackground.fillAmount = Mathf.Lerp(healthBarBackground.fillAmount, current / max, lerpSpeed);
        healthBar.color = Color.Lerp(Color.red, Color.green, current / max);
    }
}
