using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    [SerializeField]
    private Sprite goldCoinSprite;

    [SerializeField]
    private TMP_Text selectedTitle;

    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private Image selectedImage;

    [SerializeField]
    private ResourceCostData costData;

    [SerializeField]
    private HealthData[] healthDatas;

    [SerializeField]
    private TMP_Text selectedCost;

    [SerializeField]
    private GameObject costInfo;

    public void DisplayInfo(int index)
    {
        selectedImage.sprite = sprites[index];
        selectedImage.rectTransform.sizeDelta = new Vector2(32, 32);
        costInfo.SetActive(true);

        switch (index)
        {
            case 0:
                selectedTitle.text = "Archer";
                selectedCost.text = costData.archerCost.ToString();
                break;

            case 1:
                selectedTitle.text = "Gold Mine";
                selectedCost.text = costData.goldMineCost.ToString();
                break;

            case 2:
                selectedTitle.text = "Wood Tower";
                selectedCost.text = costData.towerCost.ToString();
                selectedImage.rectTransform.sizeDelta = new Vector2(16, 32);

                break;

            case 3:
                selectedTitle.text = "Wood Wall";
                selectedCost.text = costData.wallCost.ToString();
                break;

            case 4:
                selectedTitle.text = "Castle";
                costInfo.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();

        }
    }
}
