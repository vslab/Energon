#!/bin/bash

#if [ $EUID != 0 ]; then
#    sudo "$0" "$@"
#    exit $?
#fi

# create new experiment
# $1 is the algo
# $2 is the array size
PROGR=$1
INSIZE=$2
PERFARGS=" "
MINUSE=" -e "
let count=0
for var in "$@"
do
    if [ $count -gt 1 ]; then
        PERFARGS=$PERFARGS$MINUSE$var
    fi
    ((count++))
done
echo $PERFARGS
# call run (to start the ammeter)

# run the program...
perfout=`perf stat $PERFARGS ./$PROGR $INSIZE 2>&1`
echo $perfout
# parse the output of perf stat

# call stop (to stop the ammeter)

