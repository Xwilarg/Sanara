#!/bin/sh
set -e
cd SanaraV3.UnitTests/TestResults
cd $(ls)
cp coverage.cobertura.xml  ../../../coverage.xml
cd ../../..