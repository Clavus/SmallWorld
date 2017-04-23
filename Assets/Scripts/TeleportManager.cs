using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class TeleportManager : MonoBehaviour
{
    [SerializeField] private TeleportSpace[] spaces;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private VRTK_ControllerEvents eventsLeft;
    [SerializeField] private VRTK_ControllerEvents eventsRight;
    [SerializeField] private Transform VRSpace;

    private bool isSearching = false;

    void Reset()
    {
        spaces = GetComponentsInChildren<TeleportSpace>();
    }
    
	void Start ()
    {
        canvasGroup.alpha = 0;
        eventsLeft.ButtonOnePressed += OnTeleportPressed;
        eventsLeft.ButtonOneReleased += OnTeleportReleased;
        eventsRight.ButtonOnePressed += OnTeleportPressed;
        eventsRight.ButtonOneReleased += OnTeleportReleased;
    }
	
	void Update ()
    {
		if (isSearching)
        {
            bool gotActive = false;
            foreach(TeleportSpace space in spaces)
            {
                if (space.IsLookingAt() && !gotActive)
                {
                    space.Activate();
                    gotActive = true;
                }
                else
                    space.Deactivate();
            }
        }
	}

    private void OnTeleportPressed(object sender, ControllerInteractionEventArgs e)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.25f);
        isSearching = true;
    }

    private void OnTeleportReleased(object sender, ControllerInteractionEventArgs e)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, 1);
        isSearching = false;

        foreach (TeleportSpace space in spaces)
        {
            space.isInUse = false;
            if (space.IsActive())
            {
                VRSpace.transform.position = space.transform.position;
                VRSpace.transform.rotation = space.transform.rotation;
                space.isInUse = true;
            } 
        }
            
    }
}
