#!/bin/bash

#if [ $EUID != 0 ]; then
#    sudo "$0" "$@"
#    exit $?
#fi

# $1 is the algo
# $2 is the array size

select FILENAME in *;
do
     echo "You picked $FILENAME ($REPLY), it is now only accessible to you."
done

PROGR=$1
INSIZE=$2
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

