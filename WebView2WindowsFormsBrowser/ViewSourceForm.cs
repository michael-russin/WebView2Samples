using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebView2WindowsFormsBrowser
{
    public partial class ViewSourceForm : Form
    {
        public ViewSourceForm(string source) : this()
        {
            this.richTextBox1.Text = source;
        }

        public ViewSourceForm()
        {
            InitializeComponent();
        }
    }
}
