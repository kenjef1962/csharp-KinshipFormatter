namespace ACKinshipFormatter
{
    public class KinshipTerm
    {
        public string Term { get; set; }
        public string Result { get; set; }

        public KinshipTerm(string term, string result)
        {
            Term = term;
            Result = result;
        }
    }
}
