using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownImg
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = textBox1.Text;
            url = "http://www.qq9199.com/html/article/index30069.html";
            string page = "";
            string imgurl = "https://img.yaoyaoliao.com/";
            string urlHead = "https://m.bnmanhua.com";
            Main.GetImg(url,5, imgurl, urlHead);
      
        }
    }
}
