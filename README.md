# csharp-KinshipFormatter

The KinshipFormatter is a genealogy relationship string formatter that takes in a relationship string (i.e. MFMB) and returns the human readable version (2nd great uncle).

The KinshipFormatter class has a single public method ( public string Format(string kinship) ) that takes the raw format string and returns the formatted output.  

To determine the final formatting, format strings are broken down using a number of rules
- Direct relationship matching
- Rule based relationship matching
- Gender insinsitivity matching
- Raw term matching
