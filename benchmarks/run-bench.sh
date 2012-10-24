#!/bin/bash

#if [ $EUID != 0 ]; then
#    sudo "$0" "$@"
#    exit $?
#fi

# $1 is the algo
# $2 is the array size

PROGR=$1
INSIZE=$2
REMOTE=$3

cd bin

declare -a availPerfCounters

function setPerformanceCounters {
  perfout=`perf list hw 2>&1`
  ll=($perfout)
  count=0
  declare -a res
  divisor=":"
  for w in ${ll[@]}; 
  do
    if [[ $w != "[Hardware" ]] && [[ $w != "event]" ]] ; then
      if [[ $w == "OR" ]] ; then
        ((count--))
        ((count--))
      else
        availPerfCounters[$count]=$w
      fi
    fi
    ((count++))
  done
  echo ${availPerfCounters[@]}
}

function setRemoteIP {
  echo "Select the IP for the remote host (running the energon measurement framework)"
  read REMIP
  REMOTE=$REMIP
}

function selectProgram {
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

function getPerfFromArgs {
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
}

# create new experiment
function newExperiment {
  echo "TODO: newExperiment"
}

# call run (to start the ammeter)
function callRun {
  echo "TODO: callRun"
}

# run the program...
function runProgram {
  echo "running $PROGR($INSIZE)..."
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
}

# call stop (to stop the ammeter)
function callStop {
  echo "TODO: callStop"
}

clear
echo "This is the energon benchmark framework"

quit="no"
while [ $quit != "yes" ]
do
echo ""
echo "currently remote IP is $REMOTE"
echo "selected program is $PROGR"
echo "input size is $INSIZE"
echo ""
echo "1. run"
echo "2. set program"
echo "3. set input size"
echo "4. set remote host ip"
echo "5. set performance counters"
echo "6. exit"
echo -n "Your choice? : "
read choice

case $choice in
1)
  newExperiment
  callRun
  runProgram
  callStop
  ;;
2) selectProgram ;;
3) selectInputSize ;;
4) setRemoteIP ;;
5) setPerformanceCounters ;;
6) quit="yes" ;;
*) echo "\"$choice\" is not valid"
esac
done


