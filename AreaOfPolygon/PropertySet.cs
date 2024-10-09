using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace AreaOfPolygon
{
    public class PropertySet
    {
        public static void PropertySetValues(IIfcBuildingElement element)
        {
            var propertysets = element.IsDefinedBy.Where(x => x.RelatingPropertyDefinition is IIfcPropertySet)
                .Select(x => x.RelatingPropertyDefinition as IIfcPropertySet);

            foreach (var propertyset in propertysets)
            {
                Console.WriteLine($"propertyset name: {propertyset!.Name}");

                foreach (var property in propertyset.HasProperties)
                {
                    if (property is IIfcPropertySingleValue singleValue)
                        Console.WriteLine($"property name: {property.Name}, value: {singleValue.NominalValue}, unit: {singleValue.Unit?.ToString() ?? "__"}");
                    else if (property is IIfcPropertyEnumeratedValue propertyEnumeratedValue)
                    {
                        Console.WriteLine("Enumerated value: ");
                        foreach (var value in propertyEnumeratedValue.EnumerationValues)
                            Console.WriteLine($"    - {value}");
                    }
                    else if (property is IIfcPropertyListValue listValueProperty)
                    {
                        Console.WriteLine("  List Values:");
                        foreach (var value in listValueProperty.ListValues)
                        {
                            Console.WriteLine($"    - {value}");
                        }
                    }
                    else if (property is IIfcPropertyTableValue tableValueProperty)
                    {
                        Console.WriteLine("  Table Values:");
                        foreach (var (defValue, measureValue) in tableValueProperty.DefiningValues.Zip(tableValueProperty.DefinedValues, Tuple.Create))
                        {
                            Console.WriteLine($"    - Defining: {defValue}, Defined: {measureValue}");
                        }
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
