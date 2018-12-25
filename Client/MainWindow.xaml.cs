////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App //
// ver 2.0                                                                //
// Yuxuan Xing, CSE681 - Software Modeling and Analysis, Fall 2018        //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 03 Dex 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MessagePassingComm;

namespace Navigator
{
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;
        List<string> SelectedFiles = new List<string>();
        List<string> SelectedDirs = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            initializeEnvironment();
            Console.Title = "Navigator Client";
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            getTopFiles();
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            Demo456();
        }
        //----< make Environment equivalent to ClientEnvironment >-------

        void initializeEnvironment()
        {
            Environment.root = ClientEnvironment.root;
            Environment.address = ClientEnvironment.address;
            Environment.port = ClientEnvironment.port;
            Environment.endPoint = ClientEnvironment.endPoint;
        }
        //----< define how to process each message command >-------------

        void initializeMessageDispatcher()
        {
            // load remoteFiles listbox with files from root

            messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from root

            messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };
            // load remoteFiles listbox with files from folder

            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from folder

            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };
            //post the result to local screen
            messageDispatcher["analyzeFiles"] = (CommMessage msg) =>
            {
                Result.Text += "\n ==================A new analysis result:================\n";
                foreach (string s in msg.arguments)
                {
                    Result.Text += s;
                }
            };
            messageDispatcher["demo456"] = (CommMessage msg) =>
            {
                Result.Text += "\n Req 456 : \n Demostrate Dependency Analysis and Strong Components";
                Result.Text += "\n ========================================\n";
            };
        }
        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        //----< shut down comm when the main window closes >-------------

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            // The step below should not be nessary, but I've apparently caused a closing event to 
            // hang by manually renaming packages instead of getting Visual Studio to rename them.

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        //----< show files and dirs in root path >-----------------------

        public void getTopFiles()
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            localFiles.Items.Clear();
            foreach (string file in files)
            {
                localFiles.Items.Add(file);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();
            localDirs.Items.Clear();
            foreach (string dir in dirs)
            {
                localDirs.Items.Add(dir);
            }
        }
        //----< move to directory root and display files and subdirs >---

        private void localTop_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }
        //----< show selected file in code popup window >----------------

        private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = localFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        //----< move to parent directory and show files and subdirs >----

        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        //----< move into subdir and show files and subdirs >------------

        private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string dirName = localDirs.SelectedValue as string;
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles();
        }
        //----< move to root of remote directories >---------------------
        /*
         * - sends a message to server to get files from root
         * - recv thread will create an Action<CommMessage> for the UI thread
         *   to invoke to load the remoteFiles listbox
         */
        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Yuxuan Xing";
            msg1.command = "getTopFiles";
            msg1.arguments.Add("");
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopDirs";
            comm.postMessage(msg2);
        }
        //----< download file and display source in popup window >-------

        private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // coming soon

        }
        //----< move to parent directory of current remote path >--------

        private void RemoteUp_Click(object sender, RoutedEventArgs e)
        {
            // coming soon
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Yuxuan Xing";
            msg1.command = "getTopFiles";
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopDirs";
            comm.postMessage(msg2);
        }
        //----< move into remote subdir and display files and subdirs >--
        /*
         * - sends messages to server to get files and dirs from folder
         * - recv thread will create Action<CommMessage>s for the UI thread
         *   to invoke to load the remoteFiles and remoteDirs listboxs
         */
        private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveIntoFolderFiles";
            msg1.arguments.Add(remoteDirs.SelectedValue as string);
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderDirs";
            comm.postMessage(msg2);
        }

        //---------------<find all .cs files in a directory>--------------------------
        public void FindFile(string dir,ref List<string> files)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir, "*.cs"))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    FindFile(d,ref files);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

        }

        //------------<event analyze local files>------------------
        private void LocalfileAna_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg = new CommMessage(CommMessage.MessageType.request);
            msg.from = ClientEnvironment.endPoint;
            msg.to = ServerEnvironment.endPoint;
            msg.author = "Yuxuan Xing";
            msg.command = "analyzeFiles";
            foreach(string s in SelectedFiles)
            {
                string str = ClientEnvironment.root + s;
                msg.arguments.Add(str);
            }
            comm.postMessage(msg);
        }
        //------------<event analyze remote files>------------------
        private void RemotefileAna_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg = new CommMessage(CommMessage.MessageType.request);
            msg.from = ClientEnvironment.endPoint;
            msg.to = ServerEnvironment.endPoint;
            msg.author = "Yuxuan Xing";
            msg.command = "analyzeFiles";
            foreach (string s in SelectedFiles)
            {
                string str = ServerEnvironment.root + s;
                msg.arguments.Add(str);
            }
            comm.postMessage(msg);
        }
        //------------<event analyze local Directories>------------------
        private void LocalDirAna_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg = new CommMessage(CommMessage.MessageType.request);
            msg.from = ClientEnvironment.endPoint;
            msg.to = ServerEnvironment.endPoint;
            msg.author = "Yuxuan Xing";
            msg.command = "analyzeFiles";
            List<string> temp = new List<string>();
            foreach(string s in SelectedDirs)
            {
                string str = ClientEnvironment.root + s;
                FindFile(str, ref temp);
            }
            foreach(string s in temp)
            {
                msg.arguments.Add(s);
            }
            comm.postMessage(msg);
        }

        //------------<event analyze remote directories>------------------
        private void RemoteDirAna_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg = new CommMessage(CommMessage.MessageType.request);
            msg.from = ClientEnvironment.endPoint;
            msg.to = ServerEnvironment.endPoint;
            msg.author = "Yuxuan Xing";
            msg.command = "analyzeFiles";
            List<string> temp = new List<string>();
            foreach (string s in SelectedDirs)
            {
                string str = ServerEnvironment.root + s;
                FindFile(str, ref temp);
            }
            foreach (string s in temp)
            {
                msg.arguments.Add(s);
            }
            comm.postMessage(msg);
        }

        //----------------<event add and remove items based on checkedbox>--------------------------------
        private void CheckBox_Checked_localFiles(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedFiles.Add(temp);
        }

        private void CheckBox_Unchecked_localFiles(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedFiles.Remove(temp);
        }

        private void CheckBox_Checked_localDirs(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedDirs.Add(temp);
        }

        private void CheckBox_Unchecked_localDirs(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedDirs.Remove(temp);
        }
        private void CheckBox_Checked_RemoteFiles(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedFiles.Add(temp);
        }

        private void CheckBox_Unchecked_RemoteFiles(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedFiles.Remove(temp);
        }

        private void CheckBox_Checked_RemoteDirs(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedDirs.Add(temp);
        }

        private void CheckBox_Unchecked_RemoteDirs(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string temp = check.Content.ToString();
            SelectedDirs.Remove(temp);
        }

        public void Demo456()
        {
            CommMessage autotest = new CommMessage(CommMessage.MessageType.request);
            autotest.from = ClientEnvironment.endPoint;
            autotest.to = ServerEnvironment.endPoint;
            autotest.author = "Yuxuan Xing";
            autotest.command = "demo456";
            autotest.arguments.Add("");
            comm.postMessage(autotest);

            CommMessage msg = new CommMessage(CommMessage.MessageType.request);
            msg.from = ClientEnvironment.endPoint;
            msg.to = ServerEnvironment.endPoint;
            msg.author = "Yuxuan Xing";
            msg.command = "analyzeFiles";
            List<string> temp = new List<string>();
            FindFile("../../../parser", ref temp);
            foreach (string s in temp)
            {
                msg.arguments.Add(s);
            }
            comm.postMessage(msg);
        }
    }
}
