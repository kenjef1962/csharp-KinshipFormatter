using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ACKinshipFormatter
{
    public class KinshipFormatter
    {
        private List<KinshipRule> kinshipRules;

        public KinshipFormatter()
        {
            kinshipRules = new List<KinshipRule>();
            for (int i = 1; ; i++)
            {
                var ruleString = Properties.Resources.ResourceManager.GetString("KinshipRule_" + i.ToString());
                if (ruleString == null)
                    break;

                var parts = ruleString.Split('|');
                kinshipRules.Add(new KinshipRule(parts[0], parts[1]));
            }
        }

        public string Format(string kinship)
        {
            if (IsValidTerm(kinship))
            {
                var terms = GetTerms(kinship, true);
                return FormatTerms(terms);
            }

            return Properties.Resources.KinshipUnrelated;
        }

        private string FormatReplacements(string format, int[] arg)
        {
            var result = format;

            result = result.Replace("{0:ON}", ToOrdinal(arg[0]));
            result = result.Replace("{1:ON}", ToOrdinal(arg[1]));
            result = result.Replace("{2:CN}", arg[2].ToString());
            result = result.Replace("{3:CN}", arg[3].ToString());
            result = result.Replace("{4:CN}", arg[4].ToString());

            return result;
        }

        private string FormatTerms(List<KinshipTerm> terms)
        {
            var sb = new StringBuilder();

            for (var i = terms.Count - 1; i >= 0; i--)
            {
                if (i < terms.Count - 1)
                    MakeGenitive(terms[i]);

                sb.Append(terms[i].Result);
            }

            return sb.ToString();
        }

        private int GetValidCharCount(string term, params char[] valid)
        {
            return term.Count(c => valid.Contains(c));
        }

        private string GetLookupTerm(Match m, string replacement, ref string baseTerm)
        {
            string result = null;
            var conditional = new Regex(@"(EQ|LT|GT|-?\d)\(([PMFGBZCSDEHW#\$0123456789]+)\);?");

            if (conditional.IsMatch(replacement))
            {
                // it is a conditional lookup
                var expressions = conditional.Matches(replacement);
                var operators = new Dictionary<string, string>();

                foreach (Match expr in expressions)
                {
                    operators.Add(expr.Groups[1].Value, expr.Groups[2].Value);
                }

                var upcount = 0;
                var dncount = 0;

                foreach (char c in m.Groups[0].Value)
                {
                    if (c == 'P' || c == 'M' || c == 'F') upcount++;
                    if (c == 'C' || c == 'S' || c == 'D') dncount++;
                }

                int diff = upcount - dncount;

                if (operators.ContainsKey(diff.ToString()))
                {
                    result = m.Result(operators[diff.ToString()]);
                }
                else if (diff == 0 && operators.ContainsKey("EQ"))
                {
                    result = m.Result(operators["EQ"]);
                }
                else if (diff < 0 && operators.ContainsKey("LT"))
                {
                    result = m.Result(operators["LT"]);
                }
                else if (diff > 0 && operators.ContainsKey("GT"))
                {
                    result = m.Result(operators["GT"]);
                }
                else
                {
                    result = m.Value;
                }
            }
            else
            {
                // it is a simple lookup
                result = m.Result(replacement);
            }

            var parts = result.Split('#');
            if (parts.Length > 1)
                baseTerm = parts[1];

            return result;
        }

        private List<KinshipTerm> GetTerms(string term, bool useRules)
        {
            if (string.IsNullOrEmpty(term))
                return new List<KinshipTerm>();

            var result = new List<KinshipTerm>();

            if (GetTermByDirectMatch(term, ref result))
                return result;

            if (useRules && GetTermsByRule(term, ref result))
                return result;

            if (GetTermsByMarriageBoundaries(term, ref result))
                return result;

            for (var start = 0; start < term.Length; start++)
            {
                if (GetTermByDirectMatch(term.Substring(start), ref result))
                {
                    result.InsertRange(0, GetTerms(term.Substring(0, start), false));
                    break;
                }
            }

            return result;
        }

        private bool GetTermByDirectMatch(string term, ref List<KinshipTerm> result)
        {
            // first try an exact match
            var value = Properties.Resources.ResourceManager.GetString("Kinship_" + term);
            if (value != null)
            {
                result.Add(new KinshipTerm(term, value));
                return true;
            }

            // try gender neutral (excluding last character)
            var temp1 = Ungenderize(term, term.Length - 1);

            value = Properties.Resources.ResourceManager.GetString("Kinship_" + temp1);
            if (value != null)
            {
                result.Add(new KinshipTerm(term, value));
                return true;
            }

            // try gender neutral for entire string
            var temp2 = Ungenderize(term, term.Length);

            value = Properties.Resources.ResourceManager.GetString("Kinship_" + temp2);
            if (value != null)
            {
                result.Add(new KinshipTerm(term, value));
                return true;
            }

            return false;
        }

        private bool GetTermsByRule(string term, ref List<KinshipTerm> result)
        {
            foreach (var kinshipRule in kinshipRules)
            {
                var m = Regex.Match(term, kinshipRule.Expression);
                if (m.Success)
                {
                    var matchTerm = MatchTerm(m, kinshipRule.Result);
                    if (matchTerm is KinshipTerm)
                    {
                        result.AddRange(GetTerms(term.Substring(0, m.Index), true));
                        result.Add(matchTerm as KinshipTerm);
                        result.AddRange(GetTerms(term.Substring(m.Index + m.Length), true));
                        return true;
                    }
                }
            }

            return false;
        }

        private bool GetTermsByMarriageBoundaries(string term, ref List<KinshipTerm> result)
        {
            var idx = term.LastIndexOfAny(new char[] { 'H', 'W', 'E' });
            if (idx > 0)
            {
                var left = term.Substring(0, idx);
                var right = term.Substring(idx, term.Length - idx);

                result.AddRange(GetTerms(left, true));
                result.AddRange(GetTerms(right, true));
                return true;
            }

            return false;
        }

        private bool IsValidTerm(string term)
        {
            if (string.IsNullOrEmpty(term))
                return false;

            var count = GetValidCharCount(term, 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'M', 'P', 'S', 'W', 'Z');
            return count == term.Length;
        }

        private void MakeGenitive(KinshipTerm kinshipTerm)
        {
            var matches = Regex.Matches(kinshipTerm.Result, @"\w+");

            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var repl = Properties.Resources.ResourceManager.GetString(matches[i].Value);
                if (repl != null)
                {
                    kinshipTerm.Result = Regex.Replace(kinshipTerm.Result, "^(.*)" + matches[i].Value + "(.*)$", repl);
                    return;
                }
            }

            kinshipTerm.Result = Regex.Replace(kinshipTerm.Result, "^(.*)" + kinshipTerm.Result + "(.*)$", Properties.Resources.DEFAULT);
        }

        private object MatchTerm(Match m, string replacement)
        {
            var baseTerm = m.Value;
            var lookupTerm = GetLookupTerm(m, replacement, ref baseTerm);
            var value = Properties.Resources.ResourceManager.GetString("Kinship_" + lookupTerm);

            if (value != null)
            {
                var Pn = GetValidCharCount(m.Value, 'P', 'M', 'F');                                  // number of generations up; excluding top generation if there are two common ancestors
                var Gn = m.Value.IndexOfAny(new char[] { 'G', 'B', 'Z' }) >= 0 ? 1 : 0;     // indicates whether there are two common ancestors (1 - yes; 0 - no)
                var Cn = GetValidCharCount(m.Value, 'C', 'S', 'D');                                  // number of generations down; excluding top generation if there are two common ancestors

                var BPn = GetValidCharCount(baseTerm, 'P', 'M', 'F');                                // same as Pn except we are calculating on the BASE relationship
                var BGn = baseTerm.IndexOfAny(new char[] { 'G', 'B', 'Z' }) >= 0 ? 1 : 0;   // same as Gn except we are calculating on the BASE relationship
                var BCn = GetValidCharCount(baseTerm, 'C', 'S', 'D');                                // same as Cn except we are calculating on the base relationship

                var x = Pn + Gn;                                                            // full number of generations up
                var y = Math.Abs(Pn - Cn);                                                  // difference between generations up and generations down

                var Bx = BPn + BGn;                                                         // full number of generations up except we are calculating on the base relationship
                var By = Math.Abs(BPn - BCn);                                               // difference between generations up and generations down except we are calculating on the base relationship

                var VDEGREE = (y - By) + 1;                                                 // vertical degree of the relationship (up or down); indicates removedness
                var HDEGREE = (Pn > Cn) ? (Cn - BCn) + 1 : (Pn - BPn) + 1;                  // horizontal degree; indicates how many generations to a common ancestor
                var MIN = Math.Min(Pn, Cn);                                                 // when there is a generational difference indicates the shorter leg
                var MAX = Math.Max(Pn, Cn);                                                 // when there is a generational difference indicates the longer leg
                var DIFF = Math.Abs(Pn - Cn);                                               // indicates the generational difference

                int[] parms = { VDEGREE, HDEGREE, MIN, MAX, DIFF };
                var result = FormatReplacements(value, parms);

                return new KinshipTerm(lookupTerm, result);
            }
            else
            {
                return m.Value;
            }
        }

        private string Ungenderize(string term, int numChars)
        {
            numChars = Math.Min(term.Length, numChars);

            var sb = new StringBuilder();
            for (var i = 0; i < numChars; i++)
            {
                switch (term[i])
                {
                    case 'F':
                    case 'M':
                        sb.Append('P');
                        break;

                    case 'S':
                    case 'D':
                        sb.Append('C');
                        break;

                    case 'B':
                    case 'Z':
                        sb.Append('G');
                        break;

                    case 'H':
                    case 'W':
                        sb.Append('E');
                        break;

                    default: sb.Append(term[i]);
                        break;
                }
            }

            // append the remainder as is
            if (numChars < term.Length)
                sb.Append(term.Substring(numChars));

            return sb.ToString();
        }

        private string ToOrdinal(int i)
        {
            var ordinal = i.ToString();
            var len = ordinal.Length;

            if ((ordinal[len - 1] == '1') && (i != 11))
            {
                ordinal += Properties.Resources.Ordinal_ST;
            }
            else if ((ordinal[len - 1] == '2') && (i != 12))
            {
                ordinal += Properties.Resources.Ordinal_ND;
            }
            else if ((ordinal[len - 1] == '3') && (i != 13))
            {
                ordinal += Properties.Resources.Ordinal_RD;
            }
            else
            {
                ordinal += Properties.Resources.Ordinal_TH;
            }

            return ordinal;
        }
    }
}

