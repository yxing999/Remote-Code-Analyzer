# A remote code analyzer based on WCF and WPF.
### Summary
Systems that may consist of hundreds or even thousands of packages1 and perhaps several million lines of code.
In order to successfully implement big systems we need to partition code into relatively small parts and thoroughly test each of the parts before inserting them into the software baseline2. As new parts are added to the baseline and as we make changes to fix latent errors or performance problems we will re-run test sequences for those parts and, perhaps, for the entire baseline. Managing that process efficiently requires effective tools for code analysis as well as testing. 
Code analysis consists of extracting lexical content from source code files, analyzing the code's syntax from its lexical content, and building a Type Table holding the dependency results. Alternately you can provide an Abstract Syntax Tree (AST) that holds the results of our analysis. It is then fairly easy to build several backends that can do further analyses on the AST to construct code metrics, search for particular constructs, evaluate package dependencies, or some other interesting features of the code.

### Operation Instruction:
================================================================
1. Double click "Complie.bat" file.
2. Double click "run.bat" file.
3. After step 1&2, you will get 4 windows: 
	Autotest console: display all test of all functions automatically.
	GUI console:display information about GUI.
	server console:display information about server.
	GUI:user interface provided to uesrs.
4. Click "Local" button on GUI: display file list on local storage
5. Click "Remote" button: display file list on remote storage.
6. Click "Connect" button: connect remote and local.
8. Double Click file name: display file's content in a new window.
9. Click "Analyze" :display source code analysis result.
