# A remote code analyzer based on WCF and WPF.
### Summary
Systems that may consist of hundreds or even thousands of packages1 and perhaps several million lines of code.
In order to successfully implement big systems we need to partition code into relatively small parts and thoroughly test each of the parts before inserting them into the software baseline2. As new parts are added to the baseline and as we make changes to fix latent errors or performance problems we will re-run test sequences for those parts and, perhaps, for the entire baseline. Managing that process efficiently requires effective tools for code analysis as well as testing. 
Code analysis consists of extracting lexical content from source code files, analyzing the code's syntax from its lexical content, and building a Type Table holding the dependency results. Alternately you can provide an Abstract Syntax Tree (AST) that holds the results of our analysis. It is then fairly easy to build several backends that can do further analyses on the AST to construct code metrics, search for particular constructs, evaluate package dependencies, or some other interesting features of the code.

### Operation Instruction:
================================================================
