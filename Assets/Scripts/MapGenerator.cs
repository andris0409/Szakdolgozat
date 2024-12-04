using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Assets.Scripts.MapObjects;
using Assets.Scripts.Exithandlers;
using System.Diagnostics.Tracing;
using System.Diagnostics;

namespace Assets.Scripts
{
    public class MapGenerator : MonoBehaviour
    {
        public GameObject oneExitRoomPrefab;
        public GameObject oneExitCorridorPrefab;
        public GameObject straightTwoExitRoomPrefab;
        public GameObject LshapedtwoExitRoomPrefab;
        public GameObject straightTwoExitCorridorPrefab;
        public GameObject LshapedTwiExitCorridorPrefab;
        public GameObject threeExitRoomPrefab;
        public GameObject threeExitCorridorPrefab;
        public GameObject fourExitRoomPrefab;
        public GameObject fourExitCorridorPrefab;
        public GameObject doorPrefab;
        public GameObject lockerPrefab;
        public GameObject endRoomPrefab;
		public GameManager gameManager;
		public Terrain terrain;
        public GameObject navMeshBakerObject;
        public GameObject aiPrefab;
        private GameObject startRoomInstance;
        private GameObject endRoomInstance;
        private MapLogic mapLogic;
        private List<GameObject> generatedMapObjectInstances;
        private Dictionary<IMapObject, GameObject> mapObjectToInstance = new Dictionary<IMapObject, GameObject>();




        // Start is called before the first frame update
        void Start()
        {
            mapLogic = GetComponent<MapLogic>();
            mapLogic.onMapCreationComplete.AddListener(OnMapCreationComplete);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                UnityEngine.Debug.Log("G pressed, generating map");
                CleanMap();
                GenerateMap();
            }
        }

        // Method to be called when the map creation is complete
        private void OnMapCreationComplete()
        {
            GenerateMap();
            CreateAi();
        }

        public void GenerateMap()
        {
            Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			mapLogic = GetComponent<MapLogic>();
            generatedMapObjectInstances = new List<GameObject>();
            var startRoomPrefab = GetRoomPrefab(mapLogic.startRoom);
            startRoomInstance = Instantiate(startRoomPrefab, GetTerrainCenter(), startRoomPrefab.transform.rotation);
            generatedMapObjectInstances.Add(startRoomInstance);
            RotatePrefab(mapLogic.startRoom.ExitList, startRoomInstance);
            mapObjectToInstance.Add(mapLogic.startRoom, startRoomInstance);
            BfsMapObjects();
            Spawnkeys();
			stopwatch.Stop();
			UnityEngine.Debug.Log("Map generated in " + stopwatch.ElapsedMilliseconds + "ms");
		}
            
        private void Spawnkeys()
        {
            foreach (var room in mapLogic.roomList)
            {
                if (room.Keys.Count > 0)
                {
                    var roomInstance = GetMapObjectInstance(room);

                    Transform keySpawnPoint = roomInstance.transform.Find("KeySpawn");

                    if (keySpawnPoint == null)
                    {
                        UnityEngine.Debug.LogError("No 'keySpawn' GameObject found in the room: " + roomInstance.name);
                        continue;  
                    }

                    foreach (var key in room.Keys)
                    {
                        GameObject keyPrefab = Resources.Load<GameObject>("rust_key");  

                        Vector3 spawnPosition = keySpawnPoint.position;

                        var keyInstance=Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
                        keyInstance.GetComponent<KeyComponent>().keyName = key.Name;
                    }
                }
            }
        }

        private Vector3 GetKeySpawnLocation(GameObject room)
        {
           return room.transform.position;
        }

