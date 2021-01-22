using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using SRBModel.Member;

namespace SRBInterface
{
    public static class main
    {
        [CommandMethod("PlotPile")]
        public static void PlotPile()
        {

            //--------------------------------------------------------------------------------------
            // 基本句柄
            //--------------------------------------------------------------------------------------
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;


            Pile A = new Pile(3, 10);
            DBObjectCollection ret = A.Render();

            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                foreach (DBObject drawitem in ret)
                {
                    modelSpace.AppendEntity((Entity)drawitem);
                    tr.AddNewlyCreatedDBObject((Entity)drawitem, true);
                }
                tr.Commit();
            }

        }

        [CommandMethod("PlotPileCap")]
        public static void PlotPileCap()
        {

            //--------------------------------------------------------------------------------------
            // 基本句柄
            //--------------------------------------------------------------------------------------
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;


            PileCap A = new PileCap();
            DBObjectCollection ret = A.Render();

            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                foreach (DBObject drawitem in ret)
                {
                    modelSpace.AppendEntity((Entity)drawitem);
                    tr.AddNewlyCreatedDBObject((Entity)drawitem, true);
                }
                tr.Commit();
            }

        }



    }
}
