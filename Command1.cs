using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Windows.Forms;

namespace WhatLevelAmI
{
    [Transaction(TransactionMode.Manual)]
    public class FilterElementsBySharedParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document; 

            // Your code goes here
       
        List<Element> myList = new FilteredElementCollector( doc )
                .WhereElementIsNotElementType()
                .Where( e => e.LookupParameter( "MyLevel" ) != null )
                .ToList();

            // Process the filtered elements (for example, print their IDs)
            foreach (Element element in myList)
                {
                    GetLocationElement(element);
                    GetElementBoundingBox(doc, element);
            }


                return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData.Data;
        }


       public static void GetElementBoundingBox(Autodesk.Revit.DB.Document document, Autodesk.Revit.DB.Element element)
        {
            // Get the BoundingBox instance for current view.
            BoundingBoxXYZ box = element.get_BoundingBox(document.ActiveView);
            if (null == box)
            {
                throw new Exception("Selected element doesn't contain a bounding box.");
            }

            //string info = "Bounding box is enabled: " + box.Enabled.ToString();

            //// Give the user some information.
            //TaskDialog.Show("Revit", info);

            // Get the minimum and maximum points of the bounding box.
            Transform trf = box.Transform;
            XYZ min = box.Min;
            XYZ max = box.Max;
            XYZ maxInModelCoords = trf.OfPoint(max);
            XYZ minInModelCoords = trf.OfPoint(min);
            double zCentroid = (maxInModelCoords.Z + minInModelCoords.Z) / 2.0;

            string message = "BoundingBoxXYZ : ";
            message += "\n'Maximum' coordinates: " + maxInModelCoords;
            message += "\n'Minimum' coordinates: " + minInModelCoords;
            message += "\n'Centroid Z value: " + zCentroid;

            TaskDialog.Show("Revit", message);

        }

        public static void GetLocationElement(Element element)
        {
            Autodesk.Revit.DB.Location position = element.Location;

            String prompt = null;
            if (null == position)
            {
                prompt = $"No location can be found in element {element.Name.ToString()}";
            }
            else
            {
                // If the location is a point location, give the user information
                Autodesk.Revit.DB.LocationPoint positionPoint = position as Autodesk.Revit.DB.LocationPoint;
                if (null != positionPoint)
                {
                    prompt = $"Element {element.Name.ToString()} has a point location.";
                }
                else
                {

                    // If the location is a curve location, give the user information
                    Autodesk.Revit.DB.LocationCurve positionCurve = position as Autodesk.Revit.DB.LocationCurve;
                    if (null != positionCurve)
                    {
                        prompt = $"Element {element.Name.ToString()} has a curve location.";
                    }
                }
            }

            if (null != prompt)
            {
                TaskDialog.Show($"Revit", prompt);
            }
        }
    }

}
