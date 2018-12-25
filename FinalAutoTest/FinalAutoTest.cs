

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
using System.Threading;

namespace Navigator
{
    public class FinalAutoTest
    {
        public void Demo3()
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
            path = "../../../Client";
            nav.go(path, false);
            path = "../../../Server";
            nav.go(path, false);
            path = "../../../MessagePassingCommService";
            nav.go(path, false);
            path = "../../../IMessagePassingCommService";
            nav.go(path, false);
        }

        
        static void Main(string[] args)
        {
            TestUtilities.title("Desmonstrate Requirements: \n");
            FinalAutoTest test = new FinalAutoTest();
            test.Demo3();
            System.Console.ReadKey();
        }
    }
    
}

