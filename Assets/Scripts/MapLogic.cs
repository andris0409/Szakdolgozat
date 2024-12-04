using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Assets.Scripts.Exithandlers;
using Assets.Scripts.MapObjects;

namespace Assets.Scripts
{
    public enum SpawnExit
    {
        Never,
        Random,
        Always,
    }
    public class MapLogic : MonoBehaviour
    {

        IMapObject[,] grid;
        public int seed;
        public bool randomSeed = true;
        public int width;
        public int height;
        public int roomAmount;
        public int EndRoomDistance;
        public int KeyDistance;
        public int MinAIPatrolDistance;
        public int numberOfAI;
        public int startroomexits;
        public int LockerChance;
        public bool LockEndRoomDoor;
        public bool LockPath;

        public bool randomStartRoomPosition = true;
        public bool randomAIStartPosition = true;
        public Room startRoom;
        public Room endRoom;
        private int placedkeys = 0;
        private List<IMapObject> path;
        public List<Room> roomList;
        public List<Corridor> corridorList;
        public List<Room> visitedRooms;
        public UnityEvent onMapCreationComplete;

        // Start is called before the first frame update
        void Start()
        {
            LoadParameters();
			if (randomSeed)
            {
                long ticks = DateTime.Now.Ticks;
                seed = (int)(ticks % int.MaxValue);
            }
            Debug.Log("Seed: " + CreateMap(seed));
            onMapCreationComplete?.Invoke();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (randomSeed)
                {
                    long ticks = DateTime.Now.Ticks;
                    seed = (int)(ticks % int.MaxValue);
                }
                Debug.Log("Seed: " + CreateMap(seed));
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("B pressed, building graph");
                path = BfsBuildGraphWithPath();
                if (path == null)
                {
                    Debug.LogError("No path");
                }
                else
                {
                    Debug.Log("Path found");
                }
            }
        }

		/// <summary>
		/// Loads the map generation parameters from the PlayerPrefs.
		/// </summary>
		private void LoadParameters()
        {
			width = PlayerPrefs.GetInt("Width", 10);
			height = PlayerPrefs.GetInt("Height", 10);
			roomAmount = PlayerPrefs.GetInt("RoomAmount", 10);
			startroomexits = PlayerPrefs.GetInt("StartRoomExits", 4);
			EndRoomDistance = PlayerPrefs.GetInt("EndRoomDistance", 5);
			KeyDistance = PlayerPrefs.GetInt("KeyDistance", 3);
			MinAIPatrolDistance = PlayerPrefs.GetInt("MinPatrolDistance", 5);
			numberOfAI = PlayerPrefs.GetInt("NumberOfGuards", 2);
			LockerChance = PlayerPrefs.GetInt("LockerChance", 5);
			LockPath = PlayerPrefs.GetInt("LockPath", LockPath ? 1 : 0) == 1;
			LockEndRoomDoor = PlayerPrefs.GetInt("LockEndRoomDoor", LockEndRoomDoor ? 1 : 0) == 1;
            Debug.Log("Settings loaded: Width=" + width + ", Height=" + height + ", RoomAmount=" + roomAmount + ", StartRoomExits=" + startroomexits + ", EndRoomDistance=" + EndRoomDistance + ", KeyDistance=" + KeyDistance + ", MinAIPatrolDistance=" + MinAIPatrolDistance + ", NumberOfAI=" + numberOfAI + ", LockerChance=" + LockerChance + ", LockPath=" + LockPath + ", LockEndRoomDoor=" + LockEndRoomDoor);
		}

        /// <summary>
        /// Initializes and generates the game map by seeding randomness, 
        /// creating a grid, and methodically placing start/end rooms, 
        /// additional rooms, and corridors.
        /// </summary>
        public int CreateMap(int s)
        {
            SetupMap(s);
            var counter = 0;
            while (counter < 200)
            {
                if (PlaceCorrdiorsBetweenRooms())
                {
                    break;
                }
                else
                {
                    long ticks = DateTime.Now.Ticks;
                    s = (int)(ticks % int.MaxValue);
                    SetupMap(s);
                    counter++;
                }
            }
            if (counter == 200)
            {
                Debug.LogError("Couldn't create map, try again");
            }
            CorrectRoomConnections();
            RepairDeadEnds();
            path = BfsBuildGraphWithPath();
            if (LockPath)
            {
                PlaceRandomDoorWithKey();
            }
            if (LockEndRoomDoor)
            {
                LockEndRoom();
                if (!PlaceKeyForEndroom(KeyDistance))
                {
                    Debug.LogError("Couldn't place key for endroom, try with smaller minimum distance");
                }
            }
            PlaceLockers();
            return s;
        }
        private void SetupMap(int s)
        {
            UnityEngine.Random.InitState(s);
            grid = new IMapObject[width, height];
            roomList = new List<Room>();
            corridorList = new List<Corridor>();
            PlaceStartRoom();
            PlaceEndRoom();
            PlaceRooms();
        }

