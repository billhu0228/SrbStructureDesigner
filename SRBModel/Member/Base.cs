using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;


namespace SRBModel.Member
{
    public abstract class MemberBase
    {
        public ConcreteCollection concList;
        public RebarCollection rebarList;
        public abstract DBObjectCollection Render();
        public MemberBase()
        {
            concList = new ConcreteCollection();
            rebarList = new RebarCollection();
        }
    }

    public class SpreadFooting : MemberBase
    {
        public override DBObjectCollection Render()
        {
            throw new NotImplementedException();
        }
    }
    public class PileCap : MemberBase
    {
        public override DBObjectCollection Render()
        {
            throw new NotImplementedException();
        }
    }
    public class Plinth : MemberBase
    {
        public override DBObjectCollection Render()
        {
            throw new NotImplementedException();
        }
    }
    public class PierColumn : MemberBase
    {
        public override DBObjectCollection Render()
        {
            throw new NotImplementedException();
        }
    }

    public class Linker : MemberBase
    {
        public override DBObjectCollection Render()
        {
            throw new NotImplementedException();
        }
    }
}
