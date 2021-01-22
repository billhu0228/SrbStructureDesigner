using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBModel
{
    public enum RebarClass
    {
        None=0,
        R = 300,
        Y = 400,
        T = 500,
    }


    public abstract class RebarBase
    {
        public RebarClass ClassName;
        public int Diameter;
        public string TypeName;

        public RebarBase()
        {
            ClassName = 0;
            Diameter = 0;
            TypeName = "";
        }
        public RebarBase(RebarClass clsName, int diameter, string typeName)
        {
            ClassName = clsName;
            Diameter = diameter;
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        }

        public abstract float Length { get; }

    }

    public class RebarCollection : List<RebarBase>
    {
        public RebarCollection()
        {
        }

        public RebarCollection(int capacity) : base(capacity)
        {
        }

        public RebarCollection(IEnumerable<RebarBase> collection) : base(collection)
        {
        }
    }



}
