namespace SRBModel
{
    /// <summary>
    /// 箍筋
    /// </summary>
    public class StirrupRebar : RebarBase
    {
        public StirrupRebar()
        {
        }

        public StirrupRebar(RebarClass cls, int diameter, string typeName) : base(cls, diameter, typeName)
        {
        }

        public override float Length => throw new System.NotImplementedException();
    }




}
