##This is kgrep, as in Kevin's grep.

A command line tool to search, replace, extract and/or print text based on regular expresions from stdin or input file(s). All output is written to stdout.

There is a compiled version of kgrep.exe in scr/bin/Release if you want ot use the tool without compiling from source. There is no seperate config file.

Project Goals
-------------

* Refactor a working but untestable program into a "modern" application with Unit Tests, small methods, classes, etc.
* Be on guard for feature improvements as the code is being refactored. 
* OS independence. Compile with either Visual Studio or Mono. Developeded using Nunit 2.6.1 & .NET 3.5. It has not been testing using Mono.

Usage
-----

    kgrep matchpattern filename...filenameN
    cat filename | kgrep matchpattern
    
    where matchpattern = Replacement | ScanToken
        Replacement = ReplacementFilename | ReplacementString
           ReplacementString = a string with syntax "anchor~before~after" where "anchor~" is optional. 
           ReplacementFilename is a file contains a ReplacementString per line.

        ScanToken =  a regex string that when found in input will print one match per line


A SearchToken string is a regex used to scan and print matching tokens each on a new line. Example: 

    echo "hello dolly bob"|kgrep "[ld]o" 
   will print (writes matching strings on seperate lines)

    lo
    do

The "after" pattern of a Replacement can include the $1, $2, etc. that represents the matched group number. 

The Replacment string has an option "first" field: "anchor~before~after". The "before~after" replacement will be applied if and only if anchor is found in the line. anchor is a regex.

### Control options
The following controls can be embedded anywhere in the ReplacementFile or ReplacementString. Control options are not case sensitive but must be at the beginning of a line.

- delim=? where ? represents the new delimiter character. The delimitor is the character that seperates the ReplacementString fields. Default is "~".

- comment=? where ? is the character to designate the beginning of a comment. Default is "#".

- scope=[first|all] If set to first, once the Replacement is found in the file it will be applied and no other replacement applied to that line. If set to all, all Replacements will be applied to all line in all circumstances.


Replacement can be put into a file and read by kgrep. If matchpattern is a file, kgrep assumes the Replacement commands are in the file. There is no limit to the number of Replacements that can be in a file. See SampleReplacementFile.txt for examples of Replacements in a file. Example: The "~" tells kgrep the argument is a Replacement .
   
    echo "hello dolly bob"|kgrep "[ld]o~hi"   # prints "helhi hilly bob"
    echo "careful here"|kgrep "ere~ful~fully" # prints "carefully here"
    echo "careful here"|kgrep "abc~ful~fully" # "abc" not found, prints ""


## License

[The MIT License](http://opensource.org/licenses/MIT)








