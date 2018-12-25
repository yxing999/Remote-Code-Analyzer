///////////////////////////////////////////////////////////////////////////
// DepAnalysis.cs - get a graph of dependency information                //
// ver 1.1                                                               //
// Yuxuan Xing, CSE681 - Software Modeling and Analysis, Spring 2018     //
///////////////////////////////////////////////////////////////////////////
/*
 * Module operation:
 * -----------------------------------------------------------------------
 * This package defined a class DepAnalys, with the function build graph, connect node,
 * Build a graph represent the dependency information of the fils.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * parser.cs
 * CsGraph.cs
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
using TypeT;
using CodeAnalysis;
using Lexer;


namespace DepAnalysis
{
    using File = String;
    using Token = String;
    public class DepAnalys
    {
        public CsGraph<string, string> csgraph=new CsGraph<string, string>("depgraph");

        DepAnalys instance_;

        public DepAnalys()
        {
            instance_ = this;
        }
        //------------<build graph's nodes>--------------------------------
        public void BuildGraph(List<File> files)
        {
            foreach(File file in files)
            {
                string filename= file.Substring(file.LastIndexOf('\\') + 1);
                CsNode<string, string> node = new CsNode<string, string>(filename);
                csgraph.addNode(node);
            }
            if (csgraph.adjList.Count > 0)
            {
                csgraph.startNode = csgraph.adjList[0];
            }
        }

        //----------------<Analyze dependency>---------------------------------------
        public void ConnectNode(TypeAnalysis typea, string fqf)
        {
            List<string> namestore = new List<string>();
            string filename = fqf.Substring(fqf.LastIndexOf('\\') + 1);
            CsNode<string, string> node = csgraph.findNode(filename);
            Toker toker = new Toker();
            toker.doReturnComments = false;
            if (!toker.open(fqf))
            {
                Console.Write("\n can't open {0}\n", fqf);
            }
            //else
            //{
            //    Console.Write("\n  processing file: {0}\n", fqf);
            //}
            while (!toker.isDone())
            {
                Token tok = toker.getTok();
                if (tok == null) continue;
                if (typea.typetable_.table.ContainsKey(tok))//the key exist in the type table.
                {
                    if (typea.typetable_.table[tok][0].namesp == "")
                    {
                        namestore.Add(tok);
                    }
                    else
                    {
                        List<TypeItem> list_it = typea.typetable_.table[tok];
                        foreach (TypeItem it in list_it)
                        {
                            if (namestore.Contains(it.namesp))
                            {
                                //connect the node
                                node.addChild(csgraph.findNode(it.file), "");
                            }
                        }
                    }
                }
            }
            toker.close();
        }
            
        public void showdep()
        {
            foreach(CsNode<string,string> node in csgraph.adjList)
            {
                Console.Write("\n Package : {0} \n", node.name);
                Console.Write("Dependency : [");
                foreach(CsEdge<string,string> edge in node.children)
                {
                    Console.Write(" {0} ", edge.targetNode.name);
                }
                Console.Write("]");
            }
        }
#if (TEST_DEP)
        class TestDep
        {
            static void Main(string[] args)
            {
                Console.Write("\n  Demonstrating DepAnalysis");
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

                foreach(string f in files)
                {
                    danalysis.ConnectNode(typeanalysis,f);
                }

                danalysis.showdep();
                Console.Write("\n\n");
                
            }
        }
#endif 
    }
}
