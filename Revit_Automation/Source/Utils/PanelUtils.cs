using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Automation.Source.Utils
{
    public class PanelUtils
    {
        private static Document _document;
        public PanelUtils(Document document)
        {
            _document = document;
        }

        public void ComputePanelDirectionForExteriorPanels()
        {
            List<InputLine> lines = InputLineUtility.colInputLines;
            ElementId rightcolumnID = null, leftColumnID = null;

            for (int i = 0; i < lines.Count; i++)
            {
                InputLine inputLine = lines[i];

                if (inputLine.strWallType == "Ex w/ Insulation")
                {
                    
                    IOrderedEnumerable<Level> levels = ModelCreator.FindAndSortLevels(_document);
                    Level toplevel = null, baselevel = null;

                    LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;

                    PostCreationUtils.ComputeTopAndBaseLevels(inputLine, levels, out toplevel, out baselevel);

                    Element baseAttach = GenericUtils.GetNearestFloorOrRoof(baselevel, inputLine.startpoint, _document);

                    // Create columns on either side of the line and try to attach the base.
                    // Whichever side base attach is true, Panel direction is that
                    XYZ midpoint = new XYZ((inputLine.startpoint.X + inputLine.endpoint.X) / 2.0,
                                            (inputLine.startpoint.Y + inputLine.endpoint.Y) / 2.0,
                                            (inputLine.startpoint.Z + inputLine.endpoint.Z) / 2.0);

                    XYZ rightpoint = null, leftpoint = null;

                    if (linetype == LineType.Horizontal)
                    {
                        rightpoint = new XYZ(midpoint.X, midpoint.Y + 1.1, midpoint.Z);
                        leftpoint = new XYZ(midpoint.X, midpoint.Y - 1.1, midpoint.Z);
                    }
                    else
                    {
                        rightpoint = new XYZ(midpoint.X + 1.1, midpoint.Y, midpoint.Z);
                        leftpoint = new XYZ(midpoint.X - 1.1, midpoint.Y, midpoint.Z);
                    }

                    bool bright = false;

                    string strFamilySymbol = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);
                    FamilySymbol columnType = SymbolCollector.GetSymbol(strFamilySymbol, "Post", SymbolCollector.FamilySymbolType.posts);

                    using (Transaction stx = new Transaction(_document))
                    {


                        GenericUtils.SupressWarningsInTransaction(stx);

                        stx.Start("Exterior Panel Direction Computation");

                        if (rightcolumnID != null)
                            _document.Delete(rightcolumnID);
                        if (leftColumnID != null)
                            _document.Delete(leftColumnID);

                        // Create the column instance at the point
                        FamilyInstance rightcolumn = _document.Create.NewFamilyInstance(rightpoint, columnType, baselevel, StructuralType.Column);
                        FamilyInstance leftcolumn = _document.Create.NewFamilyInstance(leftpoint, columnType, baselevel, StructuralType.Column);

                        rightcolumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(2);
                        leftcolumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(2);

                        rightcolumnID = rightcolumn.Id;
                        leftColumnID = leftcolumn.Id;

                        if (baseAttach != null)
                        {
                            ColumnAttachment.AddColumnAttachment(_document, rightcolumn, baseAttach, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);

                            ColumnAttachment.AddColumnAttachment(_document, leftcolumn, baseAttach, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        stx.Commit();
                    }

                    Element right = _document.GetElement(rightcolumnID);
                    Element left = _document.GetElement(leftColumnID);

                    bool bRightSet = false, bLeftSet = false;
                    double dRightHeight = 0.0, dLeftHeight = 0.0;

                    Parameter rightHeightParam = right.LookupParameter("Height");
                    if (rightHeightParam != null)
                    {
                        dRightHeight = rightHeightParam.AsDouble();
                    }
                    Parameter LeftHeightParam = left.LookupParameter("Height");
                    if (LeftHeightParam != null)
                    {
                        dLeftHeight = LeftHeightParam.AsDouble();
                    }
                    if (dLeftHeight > dRightHeight)
                    {

                        if (linetype == LineType.Horizontal)
                        {
                            inputLine.strHorizontalPanelDirection = "L";
                        }
                        else
                        {
                            inputLine.strVerticalPanelDirection = "D";
                        }
                    }
                    else
                    {
                        if (linetype == LineType.Horizontal)
                        {
                            inputLine.strHorizontalPanelDirection = "R";
                        }
                        else
                        {
                            inputLine.strVerticalPanelDirection = "U";
                        }
                    }
                    lines[i] = inputLine;
                }
            }
            InputLineUtility.colInputLines = lines;

            using (Transaction tx = new Transaction(_document))
            {
                tx.Start("Cleanup");
                if (rightcolumnID != null)
                    _document.Delete(rightcolumnID);
                if (leftColumnID != null)
                    _document.Delete(leftColumnID);
                tx.Commit();
            }
        }
    }
}
