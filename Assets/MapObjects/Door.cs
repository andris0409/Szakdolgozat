using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class Door
    {
        public Exit ExitLocation { get; set; }
        public bool IsLocked { get; set; }
        public Key Key { get; set; }

        public Door(Exit exitLocation)
        {
            this.ExitLocation = exitLocation;
            IsLocked = false;
            Key = null;
        }

    }
}
