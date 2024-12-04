using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MapObjects
{
    public class Room : IMapObject
    {
        public int X { get; set; }
        public int Y { get; set; }
       
        public List <Exit> ExitList { get; }

        public List<Door> Doors { get; private set; }

        public List<Key> Keys { get; private set; }
        public MapObjectType ObjectType { get; set; }

        public List<Locker> Lockers { get; private set; }

        public Room(int x, int y, List<Exit> exitList)
        {
            X = x;
            Y = y;
            ExitList = exitList;
            Doors = new List<Door>();
            Keys = new List<Key>();
            Lockers = new List<Locker>();
        }

        public void AddDoor(Door door)
        {
            Doors.Add(door);
        }

        public void RemoveDoor(Door door)
        {
            Doors.Remove(door);
        }

        public void AddKey(Key key)
        {
            Keys.Add(key);
        }

        public void RemoveKey(Key key)
        {
            Keys.Remove(key);
        }

        public void AddLocker(Locker locker)
        {
            Lockers.Add(locker);
        }

        public void RemoveLocker(Locker locker)
        {
            Lockers.Remove(locker);
        }
    }
}
