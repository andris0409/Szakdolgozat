using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MapObjects
{
    public class Corridor : IMapObject
    {
        public int X { get ; set ; }
        public int Y { get ; set ; }

        public List<Exit> ExitList { get; }

        public List<Door> Doors { get; private set; }

        public MapObjectType ObjectType => MapObjectType.Corridor;

        public Corridor(int x, int y, List<Exit> exitList) {
            X = x;
            Y = y;
            ExitList = exitList;
            Doors = new List<Door>();
        }

        public void AddDoor(Door door)
        {
            Doors.Add(door);
        }

        public void RemoveDoor(Door door)
        {
            Doors.Remove(door);
        }
        
    }
}
