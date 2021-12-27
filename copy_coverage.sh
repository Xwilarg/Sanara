#!/bin/sh
set -e
cd Sanara.UnitTests/TestResults
cd $(ls)
cp coverage.cobertura.xml  ../../../coverage.xml
cd ../../..