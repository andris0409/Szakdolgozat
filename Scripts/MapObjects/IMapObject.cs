using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MapObjects
{
    public enum MapObjectType
    {
        StartRoom,
        EndRoom,
        CasualRoom,
        Corridor,
        Empty 
    }

    public enum Exit
    {
        NORTH,
        WEST,
        SOUTH,
        EAST,
        NONE
    }
    public interface IMapObject
    {
        MapObjectType ObjectType { get; }
        int X { get; set; }
        int Y { get; set; }
        List<Exit> ExitList { get; }

        List<Door> Doors { get; }

        void AddDoor(Door door);

        void RemoveDoor(Door door);

    }
}