        /// <summary>
        /// Iterates through all rooms and places corridors between them to ensure a navigable layout.
        /// </summary>
        public bool PlaceCorrdiorsBetweenRooms()
        {
            visitedRooms = new List<Room>();
            corridorList = new List<Corridor>();
            Room currentRoom = startRoom;
            visitedRooms.Add(currentRoom);
            while (visitedRooms.Count < roomList.Count)
            {
                Room nextRoom = (Room)FindNearestObject(currentRoom);
                int[] positions = ConnectRooms(currentRoom, nextRoom);
                if (positions[0] != -1)
                {
                    var verticalpath = FindPathBetweenRooms(positions[0], positions[1], positions[2], positions[3], true);
                    var horizontalpath = FindPathBetweenRooms(positions[0], positions[1], positions[2], positions[3], false);
                    var counter = 0;
                    while ((verticalpath == null || horizontalpath == null) && counter < 5)
                    {
                        positions = ConnectRooms(currentRoom, nextRoom); //try different exits of the same rooms
                        if (positions[0] == -1)
                        {
                            break;
                        }
                        verticalpath = FindPathBetweenRooms(positions[0], positions[1], positions[2], positions[3], false);
                        horizontalpath = FindPathBetweenRooms(positions[0], positions[1], positions[2], positions[3], true);
                        counter++;
                    }

                    if (verticalpath == null || horizontalpath == null)  //try different rooms
                    {
                        foreach (var room in visitedRooms)
                        {
                            currentRoom = room;
                            counter = 0;
                            while ((verticalpath == null || horizontalpath == null) && counter < 5)
                            {
                                positions = ConnectRooms(currentRoom, nextRoom);
                                if (positions[0] == -1)
                                {
                                    break;
                                }
                                verticalpath = FindPathBetweenRooms(positions[0], positions[1], positions[2], positions[3], false);
                                horizontalpath = FindPathBetweenRooms(positions[0], positions[1], positions[2], positions[3], true);
                                counter++;
                            }
                            if (verticalpath != null && horizontalpath != null)
                            {
                                break;
                            }
                        }
                    }
                    if (verticalpath != null && horizontalpath != null)
                    {
                        BuildPath(verticalpath); 
                        BuildPath(horizontalpath);
                        visitedRooms.Add(nextRoom);
                        currentRoom = nextRoom;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    visitedRooms.Add(nextRoom);
                    currentRoom = nextRoom;
                }
            }
            return true;
        }

        /// <summary>
        /// Attempts to place an IMapObject on the map grid at the specified coordinates.
        /// </summary>
        /// <param name="obj"> The IMapObject to be placed on the grid. It must have X and Y properties set to the desired coordinates. </param>
        /// <returns>Returns true if the object was successfully placed (the target grid cell was empty). Returns false if the cell was already occupied.</returns>
        private void PlaceObject(IMapObject obj)
        {
            if (grid[obj.X, obj.Y] == null)
            {
                grid[obj.X, obj.Y] = obj;
                if (obj is Room room)
                    roomList.Add(room);
                else if (obj is Corridor corridor)
                    corridorList.Add(corridor);

            }
            else
            {
                throw new ArgumentException("Can't place object on an occupied space");
            }
        }

        /// <summary>
        /// Attempts to remove an IMapObject from the map grid at the specified coordinates.
        /// </summary>
        /// <param name="x"> x grid coordinate </param>
        /// <param name="y"> y grid coordinate </param>
        /// <returns> Returns true if an object was removed. Returns false if the cell was already empty </returns>
        private void DeleteObject(int x, int y)
        {
            if (grid[x, y] == null)
            {
                throw new ArgumentException("Can't delete object from an empty space");
            }
            else
            {
                if (grid[x, y] is Room)
                    roomList.Remove((Room)grid[x, y]);
                else if (grid[x, y] is Corridor)
                    corridorList.Remove((Corridor)grid[x, y]);
                grid[x, y] = null;
            }
        }


        private IMapObject GetObjectAt(int x, int y)
        {
            return grid[x, y];
        }

        /// <summary>
        /// Places the start room on the map with a configurable number of exits. 
        /// The position can be random or fixed based on the configuration.
        /// </summary>
        /// <exception cref="ArgumentException"> Throws exception if the amount of startroom's exit is not between 1-4 </exception>
        private void PlaceStartRoom()
        {
            if (startroomexits > 4 || startroomexits < 1)
            {
                throw new ArgumentException("Invalid number of exits for start");
            }
            else
            {
                if (randomStartRoomPosition)
                {
                    SpawnExit[] exitlist = new[] { SpawnExit.Random, SpawnExit.Random, SpawnExit.Random, SpawnExit.Random };
                    startRoom = CreateRandomRoom(UnityEngine.Random.Range(1, width - 1), UnityEngine.Random.Range(1, height - 1), exitlist, MapObjectType.StartRoom);
                }
                else
                {
                    SpawnExit[] exitlist = new[] { SpawnExit.Random, SpawnExit.Random, SpawnExit.Never, SpawnExit.Random };
                    startRoom = CreateRandomRoom(UnityEngine.Random.Range(1, width - 1), 0, exitlist, MapObjectType.StartRoom);
                }
                PlaceObject(startRoom);
            }
        }

        /// <summary>
        /// Places the end room on the map. The position is random and the distance from the start room is at least the width of the map.
        /// </summary>
        /// <exception cref="ArgumentException"> Throws an exception if the startroom doesn't exist </exception>
        private void PlaceEndRoom()
        {
            if (startRoom == null)
            {
                throw new ArgumentException("Start room not placed yet");
            }
            else
            {
                int x = 0;
                int y = 0;
                List<SpawnExit> neighbours;
                do
                {
                    x = UnityEngine.Random.Range(0, width - 1);
                    y = UnityEngine.Random.Range(0, height - 1);
                    while (grid[x, y] != null)
                    {
                        x = UnityEngine.Random.Range(0, width - 1);
                        y = UnityEngine.Random.Range(0, height - 1);
                    }
                    neighbours = OccupiedNeighobours(x, y);
                } while (!(neighbours.Contains(SpawnExit.Random) || neighbours.Contains(SpawnExit.Always)));
                Room temp = CreateRandomRoom(x, y, neighbours.ToArray(), MapObjectType.EndRoom);
                int i = 0;
                while (Distance(temp, startRoom) < EndRoomDistance)
                {
                    i++;
                    if (i == 100)
                    {
                        throw new ArgumentException("Can't place end room try with smaller distance");
                    }
                    x = UnityEngine.Random.Range(0, width - 1);
                    y = UnityEngine.Random.Range(0, height - 1);
                    neighbours = OccupiedNeighobours(x, y);
                    temp = CreateRandomRoom(x, y, neighbours.ToArray(), MapObjectType.EndRoom);
                }
                endRoom = temp;
                PlaceObject(temp);
            }
        }

        /// <summary>
        /// PLaces a configurable number of rooms on the map. The positions are random
        /// </summary>
        private void PlaceRooms()
        {
            int placedRooms = 0;
            while (placedRooms < roomAmount)
            {
                int x = UnityEngine.Random.Range(0, width - 1);
                int y = UnityEngine.Random.Range(0, height - 1);
                if (grid[x, y] == null)
                {
                    List<SpawnExit> neighbours = OccupiedNeighobours(x, y);
                    if (neighbours.Contains(SpawnExit.Random) || neighbours.Contains(SpawnExit.Always))
                    {
                        Room temp = CreateRandomRoom(x, y, neighbours.ToArray(), MapObjectType.CasualRoom);
                        if (temp.ExitList.Count == 1)
                        {
                            var counter = 0;
                            while (!CheckOneExitRoomReachable(temp) && counter < 10)
                            {
                                temp = CreateRandomRoom(x, y, neighbours.ToArray(), MapObjectType.CasualRoom);
                                counter++;
                            }
                        }
                        PlaceObject(temp);
                        placedRooms++;
                    }
                }
            }
        }

        private bool CheckOneExitRoomReachable(Room room)
        {
            if (room.ExitList.Count != 1)
            {
                return true;
            }
            switch (room.ExitList[0])
            {
                case Exit.NORTH:
                    var tempRoom = GetObjectAt(room.X, room.Y + 1);
                    if (tempRoom == null)
                        return true;
                    if (tempRoom.ExitList.Count == 1)
                        return false;
                    break;
                case Exit.EAST:
                    tempRoom = GetObjectAt(room.X + 1, room.Y);
                    if (tempRoom == null)
                        return true;
                    if (tempRoom.ExitList.Count == 1)
                        return false;
                    break;
                case Exit.SOUTH:
                    tempRoom = GetObjectAt(room.X, room.Y - 1);
                    if (tempRoom == null)
                        return true;
                    if (tempRoom.ExitList.Count == 1)
                        return false;
                    break;
                case Exit.WEST:
                    tempRoom = GetObjectAt(room.X - 1, room.Y);
                    if (tempRoom == null)
                        return true;
                    if (tempRoom.ExitList.Count == 1)
                        return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Calculates the Manhattan distance between two IMapObjects
        /// </summary>
        /// <param name="mapObject1"></param>
        /// <param name="mapObject2"></param>
        /// <returns> The sum of distance on the 2 axis </returns>
        private int Distance(IMapObject mapObject1, IMapObject mapObject2)
        {
            int x = Mathf.Abs(mapObject1.X - mapObject2.X);
            int y = Mathf.Abs(mapObject1.Y - mapObject2.Y);
            return x + y;
        }

        /// <summary>
        /// Creates a room with a configurable number of exits, based on the spawnExits array. The method ensures the room has at least one exit. 
        /// Specific logic is applied for start and end rooms to meet their unique exit requirements.
        /// </summary>
        /// <param name="x">The x-coordinate on the grid where the room will be placed.</param>
        /// <param name="y">The y-coordinate on the grid where the room will be placed.</param>
        /// <param name="spawnExits">An array indicating the desired state (Never, Random, Always) for each potential exit direction (North, East, South, West).</param>
        /// <param name="roomType">The type of room to create (e.g., StartRoom, EndRoom), affecting exit logic.</param>
        /// <returns>Returns a Room object with configured exits and type, placed at the specified grid coordinates.</returns>
        /// <exception cref="ArgumentException">Throws an exception if it's impossible to create a room with the specified exits conditions, particularly if no exits are defined for the room.</exception>

        private Room CreateRandomRoom(int x, int y, SpawnExit[] spawnExits, MapObjectType roomType)
        {
            List<Exit> finalExits = new List<Exit>();

            for (int i = 0; i < spawnExits.Length; i++)
            {
                switch (spawnExits[i])
                {
                    case SpawnExit.Never:
                        // Do nothing
                        break;
                    case SpawnExit.Random:
                        if (roomType == MapObjectType.EndRoom)
                        {
                            break;
                        }
                        // Randomly decide to add this exit
                        if (UnityEngine.Random.Range(1, 5) < 4)
                        {
                            finalExits.Add((Exit)i);
                        }
                        break;
                    case SpawnExit.Always:
                        finalExits.Add((Exit)i);
                        break;
                }
            }
            if (finalExits.Count == 0)
            {
                for (int i = 0; i < spawnExits.Length; i++)
                {
                    if (spawnExits[i] == SpawnExit.Random)
                    {
                        finalExits.Add((Exit)i);
                        break;
                    }
                }
            }

            if (roomType == MapObjectType.StartRoom)
            {
                int i = 0;
                while (finalExits.Count < startroomexits && i < 4)
                {
                    if (spawnExits[i] == SpawnExit.Random && !finalExits.Contains((Exit)i))
                    {
                        finalExits.Add((Exit)i);
                    }
                    i++;
                }
            }

            if (roomType == MapObjectType.EndRoom)
            {
                while (finalExits.Count > 1)
                {
                    finalExits.RemoveAt(UnityEngine.Random.Range(0, finalExits.Count));
                }
            }

            if (finalExits.Count == 0)
            {
                Debug.Log("Seed: " + seed);
                throw new ArgumentException("No exits for room");
            }
            var room = new Room(x, y, finalExits) { ObjectType = roomType };
            return room;
        }

        /// <summary>
        /// Evaluates the occupancy and exit possibilities of neighboring grid positions relative to a specified location, 
        /// determining potential exit directions for room placement.
        /// </summary>
        /// <param name="x">The x-coordinate of the grid position being assessed.</param>
        /// <param name="y">The y-coordinate of the grid position being assessed.</param>
        /// <returns>A list of SpawnExit values indicating the potential exit states (Never, Random, Always) for each direction based on the occupancy and type of neighboring grid positions.</returns>

        private List<SpawnExit> OccupiedNeighobours(int x, int y)
        {
            List<SpawnExit> neighbours = new();
            if (y == height - 1)
                neighbours.Add(SpawnExit.Never);
            else
            {
                if (grid[x, y + 1] != null)
                {
                    if (grid[x, y + 1].ObjectType != MapObjectType.Corridor)
                    {
                        Room temp = (Room)grid[x, y + 1];
                        if (!temp.ExitList.Contains(Exit.SOUTH))
                            neighbours.Add(SpawnExit.Never);

                        else
                            neighbours.Add(SpawnExit.Always);
                    }
                }
                else
                    neighbours.Add(SpawnExit.Random);
            }
            if (x == 0)
                neighbours.Add(SpawnExit.Never);
            else
            {
                if (grid[x - 1, y] != null)
                {
                    if (grid[x - 1, y].ObjectType != MapObjectType.Corridor)
                    {
                        Room temp = (Room)grid[x - 1, y];
                        if (!temp.ExitList.Contains(Exit.EAST))
                            neighbours.Add(SpawnExit.Never);
                        else
                            neighbours.Add(SpawnExit.Always);
                    }
                }
                else
                    neighbours.Add(SpawnExit.Random);
            }
            if (y == 0)
                neighbours.Add(SpawnExit.Never);
            else
            {
                if (grid[x, y - 1] != null)
                {
                    if (grid[x, y - 1].ObjectType != MapObjectType.Corridor)
                    {
                        Room temp = (Room)grid[x, y - 1];
                        if (!temp.ExitList.Contains(Exit.NORTH))
                            neighbours.Add(SpawnExit.Never);
                        else
                            neighbours.Add(SpawnExit.Always);
                    }
                }
                else
                    neighbours.Add(SpawnExit.Random);
            }
            if (x == width - 1)
                neighbours.Add(SpawnExit.Never);
            else
            {
                if (grid[x + 1, y] != null)
                {
                    if (grid[x + 1, y].ObjectType != MapObjectType.Corridor)
                    {
                        Room temp = (Room)grid[x + 1, y];
                        if (!temp.ExitList.Contains(Exit.WEST))
                            neighbours.Add(SpawnExit.Never);
                        else
                            neighbours.Add(SpawnExit.Always);
                    }
                }
                else
                    neighbours.Add(SpawnExit.Random);
            }
            return neighbours;
        }

        /// <summary>
        /// Finds the nearest IMapObject to the specified mapObject, excluding objects that have already been visited.
        /// </summary>
        /// <param name="mapObject"> The reference map object from which the search for the nearest unvisited object begins. </param>
        /// <returns> Nearest ImapObject that is not visited yet </returns>
        private IMapObject FindNearestObject(IMapObject mapObject)
        {
            IMapObject nearestObject = null;
            int nearestDistance = int.MaxValue;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null && grid[x, y].ObjectType != MapObjectType.Corridor)
                    {
                        int distance = Distance(mapObject, grid[x, y]);
                        if (distance < nearestDistance && !visitedRooms.Contains(grid[x, y]))
                        {
                            nearestDistance = distance;
                            nearestObject = grid[x, y];
                        }
                    }
                }
            }
            return nearestObject;
        }

