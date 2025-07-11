﻿using Autodesk.Revit.DB;
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

            // Get all levels in the document
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(Level));
            //List<Level> levels = levelCollector.Cast<Level>().ToList();
            List<Level> levels = levelCollector.Cast<Level>().OrderBy(l => l.Elevation).ToList();

            double tolerancemm = 100;
            double tolerance = Common.Utils.ConvertMmToFeet(tolerancemm);
            int count = 0;

            Transaction transaction = new Transaction(doc, "Set MyLevel Parameter");
            transaction.Start();

            foreach (Element element in myList)
            {
                    //GetLocationElement(element);
                    //GetLevelInformation(element);
                    double zCentroid = GetElementBoundingBoxMid(doc, element);
                    Common.Utils.SetParameterValue(element, "MyLevel", FindClosestLevel(zCentroid, tolerance, levels).Name);
                    count++;
            }
            transaction.Commit();
            transaction.Dispose();

            TaskDialog.Show("Revit", $"Set the MyLevel parameter for {count} elements.");

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "What Level Am I?";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "runs What Level Am I? plugin");

            return myButtonData.Data;
        }

        public Level FindClosestLevel(double zCentroid, double tolerance, List<Level> levels)
        {
            Level closestLevel = levels[0];

            for (int i = 1; i < levels.Count; i++)
            {
                if ((zCentroid - tolerance) < levels[i].Elevation)
                {
                    closestLevel = levels[i-1];
                    break;
                }
                closestLevel = levels[i];
            }
            //string message = $"The closest level to the element is: {closestLevel.Name} at elevation {closestLevel.Elevation}.\n";
            //TaskDialog.Show("Revit", message);
            return closestLevel;
        }

        public static double GetElementBoundingBoxMid(Autodesk.Revit.DB.Document document, Autodesk.Revit.DB.Element element)
        {
            // Get the BoundingBox instance for current view.
            BoundingBoxXYZ box = element.get_BoundingBox(document.ActiveView);
            double zCentroid = 0.0;
            if (null == box)
            {
                throw new Exception("Selected element doesn't contain a bounding box.");
            }
            // Get the minimum and maximum points of the bounding box.
            Transform trf = box.Transform;
            XYZ min = box.Min;
            XYZ max = box.Max;
            XYZ maxInModelCoords = trf.OfPoint(max);
            XYZ minInModelCoords = trf.OfPoint(min);
            zCentroid = (maxInModelCoords.Z + minInModelCoords.Z) / 2.0;

            //string message = "BoundingBoxXYZ : ";
            //message += "\n'Maximum' coordinates: " + maxInModelCoords;
            //message += "\n'Minimum' coordinates: " + minInModelCoords;
            //message += "\n'Centroid Z value: " + zCentroid;
            //TaskDialog.Show("Revit", message);

            return zCentroid;

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

        public void GetLevelInformation(Autodesk.Revit.DB.Element element)
        {
            // Get the level object to which the element is assigned.
            if (element.LevelId.Equals(ElementId.InvalidElementId))
            {
                TaskDialog.Show("Revit", "The element isn't based on a level.");
            }
            else
            {
                Level level = element.Document.GetElement(element.LevelId) as Level;

                // Format the prompt information(Name and elevation)
                String prompt = "The element is based on a level.";
                prompt += "\nThe level name is:  " + level.Name;
                prompt += "\nThe level elevation is:  " + level.Elevation;

                // Show the information to the user.
                TaskDialog.Show("Revit", prompt);
            }
        }

    }

}
