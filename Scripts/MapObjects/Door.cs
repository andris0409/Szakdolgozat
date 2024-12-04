using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MapObjects
{
    public class Door
    {
        public String Name { get; set; }
        public Exit ExitLocation { get; set; }
        public bool IsLocked { get; set; }

        public Door(Exit exitLocation)
        {
            this.ExitLocation = exitLocation;
            IsLocked = false;
        }

    }
}
