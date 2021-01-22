namespace SRBModel
{
    /// <summary>
    /// 直筋
    /// </summary>
    class StraightRebar : RebarBase
    {
        public StraightRebar(RebarClass cls, int diameter, string typeName) : base(cls, diameter, typeName)
        {
        }

        public override float Length => throw new System.NotImplementedException();
    }




}
