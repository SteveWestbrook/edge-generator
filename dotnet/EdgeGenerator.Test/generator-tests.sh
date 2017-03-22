#!/bin/sh

# Build Test assembly
mcs -out:./bin/DotNetTest.dll -t:library ../DotNetTest/*.cs

# Copy references
cp ../../bin/EdgeReference.dll ./bin
cp ../../bin/EdgeGenerator.dll ./bin

# Build unit tests into an assembly
mcs -out:bin/EdgeGenerator.Test.dll -t:library -r:./bin/DotNetTest.dll,./bin/EdgeReference.dll,./bin/EdgeGenerator.dll,/usr/lib/mono/gac/nunit.framework/2.6.3.0__96d09a1eb7f44a77/nunit.framework.dll *.cs

# Run unit tests.
nunit-console ./bin/EdgeGenerator.Test.dll
