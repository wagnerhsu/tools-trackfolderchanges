using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Antiufo;

namespace TrackFolderChanges
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private const string Url = "http://at-my-window.blogspot.com/?page=TrackFolderChanges";

        private void lnkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Shell.StartWebPage(Url);
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            lblProductName.Text = string.Format(lblProductName.Text, Assembly.GetExecutingAssembly().GetName().Version);
            lnkSite.Text = Url;
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            Shell.StartWebPage("http://antiufo.altervista.org/donate?product=TrackFolderChanges&from=AboutBox");
  
        }
    }
}
