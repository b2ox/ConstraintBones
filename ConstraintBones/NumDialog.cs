using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConstraintBones
{
    public partial class NumDialog : Form
    {
        public NumDialog()
        {
            InitializeComponent();
        }
        public NumDialog(string desc, decimal minValue, decimal maxValue, decimal defValue)
        {
            InitializeComponent();
            lbDesc.Text = desc;
            numUD.Minimum = minValue;
            numUD.Maximum = maxValue;
            numUD.Value = defValue;
        }
        public decimal Value {
            get
            {
                return numUD.Value;
            }
            set
            {
                numUD.Value = value;
            }
        }
    }
}
