using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.MapObjects;

namespace Assets.Scripts.Exithandlers
{
    public  interface IExitHandler
    {
        GameObject GetExit(Exit exitDirection);
    }
}
