using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVConvertor.Domain
{
    public class SpecialProcessAttribute: Attribute
    {
        private string _ProcessName;

        public SpecialProcessAttribute(String processName)
        {
            this._ProcessName = processName;
        }

        public string ProcessName
        {
            get
            {
                return _ProcessName;
            }
        }
    }
}
