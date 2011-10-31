using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace TrackFolderChanges
{
    public class ChangedFolder
    {
        public ChangedFolder(string path, TreeNode node)
        {
            this.Path = path;
            this.Node = node;
            node.Tag = this;
        }

        public string Path { get; private set; }

        private WatcherChangeTypes _status;
        public WatcherChangeTypes Status
        {
            set
            {
                _status = value;
                Node.BackColor = GetColorForChangeType(value);
            }
            get
            {
                return _status;
            }
        }
        public TreeNode Node { get; private set; }


        public void MarkAllAsDeleted()
        {
            Status = WatcherChangeTypes.Deleted;
            foreach (TreeNode item in Node.Nodes)
            {
                ((ChangedFolder)item.Tag).MarkAllAsDeleted();
            }
        }

        public bool IsFolder
        {
            get
            {
                return Directory.Exists(Path);
            }
        }

        private Color GetColorForChangeType(WatcherChangeTypes changeType)
        {
            switch (changeType)
            {
                case WatcherChangeTypes.Changed: return IsFolder ? Color.Empty : Color.LightBlue;
                case WatcherChangeTypes.Created: return Color.LightGreen;
                case WatcherChangeTypes.Deleted: return Color.Wheat;
                default: return Color.Empty;
            }
        }

    }
}
