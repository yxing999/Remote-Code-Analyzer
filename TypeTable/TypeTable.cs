/////////////////////////////////////////////////////////////////////
// TypeTableDemo.cs - Project #3                                   //
//                                                                 //
// Yuxuan Xing, CSE 681 - Software Modeling and Analysis, Fall 2018//
/////////////////////////////////////////////////////////////////////

/*
 * Package Operations:
 * -------------------
 * This package provides a container for files' type,
 * Served as a bridge for further dependency analysis.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
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

namespace TypeT
{
    using File = String;
    using Type = String;
    using Namespace = String;

    public struct TypeItem
    {
        public File file;
        public Namespace namesp;
    }
    public class TypeTable
    {
        public Dictionary<Type, List<TypeItem>> table  { get; set; } =
           new Dictionary<Type, List<TypeItem>>();

        //-------------- <add by TypeItem> ---------------
        public void add(Type type,TypeItem item)
        {
            if (table.ContainsKey(type))
            {
                table[type].Add(item);  //List's Add
            }
            else
            {
                List<TypeItem> temp = new List<TypeItem>();
                temp.Add(item);
                table.Add(type, temp);  //Dictionary's Add
            }
        }

        //-------------- <add by parameter> ---------------
        public void add(Type type, File file, Namespace ns)
        {
            TypeItem temp;
            temp.file = file;
            temp.namesp = ns;
            add(type, temp);
        }

        //------------- <display the table> -------------------------
        public void show()
        {
            foreach(var elem in table)
            {
                Console.Write("\n\n {0}", elem.Key);
                foreach(var item in elem.Value)
                {
                    Console.Write("\n [{0}, {1}]", item.file, item.namesp);
                }
            }
            Console.Write("\n");
        }

#if (TEST_TYPETABLE)
        class TestTypeTable
        {
            static void Main(string[] args)
            {
                Console.WriteLine("\n Demonstrate how to build typetable");
                Console.WriteLine("\n ====================================");

                TypeTable typeTable = new TypeTable();
                typeTable.add("Type_X", "File_A", "Namespace_Test1");
                typeTable.add("Type_X", "File_B", "Namespace_Test2");
                typeTable.add("Type_Y", "File_A", "Namespace_Test1");
                typeTable.add("Type_Z", "File_C", "Namespace_Test3");

                typeTable.show();

                //access elements in table

                Console.Write("\n Types in the typeTable:");
                foreach(var elem in typeTable.table)
                {
                    Console.Write("\n {0}, ", elem.Key);
                }
                Console.Write("\n\n");
            }
        }
#endif
    }
}

