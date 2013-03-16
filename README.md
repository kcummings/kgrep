This is kgrep as in Kevin's grep.

It's a wrapper around C#'s regex engine.
It's purpose is an easy search, replace and print text from an input stream.

Usage: kgrep [-f patternfile] matchpattern [topattern] filename(s)
Finds ALL occurances of matchpattern in filename(s) or stdin.
If topattern is present, the matched source is globally replace by topattern.
All matched output is written to a seperate line to stdout (console).

If patternfile is present, matchpattern and topattern are ignore (intrepreted as filenames)



Patternfile is a file contains a list of search and replace patterns.
See SampleReplacementFile.txt for example of placing replacement strings in a file.

