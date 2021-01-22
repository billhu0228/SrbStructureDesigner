using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace SRBModel.Member
{
    public class Pile : MemberBase
    {
        double D,L;

        public Pile(double d, double l)
        {
            D = d;
            L = l;
            
            StraightRebar N1 = new StraightRebar(RebarClass.T, 32, "");
            rebarList.Add(N1);
            
        }

        public override DBObjectCollection Render()
        {
            DBObjectCollection ret=new DBObjectCollection();

            Circle a = new Circle(Point3d.Origin, Vector3d.ZAxis, 100);
            Circle a1 = new Circle(Point3d.Origin, Vector3d.ZAxis, 100);
            Circle a2 = new Circle(Point3d.Origin, Vector3d.ZAxis, 100);
            Circle a3 = new Circle(Point3d.Origin, Vector3d.ZAxis, 100);

            ret.Add(a);

            ret.Add(a1);

            return ret;            
        }
    }



}
