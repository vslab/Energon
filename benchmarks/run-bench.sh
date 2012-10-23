#!/bin/bash

#if [ $EUID != 0 ]; then
#    sudo "$0" "$@"
#    exit $?
#fi

# create new experiment
# $1 is the algo
# $2 is the array size

# call run (to start the ammeter)

# run the program...
var=`perf stat -e cycles -e instructions -e cache-misses -e cpu-clock -e branch-misses  ./$1 $2 2>&1`

# parse the output of perf stat

# call stop (to stop the ammeter)

