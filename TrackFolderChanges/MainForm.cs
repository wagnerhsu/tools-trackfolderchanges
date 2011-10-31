using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Antiufo;

namespace TrackFolderChanges
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            icons = new IconsHandler(true, false);
            treeView1.ImageList = icons.SmallIcons;
        }

        IconsHandler icons;

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = edtFolder.Text;
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                TryUpdateTree(folderBrowserDialog.SelectedPath);
            }
        }


        private void UpdateTree(string rootFolder)
        {
            fileSystemWatcher.EnableRaisingEvents = false;

            rootFolder = rootFolder.Trim('"');

            if (rootFolder.Length == 2 && rootFolder[1] == ':') rootFolder += '\\';
            rootFolder = Path.GetFullPath(rootFolder);
            if (rootFolder.Length > 3) rootFolder = rootFolder.Trim('\\');
            rootFolder = char.ToUpper(rootFolder[0]) + rootFolder.Substring(1);

            this.rootFolder = rootFolder;
            edtFolder.Text = rootFolder;
            this.nodes = new Dictionary<string, ChangedFolder>();

            fileSystemWatcher.Path = rootFolder;
            fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(CreateNode(rootFolder).Node);
            fileSystemWatcher.EnableRaisingEvents = true;
        }


        private ChangedFolder CreateNode(string path)
        {
            var name = Path.GetFileName(path);
            if (path.Equals(rootFolder, StringComparison.CurrentCultureIgnoreCase))
                name = path;
            var folder = new ChangedFolder(path, new TreeNode(name));
            nodes[path.ToLower()] = folder;
            folder.Node.ExpandAll();
            folder.Node.SelectedImageIndex = folder.Node.ImageIndex = icons.GetIcon(path);
            return folder;
        }


        private Dictionary<string, ChangedFolder> nodes;
        private string rootFolder;




        private ChangedFolder GetOrCreateNode(string path, WatcherChangeTypes changeType)
        {
            ChangedFolder folder;
            var lowerCaseName = Path.GetFullPath(path).ToLower();
            if (lowerCaseName == rootFolder.ToLower()) return (ChangedFolder)treeView1.Nodes[0].Tag;
            if (!nodes.TryGetValue(lowerCaseName, out folder))
            {
                var parentNode = GetOrCreateNode(Path.GetDirectoryName(path), WatcherChangeTypes.Changed);
                folder = CreateNode(path);
                parentNode.Node.Nodes.Add(folder.Node);
                if (parentNode.Node.Nodes.Count == 1)
                    parentNode.Node.ExpandAll();
            }
            if (changeType == WatcherChangeTypes.Deleted) folder.MarkAllAsDeleted();

            if (!(changeType == WatcherChangeTypes.Changed && folder.Status == WatcherChangeTypes.Created))
            {
                var isDirectory = Directory.Exists(path);
                folder.Status = changeType;
            }
            return folder;
        }




        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            GetOrCreateNode(e.FullPath, e.ChangeType);
        }

        private void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            GetOrCreateNode(e.FullPath, e.ChangeType);
        }

        private void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            GetOrCreateNode(e.FullPath, e.ChangeType);
        }

        private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            GetOrCreateNode(e.OldFullPath, WatcherChangeTypes.Deleted);
            GetOrCreateNode(e.FullPath, WatcherChangeTypes.Created);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TryUpdateTree(Environment.GetEnvironmentVariable("SystemDrive"));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            TryUpdateTree(rootFolder);
        }

        private void edtFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TryUpdateTree(edtFolder.Text);
            }
        }

        private void TryUpdateTree(string path)
        {
            try
            {
                UpdateTree(path);
            }
            catch (Exception ex)
            {
                Program.ReportError(this, ex);
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenItem((ChangedFolder)e.Node.Tag);
        }

        private static void OpenItem(ChangedFolder folder)
        {
            if (Directory.Exists(folder.Path))
            {
                Shell.StartFolder(folder.Path);
            }
            else
            {
                Shell.OpenFolderAndSelectItem(folder.Path);
            }
        }




    }

}
