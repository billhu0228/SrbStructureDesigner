using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBModel
{
    public enum ConcreteClass
    {
        None = 0,
        C20 = 20,
        C30 = 30,
        C35 = 35, C40 = 40, C50 = 50,
    }

    public abstract class ConcreteBase 
    {
        public ConcreteClass ClassName;
        public string TypeName;

        public ConcreteBase()
        {
            ClassName = 0;
            TypeName = "";
        }

        public ConcreteBase(ConcreteClass clsName, string typeName)
        {
            ClassName = clsName;
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        }

        public abstract float Volumn { get; }
    }


    public class ConcreteCollection : List<ConcreteBase>
    {
        public ConcreteCollection()
        {
        }

        public ConcreteCollection(int capacity) : base(capacity)
        {
        }

        public ConcreteCollection(IEnumerable<ConcreteBase> collection) : base(collection)
        {
        }
    }


}
