using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : Stats
{
    [SerializeField]
    protected Image healthBar;

    protected override void AdjustHealthBar()
    {
        if (CurrentDamageFactor() != 1)
        {
            healthBar.fillAmount = CurrentDamageFactor();
        }
    }
}
