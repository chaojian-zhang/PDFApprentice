using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFApprentice.Data
{
    public class Location
    {
        public int X;
        public int Y;
    }

    public class Entity
    {
        public string Content;
        public string Tags;
        public string Title;
        public Location Location;
        public int OwnerPage;
    }
}
