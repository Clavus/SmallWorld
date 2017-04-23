using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private GameObject bar;
    [SerializeField] private Renderer barRenderer;
    [SerializeField] private GameObject background;
    public Gradient healthColor;
    
    private float baseScale = 0;

    void Awake()
    {
        baseScale = bar.transform.localScale.x;
    }

    void Start()
    {
        SetHealth(1);
    }

    public void Hide()
    {
        bar.SetActive(false);
        background.SetActive(false);
    }

    public void Show()
    {
        bar.SetActive(true);
        background.SetActive(true);
    }

    public void SetHealth(float frac)
    {
        bar.transform.localScale.Set(frac * baseScale, 1, 1);
        barRenderer.material.color = healthColor.Evaluate(frac);
    }
    
}
