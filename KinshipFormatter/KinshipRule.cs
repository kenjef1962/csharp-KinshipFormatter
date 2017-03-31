namespace ACKinshipFormatter
{
    public class KinshipRule
    {
        public string Expression { get; set; }
        public string Result { get; set; }

        public KinshipRule(string expression, string result)
        {
            Expression = expression;
            Result = result;
        }
    }
}
