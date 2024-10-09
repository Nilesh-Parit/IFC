using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.SharedBldgElements;

namespace AreaOfPolygon
{
    public class IfcWalls
    {
        public string? wallGuid { get; set; }
        public IfcWalls(IfcStore model, string guid)
        {
           wallGuid = guid;
           Initialisers(model, guid);
        }

        public static void Initialisers(IfcStore model, string wallGuid)
        {
            var wall = model.Instances.OfType<IfcWall>().Where(x => x.GlobalId == wallGuid).FirstOrDefault();
            if (wall == null)
            {
                Console.WriteLine($"Wall for Guid = {wallGuid} not found ");
                return;
            }
        }

       
    }
}
