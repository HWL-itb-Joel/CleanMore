using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    //Volumen
    public Slider volumeSlider;
    public float volumeValue;

    //Brillo
    public Slider brightnessSlider;
    public float brightnessValue;
    public Image brightnessPanel;

    //Pantalla completa
    public Toggle screenToggle;

    //Resolución
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

    void Start()
    {
        //Volumen
        volumeSlider.value = PlayerPrefs.GetFloat("audioVolume", 0.5f);
        AudioListener.volume = volumeSlider.value;

        //Brillo
        brightnessSlider.value = PlayerPrefs.GetFloat("imageBrightness", 0.5f);
        brightnessPanel.color = new Color(brightnessPanel.color.r, brightnessPanel.color.g, brightnessPanel.color.b, brightnessSlider.value);

        //Pantalla completa
        if (Screen.fullScreen)
        {
            screenToggle.isOn = true;
        }
        else
        {
            screenToggle.isOn = false;
        }

        //Resolución
        CheckResolution();
    }
    
    public void ChangeVolume(float value)
    {
        volumeValue = value;
        PlayerPrefs.SetFloat("audioVolume", volumeValue);
        AudioListener.volume = volumeSlider.value;
    }

    public void ChangeBrightness(float value)
    {
        brightnessValue = value;
        PlayerPrefs.SetFloat("imageBrightness", brightnessValue);
        brightnessPanel.color = new Color(brightnessPanel.color.r, brightnessPanel.color.g, brightnessPanel.color.b, brightnessSlider.value);

    }

    public void ChangeFullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }

    public void CheckResolution()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> resolutionOptions = new List<string>();
        int actualResolution = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            resolutionOptions.Add(option);

            if(Screen.fullScreen && resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                actualResolution = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = actualResolution;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.value = PlayerPrefs.GetInt("resolutionNumber", 0);
    }

    public void ChangeResolution(int resolutionIndex)
    {
        PlayerPrefs.SetInt("resolutionNumber", resolutionDropdown.value);
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
