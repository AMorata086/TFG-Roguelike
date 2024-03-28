using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIScript : MonoBehaviour
{
    [SerializeField] private UIDocument userInterface;
    private ProgressBar healthBar;

    private void OnEnable()
    {
        VisualElement root = userInterface.rootVisualElement;
        healthBar = root.Q<ProgressBar>("HealthPoints");
    }

    public void updateHealthBar(int healthPoints, int maxHealth)
    {
        healthBar.value = healthPoints;
        healthBar.highValue = maxHealth;
        healthBar.title = healthPoints + "/" + maxHealth;
    }

}
