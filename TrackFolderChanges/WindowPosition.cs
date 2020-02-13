using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Antiufo
{
    public static class WindowPosition
    {
        public static void InitWindowLocation(string serializedWindowPosition, Form form)
        {
            LoadWindowPosition(serializedWindowPosition, form);
            EnsureVisible(form, false);
        }


        private static void EnsureVisible(Form form, bool startFromTopLeft)
        {
            if (!Screen.AllScreens.Any(s => s.WorkingArea.Contains(new Rectangle(form.Location, form.Size))))
            {
                if (startFromTopLeft)
                {
                    Point desktopLocation = new Point(10, 10);
                    form.DesktopLocation = desktopLocation;
                }
                else
                {
                    CenterToScreen(form);
                }
            }
        }


        private static void CenterToScreen(Form form)
        {
            Point location = default(Point);
            Screen screen = Screen.FromControl(form);

            Rectangle workingArea = screen.WorkingArea;
            location.X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - form.Width) / 2);
            location.Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - form.Height) / 2);
            form.Location = location;
        }

        public static void CopyWindowPosition(Form modelForm, Form form)
        {
            if (modelForm.WindowState == FormWindowState.Normal && !IsSnapped(modelForm))
            {
                form.StartPosition = FormStartPosition.Manual;
                Point pos = modelForm.Location;
                pos.X += 20;
                pos.Y += 20;
                form.Location = pos;
                form.Size = modelForm.Size;
                EnsureVisible(form, true);
                form.WindowState = FormWindowState.Normal;
            }
            else if (modelForm.WindowState == FormWindowState.Maximized)
            {
                form.StartPosition = FormStartPosition.Manual;
                form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                form.WindowState = FormWindowState.Normal;
            }
        }


        private static bool IsSnapped(Form form)
        {
            if (form.WindowState != FormWindowState.Normal) return false;
            if (form.DesktopBounds.Top != 0) return false;

            return (form.DesktopBounds.Left == 0 ||
                    form.DesktopBounds.Right == Screen.GetWorkingArea(form).Right);
        }



        private static void LoadWindowPosition(string serializedWindowPosition, Form form)
        {
            if (string.IsNullOrEmpty(serializedWindowPosition))
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            string[] array = serializedWindowPosition.Split(' ');
            FormWindowState windowState = (FormWindowState)Convert.ToInt32(array[0]);
            if (windowState == FormWindowState.Maximized)
            {
                form.WindowState = FormWindowState.Maximized;
            }
            else if (windowState == FormWindowState.Normal)
            {
                form.StartPosition = FormStartPosition.Manual;
                Point location = new Point(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
                form.Location = location;
                Size size = new Size(Convert.ToInt32(array[3]), Convert.ToInt32(array[4]));
                form.Size = size;
                form.WindowState = FormWindowState.Normal;
            }
            else
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                form.WindowState = FormWindowState.Normal;
            }

        }


        public static string SerializePosition(Form form)
        {
            if (IsSnapped(form)) return string.Empty;

            return string.Join(" ", new[]{
    
                Convert.ToString((int)form.WindowState),
                Convert.ToString(form.Left),
                Convert.ToString(form.Top),
                Convert.ToString(form.Width),
                Convert.ToString(form.Height)
            });


        }
    }
}
