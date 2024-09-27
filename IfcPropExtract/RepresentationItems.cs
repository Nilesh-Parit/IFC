using System;
using System.Collections.Generic;
using Xbim.Ifc2x3; // Use the appropriate namespace for your IFC version
using Xbim.Ifc4;   // Change this if you are using a different version
using Xbim.ModelGeometry.Scene;
using Xbim.Ifc4.Interfaces;
using System.Configuration;
using Xbim.Ifc;

/*
 * This code gives the list of all the IIfcRepresentationItem present in an ifc file
 */

namespace IfcPropExtract
{
    public class RepresentationItems
    {
        public static void getAllRepresentations()
        {
            // Provide the path to the IFC file
            string? ifcFilePath = ConfigurationManager.AppSettings["IfcFilePath"];
            // Load the IFC model
            // Load the IFC model
            using (var model = IfcStore.Open(ifcFilePath))
            {
                // HashSet to hold unique representation items
                HashSet<string> representationItems = new HashSet<string>();

                // Iterate over all IfcProducts
                foreach (var product in model.Instances.OfType<IIfcProduct>())
                {
                    // Check if the product has a representation
                    if (product.Representation != null)
                    {
                        foreach (var representation in product.Representation.Representations)
                        {
                            // Iterate through the items in the representation
                            foreach (var item in representation.Items)
                            {
                                // Check if the item is an instance of IIfcRepresentationItem
                                if (item is IIfcRepresentationItem representationItem)
                                {
                                    // Add to HashSet to ensure uniqueness
                                    representationItems.Add(item.GetType().Name);
                                }
                            }
                        }
                    }
                }

                // Output the results
                foreach (var item in representationItems)
                {
                    Console.WriteLine(item);
                }
            }
        }
    }
}
