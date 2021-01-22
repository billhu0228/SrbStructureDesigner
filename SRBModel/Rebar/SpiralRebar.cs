namespace SRBModel
{
    /// <summary>
    /// 螺旋筋
    /// </summary>
    public class SpiralRebar : RebarBase
    {
        public SpiralRebar()
        {
        }

        public SpiralRebar(RebarClass cls, int diameter, string typeName,
            double D1, double D2, double s)
            : base(cls, diameter, typeName)
        {
        }

        public override float Length
        {
            get
            {
                return 0;
            }
        }
    }
}
