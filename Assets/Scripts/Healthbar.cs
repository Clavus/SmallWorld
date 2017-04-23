using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private GameObject bar;
    [SerializeField] private Renderer barRenderer;
    [SerializeField] private GameObject background;
    public Gradient healthColor;

    private float baseScale = 0;
    private float currentFrac = 1;
    private float targetFrac = 1;
    private Vector3 cachedScale = Vector3.zero;

    void Awake()
    {
        baseScale = bar.transform.localScale.x;
        cachedScale = bar.transform.localScale;
        UpdateBar();
    }
    
    void Update()
    {
        if (targetFrac != currentFrac)
        {
            currentFrac = Mathf.MoveTowards(currentFrac, targetFrac, Time.deltaTime * 0.5f);
            UpdateBar();
        }
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
        targetFrac = frac;
    }

    private void UpdateBar()
    {
        cachedScale.x = currentFrac * baseScale;
        bar.transform.localScale = cachedScale;
        barRenderer.material.color = healthColor.Evaluate(currentFrac);
    }
    
}