        private void CreateAi()
        {
            var navMeshBaker = navMeshBakerObject.GetComponent<NavMeshBaker>();
            navMeshBaker.BakeNavMesh();
            for (int i = 0; i < mapLogic.numberOfAI; i++)
            {
                IMapObject TheoreticalAIStartRoom = mapLogic.SelectAIStartRoom();
                Transform AIStartRoom = GetMapObjectInstance(TheoreticalAIStartRoom).transform;
                IMapObject FakePatrolRoom = mapLogic.SelectRandomRoomInDistance(TheoreticalAIStartRoom);
                Transform patrolRoom = GetMapObjectInstance(FakePatrolRoom).transform;
                GameObject ai = Instantiate(aiPrefab, AIStartRoom.position, Quaternion.identity);
                ai.GetComponent<AIMovement>().Initialize(AIStartRoom, patrolRoom);
            }
        }

        public void CleanMap()
        {
            foreach (var mapObjectInstance in generatedMapObjectInstances)
            {
                Destroy(mapObjectInstance);
            }
            generatedMapObjectInstances.Clear();
        }

        private void BfsMapObjects()
        {
            List<IMapObject> generatedMapObjects = new();
            var queue = new Queue<(IMapObject mapObject, GameObject gameObject)>();

            generatedMapObjects.Add(mapLogic.startRoom);
            queue.Enqueue((mapLogic.startRoom, startRoomInstance));

            while (queue.Count > 0)
            {
                var (currentMapObject, currentGameObject) = queue.Dequeue();

                foreach (var exitDirection in currentMapObject.ExitList)
                {
                    IMapObject nextMapObject = mapLogic.GetNeighbour(currentMapObject, exitDirection);
                    if (nextMapObject != null && !generatedMapObjects.Contains(nextMapObject))
                    {
                        GameObject nextMapObjectInstance = GenerateMapObject(nextMapObject);
                        generatedMapObjectInstances.Add(nextMapObjectInstance);

                        Exit oppositeExitDirection = GetOppositeDirection(exitDirection);
                        GameObject currentExit = null;
                        currentExit = currentGameObject.GetComponent<IExitHandler>().GetExit(exitDirection);
                        var connectingExit = nextMapObjectInstance.GetComponent<IExitHandler>().GetExit(oppositeExitDirection);

                        ConnectRooms(currentGameObject, nextMapObjectInstance, currentExit, connectingExit);
                        generatedMapObjects.Add(nextMapObject);
                        queue.Enqueue((nextMapObject, nextMapObjectInstance));

                    }
                }

                    foreach (var door in currentMapObject.Doors)
                    {
                        GameObject temp = doorPrefab;
                        GameObject doorInstance = Instantiate(temp, new Vector3(0, 0, 0), temp.transform.rotation);
                        PositionDoor(currentGameObject, doorInstance, door.ExitLocation);
                        if(door.Name.Equals("endroomdoor"))
                        {
                            doorInstance.GetComponent<DoorComponent>().RequiredKey = "endroomkey";
                        }
                        else if(door.Name.Equals("pathdoor"))
                        {
                            doorInstance.GetComponent<DoorComponent>().RequiredKey = "pathkey";
                        }
                        else
                        {
                           UnityEngine.Debug.LogError("Door name not found");
                        }
                    }
                    if(currentMapObject is Room room)
                {
                    foreach (var locker in room.Lockers)
                    {
                        GameObject temp = lockerPrefab;
                        GameObject lockerInstance = Instantiate(temp, new Vector3(0, 0, 0), temp.transform.rotation);
                        lockerInstance.GetComponent<LockerComponent>().InitializeLocker(locker);
                        PositionLocker(currentGameObject, lockerInstance, locker);
                    }
                }
                
            }
        }

        private void PositionLocker(GameObject room, GameObject lockerInstance, Locker locker)
        {
            var index = locker.Place;
            var lockerPosition = room.transform.Find("LockerPlace" + index);
            lockerInstance.transform.position = lockerPosition.transform.position;
            lockerInstance.transform.SetParent(room.transform);
            lockerInstance.transform.localScale = new Vector3(1.3f, 1.2f, 1.6f);
            if (index < 2)
            {
                lockerInstance.transform.Rotate(0, 180, 0, Space.World);
            }
        }

