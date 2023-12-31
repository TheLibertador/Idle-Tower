using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeArea : MonoBehaviour, IInteractable
{
    [SerializeField] private Canvas upgradeCanvas;

    public void Interact()
    {
        upgradeCanvas.gameObject.SetActive(true);
    }

    public int GetInteractCount() => 1;

    public bool IsPermanent() => true;
}
