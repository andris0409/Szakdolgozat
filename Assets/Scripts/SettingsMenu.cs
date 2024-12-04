using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    // References to UI elements
    public Slider volumeSlider;
    public Dropdown resolutionDropdown;
    public Slider sensitivitySlider;

    private Resolution[] resolutions;

    void Start()
    {
       
    }

    // Volume control
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // Resolution control
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // Mouse sensitivity control
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        // Apply the sensitivity in your gameplay input script (not shown here)
    }
}