        // Helper method to get the opposite direction of an exit
        private Exit GetOppositeDirection(Exit exitDirection)
        {
            switch (exitDirection)
            {
                case Exit.NORTH: return Exit.SOUTH;
                case Exit.SOUTH: return Exit.NORTH;
                case Exit.EAST: return Exit.WEST;
                case Exit.WEST: return Exit.EAST;
                default: return exitDirection; // Default case if no opposite is defined
            }
        }
        private GameObject GenerateMapObject(IMapObject mapObject)
        {
            GameObject temp = null;
            if (mapObject == null)
                return null;
            else if (mapObject is Room r)
            {
                temp = GenerateRoom(r);
            }
            else if (mapObject is Corridor c)
            {
                temp = GenerateCorridor(c);
            }
            return temp;
        }
        private GameObject GenerateRoom(Room room)
        {
            if (room.ObjectType.Equals(MapObjectType.EndRoom))
			{
                endRoomInstance = Instantiate(endRoomPrefab, new Vector3(0, 0, 0), endRoomPrefab.transform.rotation);
                RotatePrefab(room.ExitList, endRoomInstance);
                mapObjectToInstance.Add(room, endRoomInstance);
				EndRoomTrigger trigger = endRoomInstance.GetComponent<EndRoomTrigger>();
				if (trigger != null)
				{
					trigger.gameManager = gameManager;  
				}
                else
				{
					UnityEngine.Debug.LogError("EndRoomTrigger not found in endRoomInstance");
				}
				return endRoomInstance;
			}
			GameObject roomPrefab = GetRoomPrefab(room);
            GameObject roomInstance = Instantiate(roomPrefab, new Vector3(0, 0, 0), roomPrefab.transform.rotation);
            RotatePrefab(room.ExitList, roomInstance);
            mapObjectToInstance.Add(room, roomInstance);
            return roomInstance;
        }
        private GameObject GenerateCorridor(Corridor corridor)
        {
            GameObject corridorPrefab = GetCorridorPrefab(corridor);
            GameObject corridorInstance = Instantiate(corridorPrefab, new Vector3(0, 0, 0), corridorPrefab.transform.rotation);
            RotatePrefab(corridor.ExitList, corridorInstance);
            mapObjectToInstance.Add(corridor, corridorInstance);
            return corridorInstance;
        }

        public GameObject GetMapObjectInstance(IMapObject mapObject)
        {
            if (mapObjectToInstance.ContainsKey(mapObject))
            {
                return mapObjectToInstance[mapObject];
            }
            throw new KeyNotFoundException("Map object not found in mapObjectToInstance dictionary.");
        }

