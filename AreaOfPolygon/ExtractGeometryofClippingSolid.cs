using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Common.Geometry.Shapes.OpenCascade;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace AreaOfPolygon
{
    public class ExtractGeometryofClippingSolid
    {
       public static void ExtractGeometryFromSolid(IIfcSolidModel solid, IIfcLocalPlacement placement)
        {
            if (solid is IIfcExtrudedAreaSolid extrudedSolid)
                RepresentationItems.ExtrudedAreaSolidItemHandler(extrudedSolid, placement);
            else if (solid is IIfcFacetedBrep facetedBrep)
                RepresentationItems.FacetedBrepItemHandler(facetedBrep, placement);
            else if (solid is IIfcAdvancedBrep advancedBrep)
                RepresentationItems.AdvancedBrepItemHandler(advancedBrep, placement);
            else
                Console.WriteLine("Unsupported solid model type");
        }
        public static void ExtractGeometryFromHalfSpaceSolid(IIfcHalfSpaceSolid solid, IIfcLocalPlacement placement)
        { if (solid is IIfcPolygonalBoundedHalfSpace boundedHalfSpace)
                RepresentationItems.PolygonalBoundedHalfSpaceItemHandler(boundedHalfSpace, placement);
        }

    }
}
