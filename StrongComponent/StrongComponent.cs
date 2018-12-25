///////////////////////////////////////////////////////////////////////////
// StrongComponent.cs - find strong components of a graph                //
// ver 1.1                                                               //
// Yuxuan Xing, CSE681 - Software Modeling and Analysis, Spring 2018     //
///////////////////////////////////////////////////////////////////////////
/*
 * Module operation:
 * -----------------------------------------------------------------------
 * This package consists of functions FindConnect, Tarjan,
 * Find all the strong components of the DepAnalysis.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * parser.cs
 * CsGraph.cs
 * DepAnalysis.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1,0 : 29 Oct 2018
 * - first release
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAnalys;
using CodeAnalysis;
using Lexer;
using DepAnalysis;

namespace StrongComponent
{
    using File = String;
    public class StrongComp
    {
        static StrongComp strongcomponent_;

        public StrongComp()
        {
            strongcomponent_ = this;
        }
        //result
        public List<List<CsNode<string, string>>> result = new List<List<CsNode<string, string>>>();
        //helper stack
        public Stack<CsNode<string, string>> st = new Stack<CsNode<string, string>>();

        public static int index=0;

        //-------------------<find the strongcomponent from the graph>------------------------------
        public void FindConnect(CsGraph<string,string> graph)
        {
            foreach(CsNode<string,string> node in graph.adjList)
            {
                if (node.visited == false)
                {
                    Tarjan(node);
                }
            }
        }
        
        //----------------<implementation of Targan algorithm>----------------------------------
        public void Tarjan(CsNode<string,string> node)
        {
            node.Dfn = index;
            node.Low = index;
            index++;

            st.Push(node);
            node.visited = true;

            foreach(CsEdge<string,string> n in node.children)
            {
                if (n.targetNode.visited == false)
                {
                    Tarjan(n.targetNode);
                    node.Low = Math.Min(node.Low,n.targetNode.Low); 
                }
                else if(st.Contains(n.targetNode))
                {
                    node.Low = Math.Min(node.Low,n.targetNode.Dfn);
                }
            }
            if (node.Dfn == node.Low)
            {
                List<CsNode<string, string>> temp = new List<CsNode<string, string>>();
                CsNode<string, string> top;
                do
                {
                    top = st.Pop();
                    temp.Add(top);
                } while (node!=top);

                result.Add(temp);
            }
        }

        public void ShowSC()
        {
            int line = 1;
            foreach(List<CsNode<string,string>> nodes in result)
            {
                Console.Write("\n Component {0}: ",line);
                line++;
                foreach(CsNode<string,string> node in nodes)
                {
                    Console.Write(node.name+"  ");
                }
            }
        }
#if (TEST_SC)
        class TestSC
        {
            static void Main(string[] args)
            {
                Console.Write("\n  Demonstrating Strong Component");
                Console.Write("\n ======================\n");

                TypeAnalysis typeanalysis = new TypeAnalysis();

                List<string> files = TestParser.ProcessCommandline(args);

                foreach (string file in files)
                {
                    // Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

                    ITokenCollection semi = Factory.create();
                    //semi.displayNewLines = false;

                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", args[0]);
                        return;
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
                    List<Elem> table = rep.locations;
                    File f = file.Substring(file.LastIndexOf('\\') + 1);
                    string namesp = "";
                    foreach (Elem ele in table)
                    {
                        if (ele.type == "namespace")
                        {
                            namesp = ele.name;
                        }
                        typeanalysis.add(f, ele, namesp);
                    }
                    Console.Write("\n");

                    semi.close();
                }
                //build the graph
                
                DepAnalys danalysis = new DepAnalys();
                danalysis.BuildGraph(files);

                foreach (string f in files)
                {
                    danalysis.ConnectNode(typeanalysis, f);
                }
                
                StrongComp strongcomp = new StrongComp();
                strongcomp.FindConnect(danalysis.csgraph);
                strongcomp.ShowSC();

                Console.Write("\n\n");

            }
        }
#endif
    }
}
