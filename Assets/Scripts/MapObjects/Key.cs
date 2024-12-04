using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MapObjects
{
    public class Key
    {
        public int Id { get; }
        public string Name { get; }
        public Key(int id, string name)
        {
            this.Id = id;
            Name = name;
        }
    }
}
