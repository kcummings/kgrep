#!/bin/bash
# Since unit tests do not test actual file i/o, 
# this bash script tests actual file i/o. 
# It was developed using cygwin under windows but uses unix line endings.

cp ../../Tests/bin/Debug/kgrep.exe  .
suite="./suite"
outputFolderPath=$suite/actual
inputFolderPath=$suite/input
expectedFolderPath=$suite/expected

testNames=`ls $inputFolderPath/*.txt`

for testName in $testNames; do   
    testName=`basename $testName .txt`
    echo -n "Testing " $testName "... "
    
    repFilename="$inputFolderPath/$testName.rep" 
    ./kgrep.exe "$repFilename" "$inputFolderPath/$testName.txt" > "$outputFolderPath/$testName.txt"
    
    cmp -s "$expectedFolderPath/$testName.txt" "$outputFolderPath/$testName.txt"
    if [ $? -ne 0 ]; then
        echo "  failed"
    else
	echo "  passed"
    fi
done