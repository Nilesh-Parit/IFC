using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.Interfaces;

namespace AreaOfPolygon
{
    public class QuantitySet
    {
        public static void QuantitySetValues(IIfcBuildingElement element) 
        {
            var quantitySets = element.IsDefinedBy.Where(rel => rel.RelatingPropertyDefinition is IfcElementQuantity)
                                       .Select(rel => rel.RelatingPropertyDefinition as IfcElementQuantity);

            foreach (var quantityset in quantitySets)
            {
                Console.WriteLine($"\nQuantityset name: {quantityset!.Name}");
                foreach (var quantity in quantityset.Quantities)
                {
                    if (quantity is IIfcQuantityLength quantityLength)
                        Console.WriteLine($"{quantityLength.Name}: {quantityLength.LengthValue}, unit: {quantityLength.Unit?.ToString() ?? "__"} ");
                    if (quantity is IIfcQuantityArea quantityArea)
                        Console.WriteLine($"{quantityArea.Name}: {quantityArea.AreaValue} ,unit: {quantityArea.Unit?.ToString() ?? "__"}");
                    if (quantity is IIfcQuantityVolume quantityVolume)
                        Console.WriteLine($"{quantityVolume.Name}: {quantityVolume.VolumeValue},unit: {quantityVolume.Unit?.ToString() ?? "__"}");
                }
            }
        }
    }
}
