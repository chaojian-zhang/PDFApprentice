using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFApprentice.Data
{
    public struct Location
    {
        public int X;
        public int Y;
    }

    public class Entity
    {
        public string Content;
        public string Tags;
        public Location Location;
        public int OwnerPage;
    }
}
