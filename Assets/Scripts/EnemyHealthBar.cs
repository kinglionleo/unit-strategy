using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{

    public Image healthBar;
    public Image healthBarBackground;
    public float lerpSpeed;
    private Enemy myUnit;

    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<Enemy>();
        this.transform.rotation = Camera.main.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        HealthBarFiller();
        this.transform.rotation = Camera.main.transform.rotation;
    }

    void HealthBarFiller()
    {
        float current = myUnit.getCurrentHealth();
        float max = myUnit.getMaxHealth();
        healthBar.fillAmount = current / max;
        healthBarBackground.fillAmount = Mathf.Lerp(healthBarBackground.fillAmount, current / max, lerpSpeed);
        healthBar.color = Color.Lerp(Color.red, Color.green, current / max);
    }
}
