// (c) Vasian Cepa 2005

using System;
using System.Collections;

namespace ns
{


    public class NumericComparer : IComparer
    {
        public NumericComparer()
        { }
        public int Compare(object x, object y)
        {
            if ((x is string) && (y is string))
            {
                return StringLogicalComparer.Compare((string)x, (string)y);
            }
            return -1;
        }
    }
}

