using Antiufo;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TrackFolderChanges
{
    public partial class MainForm : Form
    {
        private static ILogger Logger = LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            InitializeComponent();
            icons = new IconsHandler(true, false);
            treeView.ImageList = icons.SmallIcons;

            WindowPosition.InitWindowLocation(Program.ReadSetting("WindowPosition", null), this);
        }

        private IconsHandler icons;

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
            if (rootFolder.Length > 3) rootFolder = rootFolder.TrimEnd('\\');
            rootFolder = char.ToUpper(rootFolder[0]) + rootFolder.Substring(1);

            this.rootFolder = rootFolder;
            edtFolder.Text = rootFolder;
            this.nodes = new Dictionary<string, ChangedFolder>();

            fileSystemWatcher.Path = rootFolder;
            fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;

            treeView.Nodes.Clear();

            var folder = CreateNode(rootFolder).Node;
            folder.Tag = new ChangedFolder(rootFolder, folder);
            treeView.Nodes.Add(folder);
            fileSystemWatcher.EnableRaisingEvents = true;

            Program.WriteSetting("LastFolder", rootFolder);
            treeView.Focus();
        }

        private ChangedFolder CreateNode(string path)
        {
            var oldTopNode = treeView.TopNode;
            var name = Path.GetFileName(path);
            if (path.Equals(rootFolder, StringComparison.CurrentCultureIgnoreCase))
                name = path;
            var folder = new ChangedFolder(path, new TreeNode(name));
            nodes[path.ToLower()] = folder;
            folder.Node.ExpandAll();
            folder.Node.SelectedImageIndex = folder.Node.ImageIndex = icons.GetIcon(path);
            treeView.TopNode = oldTopNode;
            return folder;
        }

        private Dictionary<string, ChangedFolder> nodes;
        private string rootFolder;

        private ChangedFolder GetOrCreateNode(string path, WatcherChangeTypes changeType)
        {
            Logger.Debug($"{changeType}:{path}");
            ChangedFolder folder;
            var lowerCaseName = Path.GetFullPath(path).ToLower();
            if (lowerCaseName == rootFolder.ToLower()) return (ChangedFolder)treeView.Nodes[0].Tag;
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
            var defaultFolder = Environment.GetEnvironmentVariable("SystemDrive");
            var folder = Program.ReadSetting("LastFolder", defaultFolder);
            if (Directory.Exists(folder)) TryUpdateTree(folder);
            else TryUpdateTree(defaultFolder);
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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                TryUpdateTree(rootFolder);
            }
            if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                using (var form = new AboutForm())
                    form.ShowDialog(this);
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node != null)
            {
                treeView.SelectedNode = e.Node;
                cmdOpen.Enabled = File.Exists(SelectedFolder.Path) || Directory.Exists(SelectedFolder.Path);
                var parent = Directory.GetParent(SelectedFolder.Path);
                cmdOpenLocation.Enabled = parent != null && parent.Exists;
                contextMenu1.Show(treeView, e.Location);
            }
        }

        private void cmdCopyPath_Click(object sender, EventArgs e)
        {
            Clipboard.SetText("\"" + SelectedFolder.Path + "\"");
        }

        private void cmdOpenLocation_Click(object sender, EventArgs e)
        {
            var path = SelectedFolder.Path;
            if (File.Exists(path) || Directory.Exists(path))
            {
                Shell.OpenFolderAndSelectItem(path);
            }
            else
            {
                Shell.StartFolder(Directory.GetParent(path).FullName);
            }
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            Shell.StartFile(SelectedFolder.Path);
        }

        private ChangedFolder SelectedFolder
        {
            get
            {
                if (treeView.SelectedNode == null) return null;
                return (ChangedFolder)treeView.SelectedNode.Tag;
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            using (var form = new AboutForm())
                form.ShowDialog(this);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.WriteSetting("WindowPosition", WindowPosition.SerializePosition(this));
        }
    }
}