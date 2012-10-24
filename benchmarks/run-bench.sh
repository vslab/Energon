#!/bin/bash

#if [ $EUID != 0 ]; then
#    sudo "$0" "$@"
#    exit $?
#fi

# $1 is the algo
# $2 is the array size

PROGR=$1
INSIZE=$2
cd bin

function selectCase {
  echo "Select the program to measure:"
  select FILENAME in *;
  do
     PROGR=$FILENAME
     break
  done
}

function selectInputSize {
  echo "Select the input array size"
  sizes="1024 4096 16384 65536 262144 1048576 4194304 16777216 67108864 268435456 1073741824"  
  select CHOICE in $sizes;
  do
    INSIZE=$CHOICE
    break
  done
}

selectCase
selectInputSize
echo ciao

PERFARGS=" "
MINUSE=" -e "
let count=0
let countParms=0
declare -a perfcounters
for var in "$@"
do
    if [ $count -gt 1 ]; then
        PERFARGS=$PERFARGS$MINUSE$var
        perfcounters[$countParms]=$var
        ((countParms++))
    fi
    ((count++))
done

# create new experiment


# call run (to start the ammeter)


# run the program...
perfout=`perf stat $PERFARGS ./$PROGR $INSIZE 2>&1`

# parse the output of perf stat
ll=($perfout)
count=0
declare -a res
divisor=":"
for w in ${ll[@]}; 
do 
  if [ $count -ge 1 ]; then
    #echo $count$divisor$w;
    n=${ll[$count]}
    # is this word a counter name?
    localcounter=0
    for perfcounter in ${perfcounters[@]};
    do
      if [ "$n" == "$perfcounter" ]; then
        # the current word is a perf counter name
        # the preceding word was its value
        res[localcounter]=${ll[$count-1]}
      fi
      ((localcounter++))
    done  
  fi
  ((count++))
done

echo ${perfcounters[@]}
echo ${res[@]}


# call stop (to stop the ammeter)

