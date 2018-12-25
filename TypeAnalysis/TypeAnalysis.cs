/////////////////////////////////////////////////////////////////////
// TypeAnalysis.cs - Project #3                                    //
//                                                                 //
// Yuxuan Xing, CSE 681 - Software Modeling and Analysis, Fall 2018//
/////////////////////////////////////////////////////////////////////

/*
 * Package Operations:
 * -------------------
 * This package provide a class typeAnalysis, with a TypeTable and function add,
 * it is used to add parser's result to typetable.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * Parser.cs
 * 
 * Maintenance History
 * -------------------
 * ver 1.0 : 24 Oct 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeT;
using Lexer;
using CodeAnalysis;

namespace TypeAnalys
{
    using File = String;

    public class TypeAnalysis
    {
        public TypeTable typetable_ = new TypeTable();

        static TypeAnalysis instance_;

        //-----------< Construct >-----------------------------
        public TypeAnalysis()
        {
            instance_ = this;
        }

        //------------<add key and TypeItem >--------------------------------------
        public void add(File file,Elem ele,string namesp)
        {
            if (ele.type == "namespace")
            {
                typetable_.add(ele.name, file, "");
            }
            else
            {
                typetable_.add(ele.name, file, namesp);
            }
        }
        //--------------<how the result>-------------------------s
        public void display()
        {
            typetable_.show();
        }
#if (TYPEANA)
        class TestTypeAnalysis
        {
            static void Main(string[] args)
            {
                Console.Write("\n  Demonstrating TypeAnalysis");
                Console.Write("\n ======================\n");

                TypeAnalysis typeanalysis = new TypeAnalysis();

                List<string> files = TestParser.ProcessCommandline(args);
                foreach (string file in files)
                {
                    Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

                    ITokenCollection semi = Factory.create();
                    //semi.displayNewLines = false;
                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", args[0]);
                        return;
                    }

                    Console.Write("\n  Type and Function Analysis");
                    Console.Write("\n ----------------------------");

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
                    string namesp="";
                    foreach(Elem ele in table)
                    {
                        if (ele.type == "namespace")
                        {
                            namesp = ele.name;
                        }
                        typeanalysis.add(f,ele,namesp);
                    }
                    typeanalysis.display();
                    Console.Write("\n");

                    semi.close();
                }
                Console.Write("\n\n");
            }
        }
#endif
    }
}
