using System;

namespace SRBModel
{
    public class CylinderConcrete : ConcreteBase
    {
        public CylinderConcrete()
        {
        }

        public CylinderConcrete(ConcreteClass cls, string typeName) : base(cls, typeName)
        {
        }

        public override float Volumn => throw new NotImplementedException();
    }




}
