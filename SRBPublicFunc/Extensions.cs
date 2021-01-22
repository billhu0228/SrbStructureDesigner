using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace SRBPublicFunc
{
    public static class Extensions
    {
        public static string ToString2(this List<double> pt)
        {
            var st = (from a in pt select a.ToString()).ToList();
            string s = string.Join(",", st.ToArray());

            return s;
        }
        public static List<double> PadDistinct(this List<double> v)
        {
            var f = v.Distinct().ToList();

            if (f.Count == 1)
            {
                return f;
            }
            else if (f.Count == 2)
            {
                if (Math.Abs(f[0] - f[1]) < 0.1)
                {
                    return new List<double>() { f[0] };

                }
                return f;
            }
            else if (f.Count == 3)
            {
                return new List<double>() { f[0], f[1] };
            }
            else
            {
                return new List<double>() { f[0], f.Max() };
                //throw new System.Exception("支座间距错误..");
            }

        }


        public static void AddLayer(this LayerTable LT, string Lname, Color acicolor)
        {
            Database db = LT.Database;
            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                if (LT.IsWriteEnabled == false) LT.UpgradeOpen();
                if (!LT.Has(Lname))
                {
                    LayerTableRecord newly = new LayerTableRecord();
                    newly.Name = Lname;
                    newly.Color = acicolor;

                    LT.Add(newly);
                    tr.AddNewlyCreatedDBObject(newly, true);
                }
                else
                {
                    LayerTableRecord acLyrTblRec = tr.GetObject(LT[Lname], OpenMode.ForWrite) as LayerTableRecord;
                    acLyrTblRec.Color = acicolor;
                }
                tr.Commit();
            }
        }

        public static Polyline ConvertToPolyline(this Region Reg)
        {

            Brep bp = new Brep(Reg);
            Autodesk.AutoCAD.BoundaryRepresentation.Vertex st = bp.Vertices.ToList()[0];

            Polyline res = new Polyline() { Closed = true };
            int i = 0;
            foreach (var item in bp.GetVerticesStartingFrom(st))
            {
                res.AddVertexAt(i, new Point2d(item.Point.X, item.Point.Y), 0, 0, 0);
                i++;
            }
            Point3d LeftUp = new Point3d(Reg.GeometricExtents.MinPoint.X, Reg.GeometricExtents.MaxPoint.Y, 0);
            Point3d St = res.GetClosestVertexTo(LeftUp);
            Point3d Or = new Point3d(0, 0, 0);
            Vector3d Xa = Vector3d.XAxis;
            Vector3d Ya = Vector3d.YAxis;
            var Prop = Reg.AreaProperties(ref Or, ref Xa, ref Ya);



            var LineList = (from a in bp.Edges.ToList() select new Line(a.Curve.StartPoint, a.Curve.EndPoint)).ToList();
            var tst = (from a in LineList select a.Length).ToList();
            Polyline AA = new Polyline() { Closed = true, Layer = "粗线" };
            int numV = LineList.Count();

            List<Point2d> PtToFormPL = new List<Point2d>();
            Point3d PtToSearch = St;
            for (int k = 0; k < numV; k++)
            {
                var EdgFindOut = (from a in LineList where a.StartPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) || a.EndPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) select a).ToList();

                if (EdgFindOut.Count == 1)
                {
                    PtToFormPL.Add(PtToSearch.Convert2D());

                    PtToSearch = EdgFindOut[0].StartPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) ? EdgFindOut[0].EndPoint : EdgFindOut[0].StartPoint;

                    if (!LineList.Remove(EdgFindOut[0]))
                    {
                        throw new System.Exception();
                    }

                }
                else if (EdgFindOut.Count == 2)
                {

                    var PtToSearch1 = EdgFindOut[0].StartPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) ? EdgFindOut[0].EndPoint : EdgFindOut[0].StartPoint;
                    var PtToSearch2 = EdgFindOut[1].StartPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) ? EdgFindOut[1].EndPoint : EdgFindOut[1].StartPoint;
                    Vector3d v1 = PtToSearch1 - PtToSearch;
                    Vector3d v2 = PtToSearch2 - PtToSearch;
                    Vector3d vk = PtToSearch - Prop.Centroid.Convert3D();
                    if (vk.GetAngleTo(v1, Vector3d.ZAxis) < vk.GetAngleTo(v2, Vector3d.ZAxis))
                    {
                        PtToFormPL.Add(PtToSearch.Convert2D());

                        PtToSearch = EdgFindOut[0].StartPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) ? EdgFindOut[0].EndPoint : EdgFindOut[0].StartPoint;

                        if (!LineList.Remove(EdgFindOut[0]))
                        {
                            throw new System.Exception();
                        }


                    }
                    else
                    {
                        PtToFormPL.Add(PtToSearch.Convert2D());

                        PtToSearch = EdgFindOut[1].StartPoint.IsEqualTo(PtToSearch, new Tolerance(1e-6, 1e-6)) ? EdgFindOut[1].EndPoint : EdgFindOut[1].StartPoint;

                        if (!LineList.Remove(EdgFindOut[1]))
                        {
                            throw new System.Exception();
                        }
                    }
                }
                else
                {
                    //throw new System.Exception();
                }



                ;


            }

            for (int j = 0; j < PtToFormPL.Count; j++)
            {
                AA.AddVertexAt(j, PtToFormPL[j], 0, 0, 0);
            }
            return AA;
        }


        public static Point3d GetClosestVertexTo(this Polyline PL, Point3d other)
        {
            List<Point3d> VS = new List<Point3d>();
            for (int i = 0; i < PL.NumberOfVertices; i++)
            {
                VS.Add(PL.GetPoint3dAt(i));

            }
            VS.Sort((x, y) => x.DistanceTo(other).CompareTo(y.DistanceTo(other)));
            return VS[0];
        }

        public static Region ConvertToRegin(this Polyline PL)
        {
            PL.Closed = true;
            DBObjectCollection segs = PL.GetOffsetCurves(0);
            Region res = (Region)Region.CreateFromCurves(segs)[0];
            return res;
        }
        public static Region ConvertToReginNew(this Polyline PL)
        {
            PL.Closed = true;
            DBObjectCollection segs = PL.GetOffsetCurves(48);
            Region res = (Region)Region.CreateFromCurves(segs)[0];
            return res;
        }
        public static DBObjectCollection Append(this DBObjectCollection dbc, DBObjectCollection other)
        {
            foreach (DBObject item in other)
            {
                dbc.Add(item);
            }
            return dbc;
        }



        public static Point2d Swap(this Point2d pt, bool flip = true)
        {
            return flip ? new Point2d(pt.Y, pt.X) : pt;
        }
        public static Point3d Pad(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }
        public static Point2d Strip(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }

        /// <summary>
        /// Creates a layout with the specified name and optionally makes it current.
        /// </summary>
        /// <param name="name">The name of the viewport.</param>
        /// <param name="select">Whether to select it.</param>
        /// <returns>The ObjectId of the newly created viewport.</returns>        
        public static ObjectId CreateAndMakeLayoutCurrent(this LayoutManager lm, string name, bool select = true)
        {
            var id = lm.GetLayoutId(name);
            if (!id.IsValid)
            {
                id = lm.CreateLayout(name);
            }
            if (select)
            {
                lm.CurrentLayout = name;
            }
            return id;
        }
        /// <summary>
        /// Applies an action to the specified viewport from this layout.
        /// Creates a new viewport if none is found withthat number.
        /// </summary>
        /// <param name="tr">The transaction to use to open the viewports.</param>
        /// <param name="vpNum">The number of the target viewport.</param>
        /// <param name="f">The action to apply to each of the viewports.</param>
        public static void ApplyToViewport(this Layout lay, Transaction tr, int vpNum, Action<Viewport> f)
        {
            var vpIds = lay.GetViewports();
            Viewport vp = null;
            foreach (ObjectId vpId in vpIds)
            {
                var vp2 = tr.GetObject(vpId, OpenMode.ForWrite) as Viewport;
                if (vp2 != null && vp2.Number == vpNum)
                {                    // We have found our viewport, so call the action
                    vp = vp2;
                    vp.GridOn = false;
                    break;
                }
            }
            if (vp == null)
            {                // We have not found our viewport, so create one
                var btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite);
                vp = new Viewport();
                // Add it to the database
                btr.AppendEntity(vp);
                tr.AddNewlyCreatedDBObject(vp, true);
                // Turn it - and its grid - on
                //vp.On = true;
                vp.GridOn = false;
            }
            // Finally we call our function on it
            f(vp);
        }

        /// <summary>
        /// 获得Line中点Point3D.
        /// </summary>
        /// <param name="aline">目标线.</param>       

        public static Point3d GetMidPoint3d(this Line aline, double xx = 0, double yy = 0)
        {
            double x = 0.5 * (aline.StartPoint.X + aline.EndPoint.X);
            double y = 0.5 * (aline.StartPoint.Y + aline.EndPoint.Y);
            return new Point3d(x + xx, y + yy, 0);
        }

        public static Point2d GetXPoint2d(this Line aline, double part = 0.5, double xx = 0, double yy = 0)
        {
            double x = aline.StartPoint.X + part * (aline.EndPoint.X - aline.StartPoint.X);
            double y = aline.StartPoint.Y + part * (aline.EndPoint.Y - aline.StartPoint.Y);
            return new Point2d(x + xx, y + yy);
        }
        /// <summary>
        /// 获得中点
        /// </summary>
        /// <param name="aline"></param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <returns></returns>
        public static Point2d GetMidPoint2d(this Line aline, double xx = 0, double yy = 0)
        {
            double x = 0.5 * (aline.StartPoint.X + aline.EndPoint.X);
            double y = 0.5 * (aline.StartPoint.Y + aline.EndPoint.Y);
            return new Point2d(x + xx, y + yy);
        }

        /// <summary>
        /// 获得线段被矩形切割后线段
        /// </summary>
        /// <param name="aline">原线段</param>
        /// <param name="minPoint">左下角点</param>
        /// <param name="maxPoint">右上角点</param>
        /// <returns>新线段</returns>
        public static Line CutByRect(this Line aline, Point2d minPoint, Point2d maxPoint, bool isInf = true)
        {
            Line res = aline;
            Polyline BD = new Polyline();
            Point3dCollection pts = new Point3dCollection();

            double dx = maxPoint.X - minPoint.X;
            double dy = maxPoint.Y - minPoint.Y;
            BD.AddVertexAt(0, minPoint, 0, 0, 0);
            BD.AddVertexAt(1, minPoint.Convert2D(dx), 0, 0, 0);
            BD.AddVertexAt(2, maxPoint, 0, 0, 0);
            BD.AddVertexAt(3, minPoint.Convert2D(0, dy), 0, 0, 0);
            BD.Closed = true;

            if (aline.StartPoint.X == aline.EndPoint.X)
            {
                if (aline.StartPoint.X == minPoint.X)
                {
                    res.StartPoint = minPoint.Convert3D();
                    res.EndPoint = minPoint.Convert3D(0, dy);
                    return res;
                }
                if (aline.StartPoint.X == maxPoint.X)
                {
                    res.StartPoint = minPoint.Convert3D(dx);
                    res.EndPoint = maxPoint.Convert3D();
                    return res;
                }
            }
            if (isInf)
            {
                BD.IntersectWith(aline, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                res.StartPoint = pts[0];
                res.EndPoint = pts[1];
            }
            else
            {
                BD.IntersectWith(aline, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                switch (pts.Count)
                {
                    case 0:
                        break;

                    case 1:
                        if (aline.StartPoint.X > minPoint.X)
                        {
                            res.StartPoint = pts[0];
                        }
                        else
                        {
                            res.EndPoint = pts[0];
                        }
                        break;

                    case 2:
                        res.StartPoint = pts[0];
                        res.EndPoint = pts[1];
                        break;
                }
            }


            return res;
        }

        public static Line CutByDoubleLine(this Line aline, Point2d minPoint, Point2d maxPoint, bool isInf = true)
        {
            Line res = (Line)aline.Clone();
            Line A = new Line(minPoint.Convert3D(), minPoint.Convert3D(1));
            Line B = new Line(maxPoint.Convert3D(), maxPoint.Convert3D(1));
            Point3dCollection pts = new Point3dCollection();

            pts.Clear();
            aline.IntersectWith(A, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            res.StartPoint = pts[0];

            pts.Clear();
            aline.IntersectWith(B, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            res.EndPoint = pts[0];

            return res;
        }









        /// <summary>
        /// Apply plot settings to the provided layout.
        /// </summary>
        /// <param name="pageSize">The canonical media name for our page size.</param>
        /// <param name="styleSheet">The pen settings file (ctb or stb).</param>
        /// <param name="devices">The name of the output device.</param>



        public static void SetPlotSettings(this Layout lay, string pageSize, string styleSheet, string device)
        {
            using (var ps = new PlotSettings(lay.ModelType))
            {
                ps.CopyFrom(lay);
                var psv = PlotSettingsValidator.Current;
                // Set the device
                var devs = psv.GetPlotDeviceList();
                if (devs.Contains(device))
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }
                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);
                if (mns.Contains(pageSize))
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }
                psv.SetPlotWindowArea(ps, new Extents2d(new Point2d(0, 0), new Point2d(420, 297)));
                //psv.SetPlotRotation(ps, PlotRotation.Degrees000);//设置横向;纵向Degrees270

                //psv.SetStdScale(ps, 1);
                //psv.SetPlotType(ps, PlotType.Layout);//设置打印范围

                psv.RefreshLists(ps);
                psv.SetPlotRotation(ps, PlotRotation.Degrees000);

                //psv.SetPlotCentered(ps, true);
                psv.SetCustomPrintScale(ps, new CustomScale(1, 1));
                psv.SetPlotType(ps, PlotType.Window);
                psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);//设置的单位
                psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);//设置比例
                //psv.SetPlotCentered(ps, true);
                // Set the pen settings
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                }
                // Copy the PlotSettings data back to the Layout

                var upgraded = false;
                if (!lay.IsWriteEnabled)
                {
                    lay.UpgradeOpen();
                    upgraded = true;
                }
                lay.CopyFrom(ps);
                if (upgraded)
                {
                    lay.DowngradeOpen();
                }
            }
        }

        public static void SetA1PlotSettings(this Layout lay, string pageSize, string styleSheet, string device)
        {
            using (var ps = new PlotSettings(lay.ModelType))
            {

                ps.CopyFrom(lay);
                var psv = PlotSettingsValidator.Current;
                // Set the device
                var devs = psv.GetPlotDeviceList();
                if (devs.Contains(device))//设置设备
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }
                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);

                if (mns.Contains(pageSize))//设置纸张大小
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }
                //psv.SetPlotWindowArea(ps, new Extents2d(new Point2d(0, 0), new Point2d(841, 594)));//设置打印纸张大小范围
                //                                                                                   //psv.SetPlotType(ps, PlotType.Window);//设置打印范围
                //                                                                                   //psv.SetStdScale(ps, 1);//设置标准比例

                //psv.SetCustomPrintScale(ps, new CustomScale(1, 1));
                //psv.SetUseStandardScale(ps, true);
                psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);//设置比例
                psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);//设置的单位
                //psv.SetPlotRotation(ps, PlotRotation.Degrees000);//设置横向;纵向Degrees270

                //psv.SetStdScale(ps, 1);
                //psv.SetPlotType(ps, PlotType.Layout);//设置打印范围

                psv.RefreshLists(ps);
                psv.SetPlotRotation(ps, PlotRotation.Degrees000);
                psv.SetCustomPrintScale(ps, new CustomScale(1, 1));
                psv.SetPlotWindowArea(ps, new Extents2d(1, 0, 841, 594));
                psv.SetPlotType(ps, PlotType.Window);
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                    //psv.
                }
                // Copy the PlotSettings data back to the Layout

                var upgraded = false;
                if (!lay.IsWriteEnabled)
                {
                    lay.UpgradeOpen();
                    upgraded = true;
                }
                lay.CopyFrom(ps);
                if (upgraded)
                {
                    lay.DowngradeOpen();
                }
            }
        }


        /// <summary>
        /// Determine the maximum possible size for this layout.
        /// </summary>
        /// <returns>The maximum extents of the viewport on this layout.</returns>
        public static Extents2d GetMaximumExtents(this Layout lay)
        {
            // If the drawing template is imperial, we need to divide by            
            // 1" in mm (25.4)
            var div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;
            // We need to flip the axes if the plot is rotated by 90 or 270 deg
            var doIt =
              lay.PlotRotation == PlotRotation.Degrees090 ||
              lay.PlotRotation == PlotRotation.Degrees270;

            // Get the extents in the correct units and orientation
            var min = lay.PlotPaperMargins.MinPoint.Swap(doIt) / div;
            var max = (lay.PlotPaperSize.Swap(doIt) -
               lay.PlotPaperMargins.MaxPoint.Swap(doIt).GetAsVector()) / div;
            return new Extents2d(min, max);
        }



        /// <summary>
        /// Sets the size of the viewport according to the provided extents.
        /// </summary>
        /// <param name="ext">The extents of the viewport on the page.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void ResizeViewport(this Viewport vp, Extents2d ext, double fac = 1.0)
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;
            vp.CenterPoint = (Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5).Pad();
        }

        /// <summary>
        /// 改写Polylin 的 globewidth.
        /// </summary>
        /// <param name="thePline">多线名称。</param>       
        /// <param name="width">全局宽度。</param>       
        public static void GlobalWidth(this Polyline thePline, double width = 0)
        {
            for (int i = 0; i < thePline.NumberOfVertices; i++)
            {
                thePline.SetStartWidthAt(i, width);
                thePline.SetEndWidthAt(i, width);
            }

        }


        /// <summary>
        /// 获得对称
        /// </summary>
        /// <param name="thePline">多线名称</param>       
        /// <param name="axis">对称轴</param>       
        public static Polyline GetMirror(this Polyline thePline, Line2d axis)
        {
            Polyline res = (Polyline)thePline.Clone();
            res.TransformBy(Matrix3d.Mirroring(axis.Convert3D()));
            return res;
        }


        public static int GetVertexIdx(this Polyline thePL, Point3d pt)
        {
            int i = 0;
            while (true)
            {
                Point3d vetx = thePL.GetPoint3dAt(i);
                if (vetx == null)
                {
                    return -1;
                }
                else if (vetx.DistanceTo(pt) < 1)
                {
                    return i;
                }
                i++;
            }
        }





        /// <summary>
        /// 根据多线线段编号获取对应直线
        /// </summary>
        /// <param name="thePline"></param>
        /// <param name="SegID">线段编号</param>
        /// <returns>对应直线</returns>
        public static Line GetLine(this Polyline thePline, int SegID)
        {
            var seg = thePline.GetLineSegmentAt(SegID);
            Point3d p1 = seg.StartPoint;
            Point3d p2 = seg.EndPoint;
            Line res;
            if (p1.X < p2.X)
            {
                res = new Line(p1, p2);
            }
            else
            {
                res = new Line(p2, p1);
            }

            return res;
        }


        public static Line3d Convert3D(this Line2d theL2d)
        {
            return new Line3d(theL2d.StartPoint.Convert3D(), theL2d.EndPoint.Convert3D());
        }





        [DllImport("User32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);







        public static Point2d Convert2D(this Point3d theP3d, double x = 0, double y = 0)
        {
            return new Point2d(theP3d.X + x, theP3d.Y + y);
        }


        public static Point3d Convert3D(this Point3d theP3d, double x = 0, double y = 0, double z = 0)
        {
            return new Point3d(theP3d.X + x, theP3d.Y + y, theP3d.Z + z);
        }
        public static Vector3d Convert3D(this Vector2d theV2d, double x = 0, double y = 0)
        {
            return new Vector3d(theV2d.X + x, theV2d.Y + y, 0);
        }

        public static Point3d Convert3D(this Point2d theP2d, double x = 0, double y = 0)
        {
            return new Point3d(theP2d.X + x, theP2d.Y + y, 0);
        }

        public static Point2d Convert2D(this Point2d theP2d, double x = 0, double y = 0)
        {
            return new Point2d(theP2d.X + x, theP2d.Y + y);
        }


        public static Point2d MoveDistance(this Point2d theP2d, Vector2d theVec, double dist)
        {
            Vector2d newVec = new Vector2d(dist * Math.Cos(theVec.Angle), dist * Math.Sin(theVec.Angle));
            return theP2d.TransformBy(Matrix2d.Displacement(newVec));
        }

        public static double GetK(this Line cL)
        {
            double k = 0;
            k = (cL.EndPoint.Y - cL.StartPoint.Y) / (cL.EndPoint.X - cL.StartPoint.X);
            return k;
        }

        /// <summary>
        /// 平面\立面视口.
        /// </summary>
        /// <param name="vpNum">1=平面，2=剖面.</param>        
        /// <param name="ytop">1=平面，2=剖面.</param> 
        public static void DrawMyViewport(this Viewport vp, int vpNum, Point3d BasePoint, Point2d CenterPoint, int scale)
        {
            if (vpNum == 1)
            {
                vp.CenterPoint = BasePoint.Convert3D(150, 148.5);
                vp.Width = 240;
                vp.Height = 277;
            }
            else if (vpNum == 2)
            {
                vp.CenterPoint = BasePoint.Convert3D(220 + 70, 148.5, 0);
                vp.Width = 240;
                vp.Height = 277;
            }
            vp.ViewCenter = CenterPoint;
            vp.CustomScale = 1.0 / scale;
            vp.Layer = "图框";
            vp.Locked = true;
        }



        /// <summary>
        /// Sets the view in a viewport to contain the specified model extents.
        /// </summary>
        /// <param name="ext">The extents of the content to fit the viewport.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void FitContentToViewport(this Viewport vp, Extents3d ext, double fac = 1.0)
        {
            // Let's zoom to just larger than the extents
            vp.ViewCenter = (ext.MinPoint + ((ext.MaxPoint - ext.MinPoint) * 0.5)).Strip();
            // Get the dimensions of our view from the database extents
            var hgt = ext.MaxPoint.Y - ext.MinPoint.Y;
            var wid = ext.MaxPoint.X - ext.MinPoint.X;
            // We'll compare with the aspect ratio of the viewport itself

            // (which is derived from the page size)
            var aspect = vp.Width / vp.Height;
            // If our content is wider than the aspect ratio, make sure we
            // set the proposed height to be larger to accommodate the
            // content        
            if (wid / hgt > aspect)
            {
                hgt = wid / aspect;
            }
            // Set the height so we're exactly at the extents
            vp.ViewHeight = hgt;
            // Set a custom scale to zoom out slightly (could also
            // vp.ViewHeight *= 1.1, for instance)
            vp.CustomScale *= fac;
        }
        /// <summary>
        /// 引入并插入参照
        /// </summary>
        /// <param name="db"></param>
        /// <param name="path"></param>
        /// <param name="paperSpaceId"></param>
        /// <param name="pos"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool XrefAttachAndInsert(this Database db, string path, ObjectId paperSpaceId, Point3d pos, string name = null)
        {
            var ret = false;
            if (!File.Exists(path))
                return ret;

            if (String.IsNullOrEmpty(name))
                name = Path.GetFileNameWithoutExtension(path);

            try
            {
                using (var tr = db.TransactionManager.StartOpenCloseTransaction())
                {
                    var xId = db.AttachXref(path, name);
                    if (xId.IsValid)
                    {
                        Layout tmp = (Layout)tr.GetObject(paperSpaceId, OpenMode.ForWrite);
                        var btr = (BlockTableRecord)tr.GetObject(tmp.BlockTableRecordId, OpenMode.ForWrite);
                        var br = new BlockReference(pos, xId);
                        btr.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                        ret = true;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            { }

            return ret;
        }


        /// <summary>
        /// 引入并插入参照（模型空间）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="path"></param>
        /// <param name="paperSpaceId"></param>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool XrefAttachAndInsertModel(this Database db, string path, ObjectId paperSpaceId, Point3d pos, Scale3d scale, string name = null)
        {
            var ret = false;
            if (!File.Exists(path))
                return ret;

            if (String.IsNullOrEmpty(name))
                name = Path.GetFileNameWithoutExtension(path);

            try
            {
                using (var tr = db.TransactionManager.StartOpenCloseTransaction())
                {
                    var xId = db.AttachXref(path, name);
                    if (xId.IsValid)
                    {
                        //Layout tmp = (Layout)tr.GetObject(paperSpaceId, OpenMode.ForWrite);
                        var btr = (BlockTableRecord)tr.GetObject(paperSpaceId, OpenMode.ForWrite);
                        var br = new BlockReference(pos, xId);


                        br.ScaleFactors = scale;//设置块参照的缩放比例
                        btr.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                        ret = true;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            { }

            return ret;
        }


        public static ObjectId CreatPaperSpace(this Database db)
        {
            // 基本句柄

            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            DBDictionary lays = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
            int name = 1;
            foreach (DBDictionaryEntry item in lays)
            {
                if (item.Key == name.ToString())
                {
                    name++;
                }
            }
            ObjectId id = LayoutManager.Current.CreateAndMakeLayoutCurrent(name.ToString(), true);
            var lay = (Layout)tr.GetObject(id, OpenMode.ForWrite);
            lay.SetPlotSettings("A3", "monochrome.ctb", "Adobe PDF");
            tr.Commit();
            tr.Dispose();
            return id;
        }

        public static ObjectId CreatLayout(this Database db, string Name, bool isA1 = false)
        {

            ObjectId curLayId;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary lays = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                if (!lays.Contains(Name))
                {
                    curLayId = LayoutManager.Current.CreateLayout(Name);
                }
                else
                {
                    LayoutManager.Current.DeleteLayout(Name);
                    curLayId = LayoutManager.Current.CreateLayout(Name);
                    //curLayId = LayoutManager.Current.GetLayoutId(Name);
                }

                LayoutManager.Current.CurrentLayout = Name;
                var lay = (Layout)tr.GetObject(curLayId, OpenMode.ForWrite);
                if (!isA1)
                    lay.SetPlotSettings("ISO_full_bleed_A3_(420.00_x_297.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                else
                    lay.SetA1PlotSettings("ISO_full_bleed_A1_(841.00_x_594.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                tr.Commit();
            }
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayoutManager LM = LayoutManager.Current;
                DBDictionary LayoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                Layout CurrentLo = tr.GetObject(curLayId, OpenMode.ForRead) as Layout;
                BlockTableRecord BlkTblRec = tr.GetObject(CurrentLo.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId ID in BlkTblRec)
                {
                    Viewport VP = tr.GetObject(ID, OpenMode.ForRead) as Viewport;
                    if (VP != null)
                    {
                        VP.UpgradeOpen();
                        VP.Erase();
                    }
                }

                if (LM.LayoutExists("布局1"))
                {
                    LM.DeleteLayout("布局1");
                }
                if (LM.LayoutExists("布局2"))
                {
                    LM.DeleteLayout("布局2");
                }
                LM.CurrentLayout = "Model";

                tr.Commit();
            }

            return curLayId;
        }

        public static ObjectId CreatLayout(this Database db, string Name, string TKPath, bool isA1 = false, int NumPDF = 1)
        {

            ObjectId curLayId;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary lays = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                if (!lays.Contains(Name))
                {
                    curLayId = LayoutManager.Current.CreateLayout(Name);
                }
                else
                {
                    LayoutManager.Current.DeleteLayout(Name);
                    curLayId = LayoutManager.Current.CreateLayout(Name);
                    //curLayId = LayoutManager.Current.GetLayoutId(Name);
                }
                for (int page = 0; page < NumPDF; page++)
                {
                    if (isA1)
                        db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(page * 841));
                    else
                        db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(page * 420));
                }
                LayoutManager.Current.CurrentLayout = Name;
                var lay = (Layout)tr.GetObject(curLayId, OpenMode.ForWrite);
                if (!isA1)
                    lay.SetPlotSettings("ISO_full_bleed_A3_(420.00_x_297.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                else
                    lay.SetA1PlotSettings("ISO_full_bleed_A1_(841.00_x_594.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                tr.Commit();
            }
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayoutManager LM = LayoutManager.Current;
                DBDictionary LayoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                Layout CurrentLo = tr.GetObject(curLayId, OpenMode.ForRead) as Layout;
                BlockTableRecord BlkTblRec = tr.GetObject(CurrentLo.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId ID in BlkTblRec)
                {
                    Viewport VP = tr.GetObject(ID, OpenMode.ForRead) as Viewport;
                    if (VP != null)
                    {
                        VP.UpgradeOpen();
                        VP.Erase();
                    }
                }

                if (LM.LayoutExists("布局1"))
                {
                    LM.DeleteLayout("布局1");
                }
                if (LM.LayoutExists("布局2"))
                {
                    LM.DeleteLayout("布局2");
                }
                LM.CurrentLayout = "Model";

                tr.Commit();
            }

            return curLayId;
        }

        public static ObjectId CreatUpDownLayout(this Database db, string Name, string TKPath, bool isA1 = false, int NumPDF = 1, bool isUp = true, bool isPrint = true)
        {

            ObjectId curLayId;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary lays = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                if (!lays.Contains(Name))
                {
                    curLayId = LayoutManager.Current.CreateLayout(Name);
                    LayoutManager.Current.CurrentLayout = Name;
                }
                else
                {
                    //LayoutManager.Current.DeleteLayout(Name);
                    //curLayId = LayoutManager.Current.CreateLayout(Name);
                    //curLayId = LayoutManager.Current.GetLayoutId(Name);
                    curLayId = LayoutManager.Current.GetLayoutId(Name);
                }
                for (int page = 0; page < NumPDF; page++)
                {
                    if (isA1)
                    {
                        if (isUp)
                            db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(page * 841));
                        else
                        {
                            if (isPrint)
                                db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(-841 * (page + 1)));
                            else
                                db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(page * 841, -594));
                        }
                    }
                    else
                    {
                        if (isUp)
                            db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(page * 420));
                        else
                        {
                            if (isPrint)
                                db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(-420 * (page + 1)));
                            else
                                db.XrefAttachAndInsert(TKPath, curLayId, Point3d.Origin.Convert3D(page * 420, -297));
                        }

                    }
                }
                try
                {

                }
                catch { }
                var lay = (Layout)tr.GetObject(curLayId, OpenMode.ForWrite);
                if (!isA1)
                    lay.SetPlotSettings("ISO_full_bleed_A3_(420.00_x_297.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                else
                    lay.SetA1PlotSettings("ISO_full_bleed_A1_(841.00_x_594.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                tr.Commit();
            }
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayoutManager LM = LayoutManager.Current;
                DBDictionary LayoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                Layout CurrentLo = tr.GetObject(curLayId, OpenMode.ForRead) as Layout;
                BlockTableRecord BlkTblRec = tr.GetObject(CurrentLo.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId ID in BlkTblRec)
                {
                    Viewport VP = tr.GetObject(ID, OpenMode.ForRead) as Viewport;
                    if (VP != null)
                    {
                        VP.UpgradeOpen();
                        VP.Erase();
                    }
                }

                if (LM.LayoutExists("布局1"))
                {
                    LM.DeleteLayout("布局1");
                }
                if (LM.LayoutExists("布局2"))
                {
                    LM.DeleteLayout("布局2");
                }
                LM.CurrentLayout = "Model";

                tr.Commit();
            }

            return curLayId;
        }

        /// <summary>
        /// 平面\立面视口.
        /// </summary>
        public static void SettingSizeAndLoc(this Viewport vp, double w, double h, Point2d basepoint, double scale)
        {
            vp.Width = w;
            vp.Height = h;
            vp.CenterPoint = basepoint.Convert3D(0.5 * w, 0.5 * h);
            vp.CustomScale = 1.0 / scale;
            vp.Locked = false;
        }

    }
}
