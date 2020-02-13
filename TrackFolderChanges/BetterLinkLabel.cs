using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace Antiufo.Controls
{

    public class BetterLinkLabel : System.Windows.Forms.LinkLabel
    {


        public BetterLinkLabel()
        {
            NormalColor = Color.FromArgb(0x0066CC);
            HoverColor = Color.FromArgb(0x3399FF);
            LinkColor = NormalColor;
        }




        [System.ComponentModel.Browsable(true)]
        public Color NormalColor { get; set; }

        [System.ComponentModel.Browsable(true)]
        public Color HoverColor { get; set; }



        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);


        [DllImport("user32.dll")]
        static extern int SetCursor(IntPtr hCursor);


        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            LinkColor = HoverColor;
        }


        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            LinkColor = NormalColor;
        }

        protected override void WndProc(ref Message msg)
        {

            if (msg.Msg == 0x20)
            {
                DefWndProc(ref msg);

                var cur = LoadCursor(IntPtr.Zero, 0x7F89);
                SetCursor(cur);
                return;
            }

            base.WndProc(ref msg);
        }




    }
}