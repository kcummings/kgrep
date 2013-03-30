##This is kgrep, as in Kevin's grep.

A command line tool for easy search, replace and print of text based on a regular expresion from stdin or input file(s). All matched output is written to seperate lines to stdout (console).

### Project Goals
* Refactor a working but untestable program into a "modern" application with Unit Tests, small classes, etc.
* Be on guard for feature improvements as the code is being refactored. 

* Learn how GitHub works :)

### Usage

* kgrep matchpattern filename...filenameN
* cat filename | kgrep matchpattern

### matchpattern is a Replacment or a Search string. 


A Search string is a regex used to scan and print matching tokens each on a new line. Example: 

    echo "hello dolly bob"|kgrep "[ld]o" 
   will print (writes matching strings on seperate lines)

    lo
    do

A Replacement is a string with syntax "before~after" where before is a regex string to search input and after is the replacing string which can include $1, $2, etc representing the regex matched groups. The delimiter, "~", can be changed with the line "delim=?" where ? is the new deliniter single character.

Replacement can be put into a file and read by kgrep. If matchpattern is a file, kgrep assumes the Replacement commands are in the file. There is no limit to the number of Replacements that can be in a file. See SampleReplacementFile.txt for examples of Replacements in a file. Example: The "~" tells kgrep the argument is a Replacement .
   
    echo "hello dolly bob"|kgrep "[ld]o~hi"

will print (replacing matching strings)

    helhi hilly bob    








