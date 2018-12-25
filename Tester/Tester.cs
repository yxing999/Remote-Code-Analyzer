///////////////////////////////////////////////////////////////////////////
// Tester.cs - Automatic test unit for the project                       //
// ver 1.1                                                               //
// Yuxuan Xing, CSE681 - Software Modeling and Analysis, Spring 2018     //
///////////////////////////////////////////////////////////////////////////
/*
 * Module operation:
 * -----------------------------------------------------------------------
 * This package provide the auto test class for the requirement.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * parser.cs
 * CsGraph.cs
 * DepAnalysis.cs
 * Strongcomponent.cs
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
using System.IO;
using TypeAnalys;
using CodeAnalysis;
using Lexer;
using DepAnalysis;
using StrongComponent;

namespace Tester
{
    using File = String;
    class Test
    {
        //Elements to construct and test
        public TypeAnalysis typeana = new TypeAnalysis();
        public DepAnalys depana = new DepAnalys();
        public StrongComp strongcomp = new StrongComp();
        public List<string> files = new List<string>();
        public List<Elem> table = new List<Elem>();

        //---------------<find all .cs files in a directory>--------------------------
        public void FindFile(string dir)
        {
            try
            {
                foreach(string f in Directory.GetFiles(dir, "*.cs"))
                {
                    files.Add(f);
                }
                foreach(string d in Directory.GetDirectories(dir))
                {
                    FindFile(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

        }
        //-------------<Build the Typetable for analysis>--------------------------
        public void Testpre(string[] args)
        {
            Console.Write("\n\n  Construct the TypeTable by semi and parser");
            Console.Write("\n =========================================\n");

            FindFile(args[0]);

            foreach (string file in files)
            {
                ITokenCollection semi = Factory.create();

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
                table=rep.locations;
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
            typeana.display();
        }

        //---------------< Requirement 3 >---------------------------------
        public void TestReq3()
        {
            Console.Write("\n Req 3 : \n Demostrate package information");
            Console.Write("\n ========================================\n");

            string path = "../../";

            void onFile(string filename)
            {
                Console.Write("\n    {0}", filename);
            }
            void onDir(string dirname)
            {
                Console.Write("\n  {0}", dirname);
            }
            FileUtilities.Navigate nav = new FileUtilities.Navigate();
            nav.Add("*.cs");
            nav.newDir += new FileUtilities.Navigate.newDirHandler(onDir);
            nav.newFile += new FileUtilities.Navigate.newFileHandler(onFile);
            path = "../../../TypeTable";
            nav.go(path, false);
            path = "../../../TypeAnalysis";
            nav.go(path, false);
            path = "../../../DepAnalysis";
            nav.go(path, false);
            path = "../../../StrongComponent";
            nav.go(path, false);
        }

        //---------------< Requirement 4 >---------------------------------
        public void TestReq4()
        {
            Console.Write("\n Req 4 : \n Demostrate dependency analysis");
            Console.Write("\n ========================================\n");

            depana.BuildGraph(files);

            foreach (string f in files)
            {
                depana.ConnectNode(typeana, f);
            }

            depana.showdep();
            Console.Write("\n");
        }

        //---------------< Requirement 5 >---------------------------------
        public void TestReq5(string[] args)
        {
            Console.Write("\n Req 5 : \n Demostrate uesr-defined types");
            Console.Write("\n ========================================\n");
            

            foreach (string file in files)
            {
                ITokenCollection semi = Factory.create();

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
                table = rep.locations;
                Console.Write("\n");
                File f = file.Substring(file.LastIndexOf('\\') + 1);
                Console.Write("Processing file: " + f + "\n");
                Display.showMetricsTable(table);
                semi.close();
            }

            Console.Write("\n");
        }

        //---------------< Requirement 6 >---------------------------------
        public void TestReq6()
        {
            Console.Write("\n Req 6 : \n Demostrate strong components ");
            Console.Write("\n ========================================\n");

            strongcomp.FindConnect(depana.csgraph);
            strongcomp.ShowSC();
            Console.Write("\n");
        }

        //---------------< Requirement 7 >---------------------------------
        public void TestReq7()
        {
            Console.Write("\n Req 7 : \n Well formatted output");
            Console.Write("\n ========================================\n");

            Console.Write("\n  Demonstrated by the output of Req3,Req4,Req5,Req6 test");
        }

        //---------------< Requirement 8 >---------------------------------
        public void TestReq8()
        {
            Console.Write("\n Req 8 : \n This package is an autotest");
            Console.Write("\n ========================================\n");

            Console.Write("\n  Demonstrated by the output of Req3,Req4,Req5,Req6 test");
        }
        
#if(AUTO_TEST)
        static void Main(string[] args)
        {
            Test test = new Test();
            test.TestReq3();

            test.Testpre(args);

            test.TestReq4();
            test.TestReq5(args);
            test.TestReq6();

            test.TestReq7();
            test.TestReq8();
            System.Console.ReadKey();
        }
    }
#endif
}
