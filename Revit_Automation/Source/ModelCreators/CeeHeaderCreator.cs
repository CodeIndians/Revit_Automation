using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

namespace Revit_Automation
{
    internal class CeeHeaderCreator
    {
        private Document doc;
        private Form1 form;

        enum SlopeDirection
        {

        }
        public CeeHeaderCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            Dictionary <double, List<InputLine>> sortedInputLineCollection = new Dictionary <double, List<InputLine>>();
            sortedInputLineCollection = SortInputLinesByElevation(colInputLines);

            foreach (KeyValuePair<double, List<InputLine>> kvp in sortedInputLineCollection)
            {
                List<InputLine> list = kvp.Value;
                double elevation = kvp.Key;
                string levelName = "";
                InputLine temp = list[0];

                // Filter levels based on buldings to use
                List<Level> filteredLevels = new List<Level>();
                foreach (Level filteredlevel in levels)
                {
                    if (filteredlevel.Name.Contains(temp.strBuildingName))
                    {
                        filteredLevels.Add(filteredlevel);
                    }
                }

                for (int i = 0; i < filteredLevels.Count() - 1; i++)
                {
                    Level tempLevel = filteredLevels.ElementAt(i);

                    if ((temp.startpoint.Z < (tempLevel.Elevation + 1)) && (temp.startpoint.Z > (tempLevel.Elevation - 1)))
                    {
                        Level toplevel = filteredLevels.ElementAt(i + 1);
                        levelName = toplevel.Name;
                        break;
                    }
                }

                // Get the settings for this level
                CeeHeaderSettings ceeHeaderSettings = GetCeeHeaderSettingsForGivenLevel(levelName);

                PlaceCeeHeaders(ceeHeaderSettings, list);
               
            }
        }

        private void PlaceCeeHeaders(CeeHeaderSettings ceeHeaderSettings, List<InputLine> InputlineList)
        {
            double dSpan = double.Parse(GlobalSettings.s_strDeckSpan);

        }

        private CeeHeaderSettings GetCeeHeaderSettingsForGivenLevel(string levelName)
        {
            CeeHeaderSettings ceeHeaderSettings = GlobalSettings.lstCeeHeaderSettings.Find(temp => temp.strGridName == levelName); 
            return ceeHeaderSettings;
        }

        private Dictionary<double, List<InputLine>> SortInputLinesByElevation(List<InputLine> colInputLines)
        {
            Dictionary<double, List<InputLine>> sortedInputLineCollection = new Dictionary<double, List<InputLine>>();
            foreach (InputLine inputLine in colInputLines) 
            {
                double zCoord = inputLine.startpoint.Z;
                if(!sortedInputLineCollection.ContainsKey(zCoord))
                {
                    sortedInputLineCollection[zCoord] = new List<InputLine>();
                }
                sortedInputLineCollection[zCoord].Add(inputLine);
            }

            return sortedInputLineCollection;
        }
    }
}