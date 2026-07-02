using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerUI : MonoBehaviourPun
{
    [Header("Nickname")]
    [SerializeField] private TMP_Text nicknameText;

    [Header("Health")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    [Header("Down")]
    [SerializeField] private TMP_Text downText;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private Color dangerColor = Color.red;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();

        canvas.worldCamera = Camera.main;
    }
    private void Start()
    {
        nicknameText.text = photonView.Owner.NickName;
        downText.gameObject.SetActive(false);
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        float percent = Mathf.Clamp01((float)currentHealth / maxHealth);

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

    public void SetDowned(bool value)
    {
        downText.gameObject.SetActive(value);

        if (value)
        {
            healthSlider.value = 0;
            fillImage.color = dangerColor;
        }
    }
}
