using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoltHUDListener : MonoBehaviour
{
    private static BoltHUDListener _instance;
    public Button selected = null;

    public static BoltHUDListener Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void Enlarge(Button b)
    {
        LeanTween.scale(b.gameObject, new Vector2(1.2f, 1.2f), 0.2f).setEase(LeanTweenType.easeOutBack);
    }

    public void Shrink(Button b)
    {
        if(selected != b)
        {
            LeanTween.scale(b.gameObject, new Vector2(1, 1), 0.2f).setEase(LeanTweenType.easeOutBack);
        }
    }

    public void OnClick(Button b)
    {
        if(selected == b)
        {
            DeselectAll();
            selected = null;
        }
        else if(selected != null)
        {
            DeselectAll();
            selected = b;
            Enlarge(b);
        }
        else
        {
            selected = b;
            Enlarge(b);
        }
    }

    public void DeselectAll()
    {
        if(selected == null)
        {
            return;
        }
        LeanTween.scale(selected.gameObject, new Vector2(1, 1), 0.2f).setEase(LeanTweenType.easeOutBack);
        selected = null;
    }
}
