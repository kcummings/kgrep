##This is kgrep, as in Kevin's grep.

A command line tool to search, replace, extract and/or print text based on regular expressions from stdin or input file(s). All output is written to stdout. Kgrep uses .NET regex syntax.


Project Goals
-------------

* Refactor a working but untestable program into a "modern" application with Unit Tests, small methods, classes, etc. In other words, a sandbox to play, experiment and have a useful tool at the end of the day.
* OS independence. Developed using VS2010, Nunit 2.6.1, NLog 2.0.1.2 & .NET 4.0. Should be compatible with Mono however it has not be compiled or tested under Mono. A sample log file is *SampleLog.log*.

Binaries
-----
If you are anxious to try kgrep, the binaries are in the *deploy* folder. You must also have .net 4.0 installed.


Usage
-----

    kgrep matchpattern filename...filenameN
    cat filename(s) | kgrep matchpattern
    
    where matchpattern = Replacement | ScanToken
        Replacement = ReplacementFilename | ReplacementCommand
           ReplacementCommand = a string with syntax "anchor~before~after" where "anchor~" is optional. 
           ReplacementFilename is a file containing a ReplacementCommand per line.

        ScanToken =  a regex string that when found in input will print one match per line

Kgrep runs in two modes: A Scanner or a Search and Replace tool. 

It runs as a scanner if the replacement file (or replacements given on the command line) only contain *SearchTokens*. All (and only) found tokens are printed to stdout. 

If the file contains both *ReplacementCommands* and *SearchTokens*, it will search and replace the given tokens and print the results to stdout. This is the more powerful mode. Unlike grep, all named regular expression values are rememebered during the run and can be applied to locations other than just the immediate target string. 

---

### SearchToken
A *SearchToken* is a regex string used to scan and print matching patterns. 

SearchToken syntax. (Anchor is optional. Do not include the ~ delimitor if anchor is omitted.)

    anchor ~ searchtoken   

If anchor is given, only lines that match the anchor regex pattern are candidates for continued searching. Only the matching pattern is printed. Characters not matched are ignored. Before printing to stdout, all matches on a given line are concatinated together using the value in the ScannerFS string and printed on a separate line. *SearchTokens* can be placed in a file but are usually only given on the command line. Note: Currently you cannot scan a text string for the field delimiter. If found, the *SearchToken* is interpreted as a *ReplacementCommand*. Use the Control Option ScannerFS to control the characters that are placed between each matched token.

Example: 

    echo "hello dolly bob"|kgrep "[ld]o" 
   will print (writes matching patterns on separate lines)

    lo
    do
- - -

### ReplacementCommand
The power of kgrep is the *ReplacementCommand*. 

ReplacementCommand syntax: 

     anchor ~ searchtoken ~ targettoken     

Anchor acts the same here as in scanner mode. Only *searchtoken* is required however if a replacement file only consists of searchtokens, kgrep will run as a scanner (see above). The field delimiter ~ can be overriden. The targettoken can't be a regex pattern. *ReplacementCommands* can be stacked into one string and supplied on the command line or stored in a file and given as the first argument on the command line. *ReplacementCommands* are stacked using the ";" stacking character, e.g. "dog~cat; h(..)~cold" contains two *ReplacementCommands*. The stacking character cannot be overridden. 

There is no practical limit to the number of *ReplacementCommands* that can be in a replacement file or command line. *ReplacementCommands* are processed in the order given. See SampleReplacementFile.txt for examples of *ReplacementCommands* in a file. 

Leading and trailing spaces are removed from these fields. If you want to include leading or trailing spaces, enclose the string in double quotes, i.e. " world ". The enclosing double quotes will not be included as part of the search field but the blanks will be included.
 
If supplied and is not found in a line, the "before~after" replacement will **not** be applied.

    Some examples:    
    echo "Billy"|kgrep "(B|i)~"               # prints "iy" by removing all "B" and "i"s
    echo "Billy"|kgrep "(.) ~ $1-"            # prints "B-i-l-l-y-" 
    echo "hello dolly bob"|kgrep "[ld]o~hi"   # prints "helhi hilly bob"
    echo "careful here"|kgrep "ere~ful~fully" # prints "carefully here"
    echo "careful here"|kgrep "abc~ful~fully" # "abc" not found, prints ""
    echo "cat dog"|kgrep "(.?) (.?) ~ $2 $1"  # prints "dog cat"
    echo "[aA]b~B; Hello~bye"|kgrep "Hello Abe" # prints "Bye Be" Note the stacked ReplacementCommands
    echo "delim=,; a,b; g,f"|kgrep "abcdefg"  # prints "bbcdeff"
    echo "comment='; 'ignored;"|kgrep "abc"   # prints ""  because no delim present so interpreted as a SearchToken
    echo "#~; comment='; 'ignored;"|kgrep "abc"   # prints "abc" because argument is now interpreted as a ReplacementCommand

**ReplacementCommand Control Options**

The following controls can be embedded anywhere in the ReplacementFile or stacked *ReplacementCommands*. Control options are not case sensitive but must be at the beginning of a line. Control Options are no case sensitive but there values are case sensitive when applied to a line. There is no separate config file.

- **delim=?** where ? represents a single or a series of character enclosed in optional quotes. Its value is the new delimiter. This value separates the ReplacementCommand fields. Default is "~".

- **comment=?** This value designates the beginning of a comment. Default is "#".

- **ScannerFS=?** Only used in Scanner mode. The scanned tokens are "glued" together with the value of this option. Default is "\n".

- **scope=[first|all]** If set to **first**, the first time a *ReplacementCommand* is found on the line, no other replacements are applied to that line. If set to **all** , as many Replacements as possible will be applied to a line.

Caution: You can get unexpected results if you are not careful when using Control Options. For example, setting comment=~ still allows default delim=~ so the *ReplacementCommand* "comment=~; a~b" is interpreted as a *ReplacementCommand* but becomes just "a" since "~b" is now a comment and the expected replacement doesn't take effect. No change occurs to the source inputs.


**Pickups**

Pickups add a lot of flexibility to kgrep.

Kgrep considers all named regex groups, pickups. Example: The string "hello (?&lt;word&gt;[a-z]+)" contains a pickup named "word". Pickup values are kept for the duration of the run and can be used in any *targettoken*. To reference a pickup use ${pickupname} syntax. If the same pickup name is used more than once, the last pickup value is used (Previous pickup values are discarded when a pickup of the same name is used.)

If *targettoken* is not given, no replacment will occur but any pickups will be gathered. If only *targettoken* is given (~ targettoken), it is a pseudo print statement. Pickup placeholders (e.g. ${pickupname} ) will be inserted and the result printed to stdout.

Example of pickup, hold and print:
  
    Given these ReplacementCommands:
    (?<lowerword>[a-z]+)
    <tag id=(?<idpickup>[a-zA-Z]+)> 
    ~ ${idpickup} ${lowerword}

    And this input:
    <tag id=hi> today

    While send to stdout:
    hi tag    # idpickup="hi"; lowerword="tag"

    
Example of replacing with repeating pickup:
   
    Given these ReplacementCommands:
    ^(?<name>[a-z]+)~c${name}${name}


    And this input:
    a

    While send to stdout:
    caa       # mame='a'