        /// <summary>
        /// Creates a path of corridors between two rooms, either vertically or horizontally, based on the specified coordinates and direction. 
        /// This method ensures the integration of new corridors into the existing map layout, adjusting for overlaps with other corridors or rooms.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first room.</param>
        /// <param name="y1">The y-coordinate of the first room.</param>
        /// <param name="x2">The x-coordinate of the second room. This may be the same as x1 if the path is vertical.</param>
        /// <param name="y2">The y-coordinate of the second room. This may be the same as y1 if the path is horizontal.</param>
        /// <param name="vertical">Determines the direction of the path. If true, the path will be vertical (up or down); if false, the path will be horizontal (left or right).</param>

        private void BuildPath(List<IMapObject> path)
        {
            foreach (IMapObject obj in path)
            {
                if (obj is Corridor)
                {
                    PlaceOrReplace(obj);
                }
            }
        }
        private List<IMapObject> FindPathBetweenRooms(int x1, int y1, int x2, int y2, bool horizontal)
        {
            List<IMapObject> path = new();

            if (horizontal)
            {
                int startX;
                int endX;
                int y = y1;
                if (x1 < x2)
                {
                    startX = x1;
                    endX = x2;
                }
                else
                {
                    startX = x2;
                    endX = x1;
                }
                if (GetObjectAt(endX, y) is Room room)
                {
                    if (room.ExitList.Count == 1)
                    {
                        return null;
                    }
                    else
                    {
                        if (room.ExitList.Contains(Exit.WEST))
                        {
                            endX--;
                        }
                        else if (!room.ExitList.Contains(Exit.EAST))
                        {
                            return null;
                        }
                    }
                }
                if (GetObjectAt(startX, y) is Room room1)
                {
                    if (room1.ExitList.Count == 1)
                    {
                        return null;
                    }
                }
                for (int x = startX; x < endX + 1; x++)
                {
                    Corridor temp;
                    if (x == endX)
                    {
                        if (x == 0)
                            temp = new Corridor(x, y, new List<Exit> { Exit.EAST });
                        else
                            temp = new Corridor(x, y, new List<Exit> { Exit.WEST, });
                    }
                    else if (x == startX)
                    {
                        if (x == width - 1)
                            temp = new Corridor(x, y, new List<Exit> { Exit.WEST });
                        else
                            temp = new Corridor(x, y, new List<Exit> { Exit.EAST });
                    }
                    else
                    {
                        temp = new Corridor(x, y, new List<Exit> { Exit.EAST, Exit.WEST });
                    }
                    if (grid[x, y] is Room tempRoom)
                    {
                        var tempPath = IsAvoidable(tempRoom, !horizontal);
                        if (tempPath != null)
                        {
                            foreach (var obj in tempPath)
                            {
                                path.Add(obj);
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        path.Add(temp);
                    }
                }
                return path;
            }
            else
            {
                int startY;
                int endY;
                int x = x2;
                if (y1 < y2)
                {
                    startY = y1;
                    endY = y2;
                }
                else
                {
                    startY = y2;
                    endY = y1;
                }
                if (GetObjectAt(x, endY) is Room room)
                {
                    if (room.ExitList.Count == 1)
                    {
                        return null;
                    }
                    if (room.ExitList.Contains(Exit.SOUTH))
                    {
                        endY--;
                        if (GetObjectAt(x, endY) is Room room2)
                        {
                            if (room2.ExitList.Count == 1)
                            {
                                return null;
                            }
                        }
                    }
                    else if (!room.ExitList.Contains(Exit.SOUTH))
                    {
                        return null;
                    }
                }
                if (GetObjectAt(x, startY) is Room room1)
                {
                    if (room1.ExitList.Count == 1)
                    {
                        return null;
                    }
                    if (room1.ExitList.Contains(Exit.NORTH))
                    {
                        startY++;
                        if (GetObjectAt(x, startY) is Room room2)
                        {
                            if (room2.ExitList.Count == 1)
                            {
                                return null;
                            }
                        }
                    }
                    else if (!room1.ExitList.Contains(Exit.SOUTH))
                    {
                        return null;
                    }
                }
                for (int y = startY; y < endY + 1; y++)
                {
                    Corridor temp;
                    if (y == endY)
                    {
                        if (y == 0)
                            temp = new Corridor(x, y, new List<Exit> { Exit.NORTH });
                        else
                            temp = new Corridor(x, y, new List<Exit> { Exit.SOUTH });
                    }
                    else if (y == startY)
                    {
                        if (y == height - 1)
                            temp = new Corridor(x, y, new List<Exit> { Exit.SOUTH });
                        else
                            temp = new Corridor(x, y, new List<Exit> { Exit.NORTH });
                    }
                    else
                    {
                        temp = new Corridor(x, y, new List<Exit> { Exit.NORTH, Exit.SOUTH });
                    }
                    if (grid[x, y] is Room tempRoom)
                    {

                        var tempPath = IsAvoidable(tempRoom, !horizontal);
                        if (tempPath != null)
                        {
                            foreach (var obj in tempPath)
                            {
                                path.Add(obj);
                            }
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else
                    {
                        path.Add(temp);
                    }
                }
                return path;
            }
        }

        /// <summary>
        /// Adjusts the exits of a corridor at a specified location if there's an overlap with existing corridors, ensuring logical connectivity and continuity of the map layout.
        /// This method first checks for and handles any existing corridor object at the given coordinates, then recalculates exits based on neighboring corridors and rooms.
        /// Only handles the case when the object is a corridor
        /// </summary>
        /// <param name="x">The x-coordinate of the overlap location.</param>
        /// <param name="y">The y-coordinate of the overlap location.</param>
        /// <param name="c">The corridor object that potentially overlaps with existing map objects.</param>

        private void Correctoverlapping(int x, int y, IMapObject c)
        {
            Corridor corridor = (Corridor)c;
            List<Exit> exitlist = new();
            if (grid[x, y] != null)
            {
                if (grid[x, y].ObjectType == MapObjectType.Corridor)
                {
                    foreach (Exit e in grid[x, y].ExitList)
                    {
                        exitlist.Add(e);
                    }
                    DeleteObject(x, y);
                }
            }
            if (corridor.ExitList.Contains(Exit.NORTH) && !exitlist.Contains(Exit.NORTH))
            {
                exitlist.Add(Exit.NORTH);
            }
            if (corridor.ExitList.Contains(Exit.EAST) && !exitlist.Contains(Exit.EAST))
            {
                exitlist.Add(Exit.EAST);
            }
            if (corridor.ExitList.Contains(Exit.SOUTH) && !exitlist.Contains(Exit.SOUTH))
            {
                exitlist.Add(Exit.SOUTH);
            }
            if (corridor.ExitList.Contains(Exit.WEST) && !exitlist.Contains(Exit.WEST))
            {
                exitlist.Add(Exit.WEST);
            }
            Corridor temp = new(x, y, exitlist);
            PlaceOrReplace(temp);
        }

        /// <summary>
        /// Repairs dead ends in the map by modifying the exit lists of corridors 
        /// that lead to nowhere or do not connect properly with adjacent map objects.
        /// </summary>
        private void RepairDeadEnds()
        {
            List<IMapObject> corridorsToAdd = new List<IMapObject>();
            List<IMapObject> corridorsToRemove = new List<IMapObject>();
            foreach (IMapObject obj in corridorList)
            {
                int x = obj.X;
                int y = obj.Y;
                List<Exit> exitlist = new List<Exit>(obj.ExitList);
                foreach (Exit e in obj.ExitList)
                {
                    switch (e)
                    {
                        case Exit.NORTH:
                            if (grid[x, y + 1] == null)
                                exitlist.Remove(Exit.NORTH);
                            else
                            {
                                if (!grid[x, y + 1].ExitList.Contains(Exit.SOUTH))
                                    exitlist.Remove(Exit.NORTH);
                            }
                            break;
                        case Exit.SOUTH:
                            if (grid[x, y - 1] == null)
                                exitlist.Remove(Exit.SOUTH);
                            else
                            {
                                if (!grid[x, y - 1].ExitList.Contains(Exit.NORTH))
                                    exitlist.Remove(Exit.SOUTH);
                            }
                            break;
                        case Exit.EAST:
                            if (grid[x + 1, y] == null)
                                exitlist.Remove(Exit.EAST);
                            else
                            {
                                if (!grid[x + 1, y].ExitList.Contains(Exit.WEST))
                                    exitlist.Remove(Exit.EAST);
                            }
                            break;
                        case Exit.WEST:
                            if (grid[x - 1, y] == null)
                                exitlist.Remove(Exit.WEST);
                            else
                            {
                                if (!grid[x - 1, y].ExitList.Contains(Exit.EAST))
                                    exitlist.Remove(Exit.WEST);
                            }
                            break;
                    }
                }
                if (exitlist.Count != obj.ExitList.Count)
                {
                    corridorsToRemove.Add(obj);
                    corridorsToAdd.Add(new Corridor(x, y, exitlist));
                }
            }
            foreach (IMapObject obj in corridorsToRemove)
            {
                DeleteObject(obj.X, obj.Y);
            }
            foreach (IMapObject obj in corridorsToAdd)
            {
                PlaceObject(obj);
            }
        }

        private List<IMapObject> IsAvoidable(IMapObject room, bool vertical)
        {
            List<IMapObject> avoidPath = new();
            var x = room.X;
            var y = room.Y;
            if (vertical)
            {
                if (y != 0 && y != height - 1)
                {
                    if (x != width - 1)
                    {
                        //First check the right side of the room
                        if (CheckSidesOfRoom(x, y, vertical, true))
                        {
                            x++;
                            avoidPath.Add(new Corridor(x - 1, y - 1, new List<Exit> { Exit.EAST }));
                            avoidPath.Add(new Corridor(x, y - 1, new List<Exit> { Exit.NORTH, Exit.WEST }));
                            avoidPath.Add(new Corridor(x, y, new List<Exit> { Exit.NORTH, Exit.SOUTH }));
                            avoidPath.Add(new Corridor(x, y + 1, new List<Exit> { Exit.SOUTH, Exit.WEST }));
                            avoidPath.Add(new Corridor(x - 1, y + 1, new List<Exit> { Exit.EAST }));
                        }
                        else if (x != 0)
                        {
                            if (CheckSidesOfRoom(x, y, vertical, false))
                            {
                                x--;
                                avoidPath.Add(new Corridor(x + 1, y - 1, new List<Exit> { Exit.WEST }));
                                avoidPath.Add(new Corridor(x, y - 1, new List<Exit> { Exit.NORTH, Exit.EAST }));
                                avoidPath.Add(new Corridor(x, y, new List<Exit> { Exit.NORTH, Exit.SOUTH }));
                                avoidPath.Add(new Corridor(x, y + 1, new List<Exit> { Exit.SOUTH, Exit.EAST }));
                                avoidPath.Add(new Corridor(x + 1, y + 1, new List<Exit> { Exit.WEST }));
                            }
                        }
                    }
                    else
                    {
                        //Then check the left side of the room
                        if (CheckSidesOfRoom(x, y, vertical, false))
                        {
                            x--;
                            avoidPath.Add(new Corridor(x + 1, y - 1, new List<Exit> { Exit.WEST }));
                            avoidPath.Add(new Corridor(x, y - 1, new List<Exit> { Exit.NORTH, Exit.EAST }));
                            avoidPath.Add(new Corridor(x, y, new List<Exit> { Exit.NORTH, Exit.SOUTH }));
                            avoidPath.Add(new Corridor(x, y + 1, new List<Exit> { Exit.SOUTH, Exit.EAST }));
                            avoidPath.Add(new Corridor(x + 1, y + 1, new List<Exit> { Exit.WEST }));
                        }
                    }
                }
            }
            else
            {
                if (x != 0 && x != width - 1)
                {
                    if (y != height - 1)
                    {
                        //First check the top side of the room
                        if (CheckSidesOfRoom(x, y, vertical, true))
                        {
                            y++;
                            avoidPath.Add(new Corridor(x - 1, y - 1, new List<Exit> { Exit.NORTH }));
                            avoidPath.Add(new Corridor(x - 1, y, new List<Exit> { Exit.SOUTH, Exit.EAST }));
                            avoidPath.Add(new Corridor(x, y, new List<Exit> { Exit.EAST, Exit.WEST }));
                            avoidPath.Add(new Corridor(x + 1, y, new List<Exit> { Exit.SOUTH, Exit.WEST }));
                            avoidPath.Add(new Corridor(x + 1, y - 1, new List<Exit> { Exit.NORTH }));

                        }
                        else if (y != 0)
                        {
                            if (CheckSidesOfRoom(x, y, vertical, false))
                            {
                                y--;
                                avoidPath.Add(new Corridor(x - 1, y + 1, new List<Exit> { Exit.SOUTH }));
                                avoidPath.Add(new Corridor(x - 1, y, new List<Exit> { Exit.NORTH, Exit.EAST }));
                                avoidPath.Add(new Corridor(x, y, new List<Exit> { Exit.EAST, Exit.WEST }));
                                avoidPath.Add(new Corridor(x + 1, y, new List<Exit> { Exit.NORTH, Exit.WEST }));
                                avoidPath.Add(new Corridor(x + 1, y + 1, new List<Exit> { Exit.SOUTH }));
                            }
                        }
                    }
                    else
                    {
                        //Then check the bottom side of the room
                        if (CheckSidesOfRoom(x, y, vertical, false))
                        {
                            y--;
                            avoidPath.Add(new Corridor(x - 1, y + 1, new List<Exit> { Exit.SOUTH }));
                            avoidPath.Add(new Corridor(x - 1, y, new List<Exit> { Exit.NORTH, Exit.EAST }));
                            avoidPath.Add(new Corridor(x, y, new List<Exit> { Exit.EAST, Exit.WEST }));
                            avoidPath.Add(new Corridor(x + 1, y, new List<Exit> { Exit.NORTH, Exit.WEST }));
                            avoidPath.Add(new Corridor(x + 1, y + 1, new List<Exit> { Exit.SOUTH }));
                        }
                    }
                }
            }
            if (x == 0 && y == 0)
            {
                if (grid[x + 1, y + 1] is not Room)
                {
                    avoidPath.Add(new Corridor(x, y + 1, new List<Exit> { Exit.EAST }));
                    avoidPath.Add(new Corridor(x + 1, y + 1, new List<Exit> { Exit.SOUTH, Exit.WEST }));
                    avoidPath.Add(new Corridor(x + 1, y, new List<Exit> { Exit.NORTH }));

                }
            }
            else if (x == 0 && y == height - 1)
            {
                if (grid[x + 1, y - 1] is not Room)
                {
                    avoidPath.Add(new Corridor(x, y - 1, new List<Exit> { Exit.EAST }));
                    avoidPath.Add(new Corridor(x + 1, y - 1, new List<Exit> { Exit.NORTH, Exit.WEST }));
                    avoidPath.Add(new Corridor(x + 1, y, new List<Exit> { Exit.SOUTH }));
                }
            }
            else if (x == width - 1 && y == 0)
            {
                if (grid[x - 1, y + 1] is not Room)
                {
                    avoidPath.Add(new Corridor(x, y + 1, new List<Exit> { Exit.WEST }));
                    avoidPath.Add(new Corridor(x - 1, y + 1, new List<Exit> { Exit.SOUTH, Exit.EAST }));
                    avoidPath.Add(new Corridor(x - 1, y, new List<Exit> { Exit.NORTH }));
                }
            }
            else if (x == width - 1 && y == height - 1)
            {
                if (grid[x - 1, y - 1] is not Room)
                {
                    avoidPath.Add(new Corridor(x, y - 1, new List<Exit> { Exit.WEST }));
                    avoidPath.Add(new Corridor(x - 1, y - 1, new List<Exit> { Exit.NORTH, Exit.EAST }));
                    avoidPath.Add(new Corridor(x - 1, y, new List<Exit> { Exit.SOUTH }));
                }
            }
            if (avoidPath.Count == 0)
                return null;
            else
                return avoidPath;
        }

        public bool CheckSidesOfRoom(int x, int y, bool vertical, bool above)
        {
            bool result = true;
            if (vertical)
            {
                if (grid[x, y - 1] is Room || grid[x, y + 1] is Room)
                    result = false;
                else if (above)
                {
                    x++;
                }
                else { x--; }
                if (grid[x, y - 1] is Room || grid[x, y] is Room || grid[x, y + 1] is Room)
                    result = false;
            }
            else
            {
                if (grid[x - 1, y] is Room || grid[x + 1, y] is Room)
                    result = false;
                else if (above)
                {
                    y++;
                }
                else { y--; }
                if (grid[x - 1, y] is Room || grid[x, y] is Room || grid[x + 1, y] is Room)
                    result = false;
            }
            return result;
        }

        /// <summary>
        /// Places a map object on the grid if the position is empty, or addresses overlapping if the position is already occupied by a corridor.
        /// </summary>
        /// <param name="obj">The map object to be placed or for which to handle overlapping.</param>
        private void PlaceOrReplace(IMapObject obj)
        {
            if (grid[obj.X, obj.Y] == null)
            {
                PlaceObject(obj);
            }
            else if (grid[obj.X, obj.Y].ObjectType == MapObjectType.Corridor)
            {
                Correctoverlapping(obj.X, obj.Y, obj);
            }
        }

        /// <summary>
        /// Determines potential connection points between two rooms based on a randomly selected exit for each room.
        /// </summary>
        /// <param name="room1">The first room to connect.</param>
        /// <param name="room2">The second room to connect.</param>
        /// <returns>An array of integers representing the adjusted coordinates (x1, y1 for room1 and x2, y2 for room2) to suggest where corridors could connect the rooms.</returns>

        private int[] ConnectRooms(Room room1, Room room2)
        {
            foreach (Exit e in room1.ExitList)
            {
                if (GetNeighbour(room1, e) == room2)
                {
                    return new[] { -1, -1, -1, -1 };
                }
            }
            int x1 = room1.X;
            int x2 = room2.X;
            int y1 = room1.Y;
            int y2 = room2.Y;
            int rand1 = UnityEngine.Random.Range(0, room1.ExitList.Count);
            int rand2 = UnityEngine.Random.Range(0, room2.ExitList.Count);
            Exit exit1 = room1.ExitList[rand1];
            Exit exit2 = room2.ExitList[rand2];
            switch (exit1)
            {
                case Exit.NORTH: y1++; break;
                case Exit.SOUTH: y1--; break;
                case Exit.EAST: x1++; break;
                case Exit.WEST: x1--; break;
            }
            switch (exit2)
            {
                case Exit.NORTH: y2++; break;
                case Exit.SOUTH: y2--; break;
                case Exit.EAST: x2++; break;
                case Exit.WEST: x2--; break;
            }

            return new[] { x1, y1, x2, y2 };
        }

        /// <summary>
        /// Ensures that each room's exits are properly connected by adjusting or creating corridors at the exit points. 
        /// This method aims to correct any logical inconsistencies in room-to-corridor connections across the map.
        /// </summary>
        private void CorrectRoomConnections()
        {
            foreach (Room r in roomList)
            {
                foreach (Exit exit in r.ExitList)
                {
                    switch (exit)
                    {
                        case Exit.NORTH:
                            Correctoverlapping(r.X, r.Y + 1, new Corridor(r.X, r.Y + 1, new List<Exit> { Exit.SOUTH }));
                            break;
                        case Exit.SOUTH:
                            Correctoverlapping(r.X, r.Y - 1, new Corridor(r.X, r.Y - 1, new List<Exit> { Exit.NORTH }));
                            break;
                        case Exit.EAST:
                            Correctoverlapping(r.X + 1, r.Y, new Corridor(r.X + 1, r.Y, new List<Exit> { Exit.WEST }));
                            break;
                        case Exit.WEST:
                            Correctoverlapping(r.X - 1, r.Y, new Corridor(r.X - 1, r.Y, new List<Exit> { Exit.EAST }));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the neighbour of the object in the specified direction.
        /// </summary>
        /// <param name="obj">The object whose neighbour is to be found.</param>
        /// <param name="exit">The direction in which to find the neighbour.</param>
        /// <returns>The neighbour IMapObject in the specified direction, or null if not found.</returns>
        public IMapObject GetNeighbour(IMapObject obj, Exit exit)
        {
            var x = obj.X;
            var y = obj.Y;
            return exit switch
            {
                Exit.NORTH => GetObjectAt(x, y + 1),
                Exit.SOUTH => GetObjectAt(x, y - 1),
                Exit.EAST => GetObjectAt(x + 1, y),
                Exit.WEST => GetObjectAt(x - 1, y),
                _ => null,
            };
        }
        void OnDrawGizmos()
        {
            if (grid == null) return;

            float size = 1.0f; // Size of 1 room or corridor Gizmo

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = new Vector3(x * size, 0.7f, y * size);

                    if (grid[x, y] != null)
                    {
                        // Determine color based on the object type
                        if (path != null)
                        {
							if (path.Contains(grid[x, y]) && grid[x, y].ObjectType == MapObjectType.Corridor)
							{
								Gizmos.color = Color.cyan;
							}
							else
							{
								Gizmos.color = GetGizmoColor(grid[x, y].ObjectType);
							}
						}
                        else
                        {
                            Gizmos.color = GetGizmoColor(grid[x, y].ObjectType);
                        }
                        Gizmos.DrawCube(position, Vector3.one * size);
                        if (grid[x, y] is Room room)
                        {
                            if (room.Keys.Any(k => k.Name.Equals("endroomkey")))
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawSphere(position, size * 0.3f); // Red sphere for endroom key
                            }
                            if (room.Keys.Any(k => k.Name.Equals("pathkey")))
                            {
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawSphere(position, size * 0.3f); // Magenta sphere for path key
                            }
                            if (room.Lockers.Count > 0)
                            {
                                Gizmos.color = Color.yellow;
                                Gizmos.DrawSphere(position, size * 0.3f); // Yellow sphere for locker
                            }
                        }
                        var mapObject = grid[x, y];
                        foreach (var exit in mapObject.ExitList)
                        {
                            Vector3 exitDirection = GetExitDirectionVector(exit);
                            Gizmos.color = Color.white; // Exit line color
                            Gizmos.DrawLine(position, position + exitDirection * (size / 2));
                        }
                        foreach (var door in mapObject.Doors)
                        {
                            Vector3 doorDirection = GetExitDirectionVector(door.ExitLocation);
                            // Check if the room type determines the door color
                            Gizmos.color = mapObject.ObjectType == MapObjectType.EndRoom ? Color.red : Color.magenta;
                            Gizmos.DrawLine(position, position + doorDirection * (size / 2));
                        }


                    }
                    else
                    {
                        // Draw a smaller Gizmo for empty space
                        Gizmos.color = Color.gray;
                        Gizmos.DrawWireCube(position, Vector3.one * (size / 4));
                    }
                }
            }
        }


        Color GetGizmoColor(MapObjectType objectType)
        {
            return objectType switch
            {
                MapObjectType.StartRoom => Color.red,
                MapObjectType.EndRoom => Color.green,
                MapObjectType.CasualRoom => Color.blue,
                MapObjectType.Corridor => Color.yellow,
                _ => Color.black,
            };
        }

        Vector3 GetExitDirectionVector(Exit exit)
        {
            return exit switch
            {
                Exit.NORTH => Vector3.forward,
                Exit.EAST => Vector3.right,
                Exit.SOUTH => Vector3.back,
                Exit.WEST => Vector3.left,
                _ => Vector3.zero,
            };
        }

        public bool IsSolvable()
        {
            if (!LockPath)
            {
                return true;
            }
            var visited = new HashSet<IMapObject>();
            var queue = new Queue<IMapObject>();
            visited.Add(startRoom);
            queue.Enqueue(startRoom);

            while (queue.Count > 0)
            {
                var currentMapObject = queue.Dequeue();

                foreach (var exitDirection in currentMapObject.ExitList)
                {
                    IMapObject nextMapObject = GetNeighbour(currentMapObject, exitDirection);
                    if (currentMapObject.Doors.Count != 0)
                    {
                        continue;
                    }
                    if (nextMapObject != null && !visited.Contains(nextMapObject))
                    {
                        visited.Add(nextMapObject);
                        if (nextMapObject is Room room && room.Keys.Any(k => k.Name == "pathkey"))
                        {
                            return true;
                        }
                        queue.Enqueue(nextMapObject);
                    }
                }
            }
            return false;
        }

        public List<IMapObject> BfsBuildGraphWithPath()
        {
            var visited = new HashSet<IMapObject>();
            var queue = new Queue<IMapObject>();
            var parentMap = new Dictionary<IMapObject, IMapObject>();

            visited.Add(startRoom);
            queue.Enqueue(startRoom);
            parentMap[startRoom] = null; // Start room has no parent

            while (queue.Count > 0)
            {
                var currentMapObject = queue.Dequeue();

                // Check if current map object is the end room
                if (currentMapObject == endRoom)
                {
                    // If it's the end room, backtrack using the parent map to create the path
                    return ConstructPath(parentMap, currentMapObject);
                }

                foreach (var exitDirection in currentMapObject.ExitList)
                {
                    IMapObject nextMapObject = GetNeighbour(currentMapObject, exitDirection);
                    if (nextMapObject != null && !visited.Contains(nextMapObject))
                    {
                        visited.Add(nextMapObject);
                        queue.Enqueue(nextMapObject);
                        parentMap[nextMapObject] = currentMapObject; // Set the parent of the next room
                    }
                }
            }

            return null; 
        }

        private List<IMapObject> ConstructPath(Dictionary<IMapObject, IMapObject> parentMap, IMapObject endRoom)
        {
            var solutionPath = new List<IMapObject>();
            var step = endRoom;

            while (step != null)
            {
                solutionPath.Add(step);
                step = parentMap[step];
            }

            solutionPath.Reverse();
            return solutionPath;
        }

        public void LockEndRoom()
        {
            var exit = endRoom.ExitList[0];
            Door door = new Door(exit)
            {
                IsLocked = true,
                Name = "endroomdoor"
            };
            endRoom.AddDoor(door);
        }

        public void CreateDoorInPath(int position)
        {
            if (path != null)
            {
                Exit direction = Exit.NONE;
                foreach (var exit in path[position].ExitList)
                {
                    if (path.Contains(GetNeighbour(path[position], exit)))
                    {
                        direction = exit;
                        break;
                    }
                }
                if (direction == Exit.NONE)
                {
                    throw new ArgumentException("No direction for the door to be found");
                }
                var door = new Door(direction)
                {
                    IsLocked = true,
                    Name = "pathdoor"
                };
                path[position].AddDoor(door);
            }
            else
            {
                throw new ArgumentException("Path is not created yet");
            }
        }

        public void RemoveDoor(int postiion, Door door)
        {
            if (path != null)
            {
                path[postiion].RemoveDoor(door);
            }
            else
            {
                throw new ArgumentException("Path is not created yet");
            }
        }
        public bool PlaceRandomDoorWithKey()
        {
            var start = EndRoomDistance+1 / 2;
            List<int> doorPositions = Enumerable.Range(start - 1, path.Count - start).ToList();
            while (doorPositions.Count > 0)
            {
                var random = UnityEngine.Random.Range(0, doorPositions.Count);
                CreateDoorInPath(doorPositions[random]);
                if (PlaceKeyForPathRoom())
                {
                    return true;
                }
                else
                {
                    RemoveDoor(doorPositions[random], path[doorPositions[random]].Doors[0]);
                    doorPositions.RemoveAt(random);
                }
            }
            return false;
        }

        public bool PlaceKeyForPathRoom()
        {
            bool result = false;
            foreach (var room in roomList)
            {
                if (!path.Contains(room))
                {
                    var key = new Key(placedkeys, "pathkey");
                    room.AddKey(key);
                    if (IsSolvable())
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        room.RemoveKey(key);
                    }
                }
            }
            placedkeys++;
            return result;
        }

        public bool PlaceKeyForEndroom(int distance)
        {
            Key key = new Key(placedkeys, "endroomkey");
            foreach (var room in roomList)
            {
                if (Distance(room, endRoom) >= distance && room.ObjectType != MapObjectType.StartRoom && room.Keys.Count == 0)
                {
                    room.AddKey(key);
                    if (IsSolvable())
                    {
                        return true;
                    }
                    else
                    {
                        room.RemoveKey(key);
                    }
                }
            }
            return false;
        }

        public void PlaceLockers()
        {
            foreach (var room in roomList)
            {
                if (room.ObjectType == MapObjectType.CasualRoom)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int random = UnityEngine.Random.Range(0, 99);
                        if (random < LockerChance)
                        {
                            var locker = new Locker(i + 1);
                            room.AddLocker(locker);
                        }
                    }
                }
            }
        }

        public IMapObject SelectAIStartRoom()
        {
            if (randomAIStartPosition)
            {
                int randomNumber = UnityEngine.Random.Range(1, roomList.Count); 
                return roomList[randomNumber];
            }
            else
            {
                return endRoom; 
            }
        }

        public IMapObject SelectRandomRoomInDistance(IMapObject desiredRoom)
        {
            List<IMapObject> roomOptions = new List<IMapObject>();

            foreach (var room in roomList)
            {
                if (Distance(room, desiredRoom)> MinAIPatrolDistance)
                {
                    roomOptions.Add(room);
                }
            }

            if (roomOptions.Count == 0)
            {
                Debug.LogWarning("No room options found");
                return null;
            }

            int random = UnityEngine.Random.Range(0, roomOptions.Count);
            return roomOptions[random];
        }
    }
}
