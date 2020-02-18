using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Hecatomb
{
    public class TextFile
    {

        public static void ShowFile()
        {
            Application.Run(new Form1());
            //var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            //System.IO.Directory.CreateDirectory(path + @"\logs");
            //System.IO.File.WriteAllText(path + @"\logs\" + "testing" + ".txt", "yeah this is some text alright");
            //Process.Start(path + @"\logs\" + "testing" + ".txt");
        }
    }

    public partial class Form1 : Form
    {
        private TextBox textBox1;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            textBox1.Width = 250;
            textBox1.Height = 50;
            textBox1.Text = "this is text";
        }
    }
}
