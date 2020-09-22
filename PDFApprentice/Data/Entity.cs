using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFApprentice.Data
{
    public struct Location
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Entity
    {
        public string Content { get; set; }
        public string Tags { get; set; }
        public Location Location { get; set; }
        public uint OwnerPage { get; set; }
    }
}
