#!/bin/bash
cp node_modules/edge-reference/bin/EdgeReference.dll ./bin
rm bin/EdgeGenerator.dll
$1 -out:bin/EdgeGenerator.dll -t:library ./dotnet/EdgeGenerator/*.cs -r:bin/EdgeReference.dll
