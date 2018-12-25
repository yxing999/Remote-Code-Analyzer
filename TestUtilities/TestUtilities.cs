using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Navigator;

namespace MessagePassingComm
{
    public class TestUtilities
    {
        /*----< saves duplicating presentation of results >------------*/

        public static void checkResult(bool result, string compName)
        {
            if (!ClientEnvironment.verbose)
                return;
            if (result)
                Console.Write("\n -- {0} test passed", compName);
            else
                Console.Write("\n -- {0} test failed", compName);
        }
        /*----< check for null reference >-----------------------------*/

        public static bool checkNull(object value, string msg = "")
        {
            if (value == null && ClientEnvironment.verbose)
            {
                Console.Write("\n  null reference");
                if (msg.Length > 0)
                    Console.Write("\n  {0}", msg);
                return true;
            }
            return (value == null);
        }
        /*----< saves duplicating exception handling >-----------------*/
        /*
         * - invokes runnable inside try/catch block
         * - returns true only if no exception is thrown and
         *   runnable returns true;
         * - intent is that runnable only returns true if 
         *   its operation succeeds.
         */
        public static bool handleInvoke(Func<bool> runnable, string msg = "")
        {
            try
            {
                return runnable();
            }
            catch (Exception ex)
            {
                if (ClientEnvironment.verbose)
                {
                    Console.Write("\n  {0}", ex.Message);
                    if (msg.Length > 0)
                        Console.Write("\n  {0}", msg);
                }
                return false;
            }
        }
        /*----< pretty print titles >----------------------------------*/

        public static void title(string msg, char ch = '-')
        {
            Console.Write("\n  {0}", msg);
            Console.Write("\n {0}", new string(ch, msg.Length + 2));
        }
        /*----< pretty print titles >----------------------------------*/

        public static void vbtitle(string msg, char ch = '-')
        {
            if (ClientEnvironment.verbose)
            {
                Console.Write("\n  {0}", msg);
                Console.Write("\n {0}", new string(ch, msg.Length + 2));
            }
        }
        /*----< write line of text only if verbose mode is set >-------*/

        public static void putLine(string msg = "")
        {
            if (ClientEnvironment.verbose)
            {
                if (msg.Length > 0)
                    Console.Write("\n  {0}", msg);
                else
                    Console.Write("\n");
            }
        }

#if(TEST_TESTUTILITIES)

    /*----< construction test >------------------------------------*/

    static void Main(string[] args)
    {
      title("Testing TestUtilities", '=');
      ClientEnvironment.verbose = true;

      Func<bool> passTest = () => { Console.Write("\n  pass test"); return true; };
      checkResult(handleInvoke(passTest), "TestUtilities.handleInvoke");

      Func<bool> failTest = () => { Console.Write("\n  fail test"); return false; };
      checkResult(handleInvoke(failTest), "TestUtilities.handleInvoke");

      Func<bool> throwTest = () => { Console.Write("\n  throw test"); throw new Exception(); return false; };
      checkResult(handleInvoke(throwTest), "TestUtilities.handleInvoke");

      Console.Write("\n\n");
    }
  }
}
#endif