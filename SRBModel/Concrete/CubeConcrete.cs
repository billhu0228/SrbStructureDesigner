using System;

namespace SRBModel
{
    public class CubeConcrete : ConcreteBase
    {
        public CubeConcrete()
        {
        }

        public CubeConcrete(ConcreteClass cls, string typeName) : base(cls, typeName)
        {
        }

        public override float Volumn => throw new NotImplementedException();
    }




}
