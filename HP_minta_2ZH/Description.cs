using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP_minta_2ZH
{
    class DescriptionAttribute : Attribute
    {
        string text;

        public DescriptionAttribute(string text)

        {

            this.Text = text;

        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
            }
        }
    }
}
