using Assets.Scripts.ModularFirstPersonController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{

	public class GameManager : MonoBehaviour
	{
		public GameObject caughtScreen;
		public GameObject winScreen;
		public FirstPersonController playerController;

		public void PlayerCaught()
		{
			caughtScreen.SetActive(true);
			playerController.enabled = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void PlayerWon()
		{
			winScreen.SetActive(true);  
			playerController.enabled = false;  
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void ReturnToMenu()
		{
			SceneManager.LoadScene("MainMenu");
		}
	}
}
