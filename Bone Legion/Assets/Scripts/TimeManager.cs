// Author: Hope Oberez (H.O)
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;

// Handles the game's timer and day and night cycle
public class TimeManager : MonoBehaviour
{
    private int minutes;
    private int hours;
    private int days;
    private bool paused;

    public int Minutes
    { get { return minutes; }  set { minutes = value; OnMinuteChange(value); } }
    public int Hours
    { get { return hours; } set { hours = value; OnHoursChange(value); } }
    public int Days
    { get { return days; } set { days = value; OnDayChange(value); } }


    [SerializeField]
    private TMP_Text timeText;

    [SerializeField] 
    private TMP_Text daysText;

    [SerializeField]
    private TMP_Text periodOfDayText;

    [SerializeField]
    private Light2D light2D;

    private float tick;

    private void Start()
    {
        paused = false;
        NightLight();
    }

    public void StartTime()
    {
        StartCoroutine(UpdateTime());
    }

    public void EndTime()
    {
        StopCoroutine(UpdateTime());
    }

    IEnumerator UpdateTime()
    {
        Minutes = 0;
        while (true)
        {
            if (GameManager.Instance.state == GameState.GamePaused)
            {
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                timeText.text = string.Format("{0:00}:{1:00}", Hours, Minutes);
                daysText.text = "Day " + Days;

                atSunrise();
                atSunset();

                yield return new WaitForSeconds(1);
                Minutes += 10;
            }
        }
    }

    private void OnMinuteChange(int value)
    {
        if (value >= 60)
        {
            Hours += 1;
            minutes = 0;
        }
    }

    private void OnHoursChange(int value)
    {
        if (value >= 24)
        {
            Hours = 0;
            Days++;
        }

    }

    private void OnDayChange(int value)
    {
        return;
    }

    public void atSunset()
    {
        if (Hours == 16 && Minutes == 0)
        {
            periodOfDayText.text = "Sunset";
            periodOfDayText.color = new Color(1, 0.6896f, 0.1933f);

            tick = 1.2f;
            return;
        }

        if (Hours >= 16 && Hours <= 17)
        {
            tick -= 0.01f;
            UpdateLight();
            Debug.Log(light2D.color);
        }

        if (Hours == 18 && Minutes == 0)
        {
            periodOfDayText.text = "Night";
            periodOfDayText.color = new Color(1, 0.08018869f, 0.08018869f);
            NightLight();
        }

    }

    public void atSunrise()
    {

        if (Hours == 6 && Minutes == 0)
        {
            periodOfDayText.text = "Sunrise";
            periodOfDayText.color = new Color(1, 0.6896f, 0.1933f);

            tick = 0f;
            return;
        }

        if (Hours >= 6 && Hours <= 7)
        {
            tick += 0.01f;
            UpdateLight();
        }

        if (Hours == 8 && Minutes == 0)
        {
            periodOfDayText.text = "Day";
            periodOfDayText.color = new Color(1, 1, 1);

            DayLight();
        }
    }

    public void UpdateLight()
    {
        float red = 0.38f * Mathf.Pow(tick + 0.33f, 2) + 0.12f;
        float green = 0.6f * Mathf.Pow(tick + 0.04f, 2) + 0.13f;
        float blue = 0.34f * Mathf.Pow(tick, 2) + 0.3f;
        light2D.color = new Color(red, green, blue, 1);
    }

    public void DayLight()
    {
        light2D.color = new Color(1, 0.9411765f, 0.788253f, 1);
    }

    public void NightLight()
    {
        light2D.color = new Color(0.1568628f, 0.1372549f, 0.3019608f, 1);
    }
}
