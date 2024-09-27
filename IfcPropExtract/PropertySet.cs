using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;

namespace IfcPropExtract
{
    public class PropertySet
    {
        public static void getPropertySet(string guid, string? filepath)
        {
            string wall1 = "1Z4ebjXojEHwWyXyiXHM$q";

            using (var model = IfcStore.Open(filepath))
            {
                var wall = model.Instances.FirstOrDefault<IfcWall>(x => x.GlobalId == wall1);

                if (wall == null) return;

                Console.WriteLine("Wall name : " + wall.Name);
                Console.WriteLine("Wall type : " + wall.GetType().Name);

                var propertysets = wall.IsDefinedBy
                    .Where(r => r.RelatingPropertyDefinition is IfcPropertySet)
                    .Select(r => r.RelatingPropertyDefinition as IfcPropertySet);

                Console.WriteLine();

                foreach (var propertyset in propertysets)
                {
                    if (propertyset == null) continue;
                    Console.WriteLine("Property Set Name : " + propertyset.Name);
                    Console.WriteLine();

                    foreach (var property in propertyset.HasProperties.OfType<IfcPropertySingleValue>())
                    {
                        if (property == null) continue;

                        Console.WriteLine("\t"+property.Name+" : "+property.NominalValue);
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
