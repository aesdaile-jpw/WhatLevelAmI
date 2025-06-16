using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;

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
            // this is some code created by Copilot

                // Create a FilteredElementCollector to collect all elements in the document
               // FilteredElementCollector collector = new FilteredElementCollector(doc);

                // Filter elements that have the shared parameter "MyLevel"
                //var elementsWithMyLevel = collector
                //    .WherePasses(new ElementParameterFilter(new SharedParameterElementFilter("MyLevel")))
                //    .ToList();

            //public static List<Element> GetElementsInCategoryWithParameter( Document doc, BuiltInCategory category, string parameterName ) { return new FilteredElementCollector( doc ).OfCategory( category ).WhereElementIsNotElementType().Where( e => e.LookupParameter( parameterName ) != null ).ToList(); }


        List<Element> myList = new FilteredElementCollector( doc )
                .WhereElementIsNotElementType()
                .Where( e => e.LookupParameter( "MyLevel" ) != null )
                .ToList();

            // Process the filtered elements (for example, print their IDs)
            foreach (Element element in myList)
                {
                    TaskDialog.Show("Element ID", element.Id.ToString());
                }

                return Result.Succeeded;
        }

        //public class SharedParameterElementFilter : ISelectionFilter
        //{
        //    private string _parameterName;

        //    public SharedParameterElementFilter(string parameterName)
        //    {
        //        _parameterName = parameterName;
        //    }

        //    public bool PassesFilter(Element element)
        //    {
        //        Parameter param = element.LookupParameter(_parameterName);
        //        return param != null; //&& param.HasValue//
        //    }
        //}
         // end code
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
    }

}
