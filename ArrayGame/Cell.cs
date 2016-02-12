using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ArrayGame
{
    public class Cell
    {
        public Label FormLabel;

        public Cell(Label formLabel)
        {
            this.FormLabel = formLabel;
        }

        public void SetData(string text, Color color) {
            this.FormLabel.Text = text;
            this.FormLabel.ForeColor = color;
        }
    }
}
