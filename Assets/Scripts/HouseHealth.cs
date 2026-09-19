using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HouseHealth : MonoBehaviour
{
    private Image healthBarImage;
    [SerializeField] private GameObject gameOverScreen;
    private static HouseHealth instance;
    private static int workingChairAmount;
    private static int totalChairAmount;

    private void Awake()
    {
        instance = this;
        workingChairAmount = 0;
        totalChairAmount = 0;
        healthBarImage = GetComponent<Image>();
    }

    public static void IncreaseChairAmount()
    {
        totalChairAmount++;
        workingChairAmount++;
    }

    public static void DecreaseChairAmount()
    {
        workingChairAmount = Mathf.Max(0, workingChairAmount - 1);
    }

    private void Update()
    {
        instance.healthBarImage.fillAmount = totalChairAmount > 0
            ? (float)workingChairAmount / totalChairAmount
            : 0f;
        if (workingChairAmount == 0)
        {
            gameOverScreen.SetActive(true);
        }
    }

    public void ResetGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
}
