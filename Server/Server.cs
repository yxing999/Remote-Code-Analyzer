/////////////////////////////////////////////////////////////////////
// Server.cs - Build the server for codeanalyze system             //
// ver 1.0   grammatical analysis                                  //
//                                                                 //
// Yuxuan Xing, CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements the server side of Remote Package Dependency Analysis.
 * Including initial state and message handling functions.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * parser.cs
 * CsGraph.cs
 * DepAnalysis.cs
 * Strongcomponent.cs
 * IMessagePassingCommService.cs
 * MessagePassingCommService.cs
 * 
 * Maintenance History
 * -------------------
 * ver 1.0 : 03 Dec 2018
 * - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using Lexer;
using TypeT;
using TypeAnalys;
using DepAnalysis;
using StrongComponent;
using CodeAnalysis;

namespace Navigator
{
    using File = String;
    public class Server
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;

        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        public Server()
        {
            initializeEnvironment();
            Console.Title = "Navigator Server";
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            Environment.root = ServerEnvironment.root;
            Environment.address = ServerEnvironment.address;
            Environment.port = ServerEnvironment.port;
            Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher()
        {
            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopFiles"] = getTopFiles;

            Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopDirs"] = getTopDirs;

            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;

            Func<CommMessage, CommMessage> analyzeFiles = (CommMessage msg) =>
            {
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "analyzeFiles";
                TypeAnalysis typeana = new TypeAnalysis();
                DepAnalys depana = new DepAnalys();
                StrongComp strongcomp = new StrongComp();
                List<Elem> table = new List<Elem>();
                foreach (string file in msg.arguments)
                {
                    ITokenCollection semi = Factory.create();

                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", file);
                        return null;
                    }
                    BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                    Parser parser = builder.build();
                    try
                    {
                        while (semi.get().Count > 0)
                            parser.parse(semi);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n\n  {0}\n", ex.Message);
                    }
                    Repository rep = Repository.getInstance();
                    table = rep.locations;
                    File f = file.Substring(file.LastIndexOf('\\') + 1);
                    string namesp = "";
                    foreach (Elem ele in table)
                    {
                        if (ele.type == "namespace")
                        {
                            namesp = ele.name;
                        }
                        typeana.add(f, ele, namesp);
                    }
                    semi.close();
                }
                depana.BuildGraph(msg.arguments);
                foreach (string f in msg.arguments)
                {
                    depana.ConnectNode(typeana, f);
                }
                //move to reply.arguments[0];
                foreach(CsNode<string, string> node in depana.csgraph.adjList)
                {
                    StringBuilder temp = new StringBuilder();
                    temp.Append(node.name.Substring(node.name.LastIndexOf('/') + 1));
                    temp.Append(" \n Dependency : [");
                    foreach (CsEdge<string, string> edge in node.children)
                    {
                        temp.Append(edge.targetNode.name.Substring(edge.targetNode.name.LastIndexOf('/') + 1));
                        temp.Append(" ");
                    }
                    temp.Append(" ] \n");
                    reply.arguments.Add(temp.ToString());
                }
                strongcomp.FindConnect(depana.csgraph);
                //move to reply.arguments[1];
                int i = 1;
                foreach (List<CsNode<string, string>> nodes in strongcomp.result)
                {
                    StringBuilder temp = new StringBuilder();
                    temp.Append("Components "+i.ToString()+": ");
                    foreach (CsNode<string, string> node in nodes)
                    {
                        temp.Append(node.name.Substring(node.name.LastIndexOf('/')+1)+" ");
                    }
                    temp.Append("\n");
                    i++;
                    reply.arguments.Add(temp.ToString());
                }
                return reply;
            };
            messageDispatcher["analyzeFiles"] = analyzeFiles;

            Func<CommMessage, CommMessage> demo456 = (CommMessage msg) =>
            {
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "demo456";
                reply.arguments.Add("");
                return reply;
            };
            messageDispatcher["demo456"] = demo456;
        }
        /*----< Server processing >------------------------------------*/
        /*
         * - all server processing is implemented with the simple loop, below,
         *   and the message dispatcher lambdas defined above.
         */
        static void Main(string[] args)
        {
            TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                Server server = new Server();
                server.initializeDispatcher();
                server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);

                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    reply.show();
                    server.comm.postMessage(reply);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }
        }
    }
}
