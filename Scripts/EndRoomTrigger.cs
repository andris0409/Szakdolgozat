using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{

	public class EndRoomTrigger : MonoBehaviour
	{
		public GameManager gameManager;

		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				if (gameManager != null)
				{
					gameManager.PlayerWon();
				}
				else
				{
					Debug.LogError("GameManager reference is missing");
				}
			}
		}
	}
}
