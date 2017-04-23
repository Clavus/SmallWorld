using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TeleportSpace : MonoBehaviour
{
    [SerializeField] private Transform lookAt;
    [SerializeField] private Image image;
    [SerializeField] private Color imageDefaultColor = Color.white;
    [SerializeField] private Color imageActiveColor = Color.red;
    [SerializeField] private float imageDefaultScale = 0.5f;
    [SerializeField] private float imageActiveScale = 1f;

    public bool isInUse;

    private Transform cameraTranform;
    private bool isActive = false;

    private void Start()
    {
        cameraTranform = Camera.main.transform;
        image.color = imageDefaultColor;
        image.transform.localScale.Set(imageDefaultScale, imageDefaultScale, imageDefaultScale);
    }

    public bool IsLookingAt()
    {
        Vector3 targetDir = (lookAt.position - cameraTranform.position).normalized;
        return !isInUse && Vector3.Dot(cameraTranform.forward, targetDir) > 0.98f;
    }

    public bool IsActive()
    {
        return isActive;
    }
    
    public void Activate()
    {
        if (!isActive)
        {
            isActive = true;
            image.DOKill();
            image.DOColor(imageActiveColor, 0.25f);
            image.transform.DOKill();
            image.transform.DOScale(imageActiveScale, 0.25f);
        }
    }

    public void Deactivate()
    {
        if (isActive)
        {
            isActive = false;
            image.DOKill();
            image.DOColor(imageDefaultColor, 1f);
            image.transform.DOKill();
            image.transform.DOScale(imageDefaultScale, 1f);
        }
    }
}
