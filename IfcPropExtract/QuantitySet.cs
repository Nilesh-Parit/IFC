using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Step21;
using Xbim.Common.Metadata;
using Xbim.Common.Geometry;
//using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.PropertyResource;

namespace IfcPropExtract
{
    public class QuantitySet
    {
        public static void getQuantitySet(string guid, string? filepath)
        {
            string slabGuid = "1shLRqRhD93OBwyi82sd99";

            using(var model = IfcStore.Open(filepath))
            {
                var slab = model.Instances.FirstOrDefault<IfcSlab>(x => x.GlobalId == slabGuid);

                if (slab == null) return;

                Console.WriteLine("Slab Name : " + slab.Name);
                Console.WriteLine("Slab Guid : " + slab.GlobalId);

                Console.WriteLine();

                var Qset = slab.IsDefinedBy
                    .Where(r => r.RelatingPropertyDefinition is IfcElementQuantity)
                    .Select(r => r.RelatingPropertyDefinition as IfcElementQuantity);

                foreach (var qset in Qset)
                {
                    if (qset == null) continue;
                    Console.WriteLine("QuantitySet Name : " + qset.Name);

                    Console.WriteLine();

                    foreach (var quantity in qset.Quantities)
                    {
                        Console.Write("\t");

                        if (quantity is IIfcQuantityLength length)
                            Console.WriteLine(length.Name + ": " + length.LengthValue + ", " + 
                                (length.Unit != null ? length.Unit.ToString() : "no unit"));

                        if(quantity is IIfcQuantityArea area)
                            Console.WriteLine(area.Name + ": " + area.AreaValue + ", " +
                                (area.Unit != null ? area.Unit.ToString() : "no unit"));

                        if (quantity is IIfcQuantityVolume volume)
                            Console.WriteLine(volume.Name + ": " + volume.VolumeValue + ", " +
                                (volume.Unit != null ? volume.Unit.ToString() : "no unit"));
                    }
                }
            }
        }
    }
}
