using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MainMenu : MonoBehaviour
    {
		public GameObject settingsPanel;
		public GameObject mainMenuPanel;
		public SettingsManager SettingsManager;
		public void StartGame()
        {
			SceneManager.LoadScene("SampleScene");
        }

		public void OpenSettings()
		{
			settingsPanel.SetActive(true);
			mainMenuPanel.SetActive(false);
		}

		public void CloseSettings()
		{
			mainMenuPanel.SetActive(true);
			settingsPanel.SetActive(false);
			SettingsManager.SaveSettings();
		}


		public void QuitGame()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
