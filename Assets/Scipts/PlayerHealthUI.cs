using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private TMP_Text downText;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private Color dangerColor = Color.red;

    private void Awake()
    {
        downText.gameObject.SetActive(false);
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        float percent = (float)currentHealth / maxHealth;

        fillImage.fillAmount = percent;

        if (percent > 0.6f)
        {
            fillImage.color = healthyColor;
        }
        else if (percent > 0.3f)
        {
            fillImage.color = warningColor;
        }
        else
        {
            fillImage.color = dangerColor;
        }
    }

    public void SetDowned(bool downed)
    {
        healthBar.SetActive(!downed);
        downText.gameObject.SetActive(downed);
    }
}
