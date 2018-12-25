///////////////////////////////////////////////////////////////////////
// Executive.cs - Demonstrate Prototype Code Analyzer                //
// ver 1.0                                                           //
// Language:    C#, 2017, .Net Framework 4.7.1                       //
// Platform:    Dell Precision T8900, Win10                          //
// Application: Demonstration for CSE681, Project #3, Fall 2018      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines the following class:
 *   Executive:
 *   - uses Parser, RulesAndActions, Semi, and Toker to perform basic
 *     code metric analyzes
 */
/* Required Files:
 *   Executive.cs
 *   Parser.cs
 *   IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 *   ITokenCollection.cs, Semi.cs, Toker.cs
 *   Display.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 09 Oct 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
  using Lexer;

  class Executive
  {
    //----< process commandline to get file references >-----------------

    static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
      if (args.Length < 2)
      {
        Console.Write("\n  Please enter path and file(s) to analyze\n\n");
        return files;
      }
      string path = args[0];
      if(!Directory.Exists(path))
      {
        Console.Write("\n  invalid path \"{0}\"", System.IO.Path.GetFullPath(path));
        return files;
      }
      path = Path.GetFullPath(path);
      for (int i = 1; i < args.Length; ++i)
      {
        string filename = Path.GetFileName(args[i]);
        files.AddRange(Directory.GetFiles(path, filename));
      }
      return files;
    }

    static void ShowCommandLine(string[] args)
    {
      Console.Write("\n  Commandline args are:\n  ");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
      Console.Write("\n");
    }

    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Parser");
      Console.Write("\n ======================\n");

      ShowCommandLine(args);

      List<string> files = ProcessCommandline(args);
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
        Display.showMetricsTable(table);
        Console.Write("\n");

        semi.close();
      }
      Console.Write("\n\n");
    }
  }
}
