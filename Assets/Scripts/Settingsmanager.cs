using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{


	public class SettingsManager : MonoBehaviour
	{
		public InputField widthInput;
		public InputField heightInput;
		public InputField roomAmountInput;
		public InputField startRoomExitsInput;
		public InputField endRoomDistanceInput;
		public InputField keyDistanceInput;
		public InputField numberOfGuardsInput;
		public InputField minPatrolDistanceInput;
		public InputField lockerSpawnChanceInput;
		public Toggle lockPathInput;
		public Toggle lockEndroomInput;

		public  void  SaveSettings()
		{
			int width = int.TryParse(widthInput.text, out width) ? width : 10;
			int height = int.TryParse(heightInput.text, out height) ? height : 10;
			int roomAmount = int.TryParse(roomAmountInput.text, out roomAmount) ? roomAmount : 50;
			int startRoomExits = int.TryParse(startRoomExitsInput.text, out startRoomExits) ? startRoomExits : 4;
			int endRoomDistance = int.TryParse(endRoomDistanceInput.text, out endRoomDistance) ? endRoomDistance : 10;
			int keyDistance = int.TryParse(keyDistanceInput.text, out keyDistance) ? keyDistance : 10;
			int numberOfGuards = int.TryParse(numberOfGuardsInput.text, out numberOfGuards) ? numberOfGuards : 10;
			int minPatrolDistance = int.TryParse(minPatrolDistanceInput.text, out minPatrolDistance) ? minPatrolDistance : 10;
			int lockerSpawnChance = int.TryParse(lockerSpawnChanceInput.text, out lockerSpawnChance) ? lockerSpawnChance : 10;
			bool lockPath = lockPathInput.isOn;
			bool lockEndroom = lockEndroomInput.isOn;

			PlayerPrefs.SetInt("Width", width);
			PlayerPrefs.SetInt("Height", height);
			PlayerPrefs.SetInt("RoomAmount", roomAmount);
			PlayerPrefs.SetInt("StartRoomExits", startRoomExits);
			PlayerPrefs.SetInt("EndRoomDistance", endRoomDistance);
			PlayerPrefs.SetInt("KeyDistance", keyDistance);
			PlayerPrefs.SetInt("NumberOfGuards", numberOfGuards);
			PlayerPrefs.SetInt("MinPatrolDistance", minPatrolDistance);
			PlayerPrefs.SetInt("LockerChance", lockerSpawnChance);
			PlayerPrefs.SetInt("LockPath", lockPath ? 1 : 0);
			PlayerPrefs.SetInt("LockEndRoomDoor", lockEndroom ? 1 : 0);

			PlayerPrefs.Save(); 
			Debug.Log("height: " + height + " width: " + width + " roomAmount: " + roomAmount + " startRoomExits: " + startRoomExits + " endRoomDistance: " + endRoomDistance + " keyDistance: " + keyDistance + " numberOfGuards: " + numberOfGuards + " minPatrolDistance: " + minPatrolDistance + " lockerSpawnChance: " + lockerSpawnChance + " lockPath: " + lockPath + " lockEndroom: " + lockEndroom);
		}
	}
}