        private GameObject GetRoomPrefab(Room r)
        {
            switch (r.ExitList.Count)
            {
                case 1:
                    return oneExitRoomPrefab;
                case 2:
                    if (r.ExitList.Contains(Exit.NORTH) && r.ExitList.Contains(Exit.SOUTH) || r.ExitList.Contains(Exit.EAST) && r.ExitList.Contains(Exit.WEST))
                    {
                        return straightTwoExitRoomPrefab;
                    }
                    else
                    {
                        return LshapedtwoExitRoomPrefab;
                    }
                case 3:
                    return threeExitRoomPrefab;
                case 4:
                    return fourExitRoomPrefab;
                default:
                    return null;
            }
        }
        private GameObject GetCorridorPrefab(Corridor c)
        {
            switch (c.ExitList.Count)
            {
                case 1:
                    return oneExitCorridorPrefab;
                case 2:
                    if (c.ExitList.Contains(Exit.NORTH) && c.ExitList.Contains(Exit.SOUTH) || c.ExitList.Contains(Exit.EAST) && c.ExitList.Contains(Exit.WEST))
                    {
                        return straightTwoExitCorridorPrefab;
                    }
                    else
                    {
                        return LshapedTwiExitCorridorPrefab;
                    }
                case 3:
                    return threeExitCorridorPrefab;
                case 4:
                    return fourExitCorridorPrefab;
                default:
                    return null;
            }
        }
        private void RotatePrefab(List<Exit> exitList, GameObject gameObject)
        {
            if (exitList.Count == 1)
            {
                switch (exitList[0])
                {
                    case Exit.WEST:
                        gameObject.transform.Rotate(0, 90, 0, Space.World);
                        break;
                    case Exit.NORTH:
                        gameObject.transform.Rotate(0, 180, 0, Space.World);
                        break;
                    case Exit.EAST:
                        gameObject.transform.Rotate(0, 270, 0, Space.World);
                        break;
                    default:
                        break;
                }
            }
            else if (exitList.Count == 2)
            {
                if (exitList.Contains(Exit.EAST) && exitList.Contains(Exit.WEST))
                {
                    gameObject.transform.Rotate(0, 90, 0);
                }
                else if (exitList.Contains(Exit.EAST) && exitList.Contains(Exit.SOUTH))
                {
                    gameObject.transform.Rotate(0, 90, 0);
                }
                else if (exitList.Contains(Exit.SOUTH) && exitList.Contains(Exit.WEST))
                {
                    gameObject.transform.Rotate(0, 180, 0);
                }
                else if (exitList.Contains(Exit.NORTH) && exitList.Contains(Exit.WEST))
                {
                    gameObject.transform.Rotate(0, 270, 0);
                }
            }
            else if (exitList.Count == 3)
            {
                if (!exitList.Contains(Exit.EAST))
                {
                    gameObject.transform.Rotate(0, 90, 0);
                }
                else if (!exitList.Contains(Exit.SOUTH))
                {
                    gameObject.transform.Rotate(0, 180, 0);
                }
                else if (!exitList.Contains(Exit.WEST))
                {
                    gameObject.transform.Rotate(0, 270, 0);
                }
            }
        }
        private Vector3 GetTerrainCenter()
        {
            return new Vector3(terrain.terrainData.size.x / 2, 0, terrain.terrainData.size.z / 2);
        }

        public void ConnectRooms(GameObject room1, GameObject room2, GameObject room1ConnectionPoint, GameObject room2ConnectionPoint)
        {
            if (room1 == null || room2 == null)
            {
                UnityEngine.Debug.LogError("Rooms  are missing.");
                return;
            }
            if (room1ConnectionPoint == null || room2ConnectionPoint == null)
            {
                UnityEngine.Debug.LogError("Connection points are missing.");
                return;
            }

            // Calculate the world position of room2's connection point
            Vector3 room2ConnectionWorldPosition = room2.transform.TransformPoint(room2ConnectionPoint.transform.localPosition);

            // Calculate the vector from room2's connection point to room1's connection point
            Vector3 connectionOffset = room1ConnectionPoint.transform.position - room2ConnectionWorldPosition;

            // Move room2 by the connection offset so that the connection points are aligned
            room2.transform.position += connectionOffset;
        }

        public void PositionDoor(GameObject room, GameObject door, Exit exit)
        {
            if (room == null || door == null)
            {
                UnityEngine.Debug.LogError("Room or door is missing.");
                return;
            }

            GameObject exitPoint = room.GetComponent<IExitHandler>().GetExit(exit);

            if (exitPoint == null)
            {
                UnityEngine.Debug.LogError("Exit point is missing.");
                return;
            }
            door.transform.position = exitPoint.transform.position;
            if (exit == Exit.NORTH || exit == Exit.SOUTH)
            {
                door.transform.Rotate(0, 90, 0, Space.World);
                door.transform.position = new Vector3(
                   door.transform.position.x + 0.25f,
                   door.transform.position.y,
                   door.transform.position.z);
            }
            else
            {
                door.transform.position = new Vector3(
                    door.transform.position.x,
                    door.transform.position.y,
                    door.transform.position.z + 0.3f);
            }

        }
    }
}
