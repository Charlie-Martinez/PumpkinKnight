using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerMovement player;
    public Image fillImage;

    // Update is called once per frame
    void Update()
    {
        if (player != null && fillImage != null)
        {
            float healthPercent = (float)player.currentHealth / player.maxHealth;
            fillImage.fillAmount = healthPercent;
        }
    }
}
