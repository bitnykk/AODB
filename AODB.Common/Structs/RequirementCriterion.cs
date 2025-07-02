using AODB.Common.Enums;
using System;

namespace AODB.Common.Structs
{
    public struct RequirementCriterion
    {
        public int Stat;
        public uint Value;
        public Operator Operator;

        public RequirementCriterion(int stat, uint value, Operator op)
        {
            Stat = stat;
            Value = value;
            Operator = op;
        }
    }
}