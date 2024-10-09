using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace AreaOfPolygon
{
    public class Location
    {
        public static void LocationOfElement(IIfcBuildingElement element)
        {
            if (element.ObjectPlacement is IIfcLocalPlacement locations)
            {
                var relativeLocation = locations.RelativePlacement as IIfcAxis2Placement3D;
                if (relativeLocation != null)
                {
                    Console.WriteLine("\nLocation :");
                    Console.WriteLine($"local coordinates: X= {relativeLocation.Location.X}, local Y= {relativeLocation.Location.Y},local Z= {relativeLocation.Location.Z}");
                }
            }
        }
    }
}
