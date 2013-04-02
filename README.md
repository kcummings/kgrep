##This is kgrep, as in Kevin's grep.

A command line tool to search, replace, extract and/or print text based on regular expresions from stdin or input file(s). All output is written to stdout.


Project Goals
-------------

* Refactor a working but untestable program into a "modern" application with Unit Tests, small methods, classes, etc. In other words, a sandbox to play, experiment and have a useful tool at the end of the day.
* OS independence. Developed using VS2010, Nunit 2.6.1 & .NET 3.5. Should be compatible with Mono however it has not be compiled or tested under Mono.

Usage
-----

    kgrep matchpattern filename...filenameN
    cat filename | kgrep matchpattern
    
    where matchpattern = Replacement | ScanToken
        Replacement = ReplacementFilename | ReplacementString
           ReplacementString = a string with syntax "anchor~before~after" where "anchor~" is optional. 
           ReplacementFilename is a file containing a ReplacementString per line.

        ScanToken =  a regex string that when found in input will print one match per line

- - -

### ReplacementString
The power of kgrep is the *ReplacementString*. These can be stacked into one string or stored in a file and given as the first argument on the command line. *ReplacementStrings* are stacked using the ";" stacking character, e.g. "dog~cat; h(..)~cold" contains two *ReplacementStrings*. The stacking character cannot be overridden. If the field delimitor (default is "~") is anywhere in the first argument, the argument is interpreted as a *ReplacementString* otherwise it's considered either a *SearchToken* or the name of a file that contains *ReplacementStrings*.

There is no practical limit to the number of *ReplacementStrings* that can be in a replacement file. *ReplacementStrings* are processed in the order given. See SampleReplacementFile.txt for examples of *ReplacementStrings* in a file.  *ReplacementStrings* consists of two or three fields: an anchor, before and after field. Any or all can be a regex.The anchor field is optional. Leading and trailing spaces are removed from these fields. If you want to include a leading or trailing space, use '\s', e.g. " \sA" is a pattern to match a space followed by an uppercase A. 
If supplied and is not found in a line, the "before~after" replacement will **not** be applied.

    Some examples:    
    echo "hello dolly bob"|kgrep "[ld]o~hi"   # prints "helhi hilly bob"
    echo "careful here"|kgrep "ere~ful~fully" # prints "carefully here"
    echo "careful here"|kgrep "abc~ful~fully" # "abc" not found, prints ""
    echo "cat dog"|kgrep "(.?) (.?) ~ $2 $1"  # prints "dog cat"
    echo "[aA]b~B; Hello~bye"|kgrep "Hello Abe" # prints "Bye Be" Note the stacked ReplacementStrings
    echo "delim=,; a,b; g,f"|kgrep "abcdefg"  # prints "bbcdeff"
    echo "comment='; 'ignored;"|kgrep "abc"   # prints ""  because no delim present so interpreted as a SearchToken
    echo "#~; comment='; 'ignored;"|kgrep "abc"   # prints "abc" because argument is now interpreted as a ReplacementString

**ReplacementString Control Options**

The following controls can be embedded anywhere in the ReplacementFile or stacked *ReplacementStrings*. Control options are not case sensitive but must be at the beginning of a line. There is no seperate config file.

- **delim=?** where ? represents the new delimiter character. The delimitor is the character that seperates the ReplacementString fields. Default is "~".

- **comment=?** where ? is the character to designate the beginning of a comment. Default is "#".

- **scope=[first|all]** If set to first, once the Replacement is found in the file it will be applied and no other replacement applied to that line. If set to all, all Replacements will be applied to all line in all circumstances.

Caution: You can get unexpected results if you are not careful when using Control Options. For example, setting comment=~ still allows default delim=~ so the *ReplacementString* "comment=~; a~b" is interpretted as a *ReplacementString* but becomes just "a" since "~b" is now a comment and the expected replacement doesn't take effect. No change occures to the source inputs.

---

### SearchToken
A *SearchToken* is a regex string used to scan and print matching tokens each on a new line. A stacking character has no special meaning in a *SearchToken* and is interpreted as a normal character. Note: Currently you cannot scan a text string for a field delimitor. If found, the *SearchToken* is intrepreted as a *ReplacementString*.

Example: 

    echo "hello dolly bob"|kgrep "[ld]o" 
   will print (writes matching strings on seperate lines)

    lo
    do










