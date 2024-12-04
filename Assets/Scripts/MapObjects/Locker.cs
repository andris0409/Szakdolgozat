using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    public class Locker 
    {
        public int Place { get; set; }
        public bool IsOccupied { get; private set; }

        public void EnterLocker()
        {
            IsOccupied = true;
            
        }

        public void ExitLocker()
        {
            IsOccupied = false;
            
        }

        public Locker(int place)
        {
            Place = place;
        }
    }
}
