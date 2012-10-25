#!/bin/bash

if [ $EUID != 0 ]; then
    sudo "$0" "$@"
    exit $?
fi

# $1 is the algo
# $2 is the array size

PROGR=$1
INSIZE=$2
REMOTE=$3
ITER=1
PERFARGS=" "
PROTOCOL="http://"
NEWCASE="/Temporary_Listen_Addresses/case"
START="/Temporary_Listen_Addresses/start"
STOP="/Temporary_Listen_Addresses/stop"
DIVISOR="/"

cd bin

# available performance counters
declare -a availPerfCounters
declare -A perfCountersAliases
# active performance counters
declare -a perfcounters
# results
declare -a res

function printAvailablePerfCounters {
  echo "Available performance counters are: ${availPerfCounters[@]}"
}

function printSelectedPerfCounters {
  echo "Selected performance counters are: ${perfcounters[@]}"
}

function setPerformanceCounters {
  # gather the available perf counters
  perfout=`perf list hw 2>&1`
  ll=($perfout)
  count=1
  declare -a res
  divisor=":"
  skipnext="no"
  for w in ${ll[@]}; 
  do
    if [[ $w != "[Hardware" ]] && [[ $w != "event]" ]] ; then
      if [[ $w == "OR" ]] ; then
        skipnext="yes"
      else
        if [[ $skipnext == "yes" ]] ; then
          # alias
          prevCount=$count-1
          alias=${availPerfCounters[$prevCount]}
          perfCountersAliases[$alias]=$w
          skipnext="no"
        else
          availPerfCounters[$count]=$w
          ((count++))
        fi
      fi
    fi
  done
  availPerfCounters[$count]="seconds"

  quit="no"
  while [ $quit != "yes" ]
  do
    printSelectedPerfCounters
    count=1
    echo "0. done"
    for p in ${availPerfCounters[@]}; do
      present="no"
      for a in ${perfcounters[@]}; do
        if [ $p == $a ] ; then
          present="yes"
        fi
      done
      if [ $present == "no" ] ; then
        echo "$count. $p (ADD)"
      else
        echo "$count. $p (REMOVE)"
      fi
      ((count++))
    done
    read choice
    if [ $choice -eq 0 ] ; then      
      break;
    else
      curr=${perfcounters[$choice]}
      if [ ${#curr} -gt 0 ] ; then
        perfcounters[$choice]=""
      else
        perfcounters[$choice]=${availPerfCounters[$choice]}
      fi
    fi
  done
  PERFARGS=" "
  MINUSE=" -e "
  for p in ${perfcounters[@]}; do
    if [ ${#p} -gt 0 ] ; then
      if [ "$p" != "seconds" ] ; then
        PERFARGS=$PERFARGS$MINUSE$p
      fi
    fi
  done
  echo $PERFARGS
}

function setIter {
  echo "Set the number of iterations of every run (current value is $ITER)"
  read ITER
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
function newCase {
  URL=$PROTOCOL$REMOTE$NEWCASE$DIVISOR$INSIZE
  echo calling $URL
  CURLRES=`curl $URL`
  echo $CURLRES
}

# call run (to start the ammeter)
function callRun {
  URL=$PROTOCOL$REMOTE$START
  echo calling $URL
  CURLRES=`curl $URL`
  echo $CURLRES
}

# run the program...
function runProgram {
  echo "running $PROGR($INSIZE)..."
  perfout=`perf stat $PERFARGS ./$PROGR $INSIZE 2>&1`
  # parse the output of perf stat
  ll=($perfout)
  count=0
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
        #echo checking $n against $perfcounter
        if [ "$n" == "$perfcounter" ]; then
          # the current word is a perf counter name
          # the previous word was its value
          v=${ll[$count-1]}
          # assign it only if it' s a number
          if [[ "$v" =~ ^([0-9]+(,)?)+([.][0-9]+)?$ ]] ; then
            v=`echo $v | sed s/,//g`
            res[localcounter]=$v
          fi
        else
          # checking alias
          aliasPerf=${perfCountersAliases[$perfcounter]}
          if [ "$aliasPerf" == "$n" ]; then
            # the current word is a perf counter name
            # the previous word was its value
            v=${ll[$count-1]}
            # assign it only if it' s a number
            if [[ "$v" =~ ^([0-9]+(,)?)+([.][0-9]+)?$ ]] ; then
              v=`echo $v | sed s/,//g`
              res[localcounter]=$v
            fi
          fi
        fi
        ((localcounter++))
      done  
    fi
    ((count++))
  done
  echo "$PROGR($INSIZE) terminated"
  #echo $perfout
  echo ${perfcounters[@]}
  echo ${res[@]}
}

# call stop (to stop the ammeter)
function callStop {
  URL=$PROTOCOL$REMOTE$STOP
  for w in ${res[@]}; do
    URL=$URL$DIVISOR$w
  done
  echo calling $URL
  CURLRES=`curl $URL`
  echo $CURLRES
}

function experiment {
  for m in 1 4 16 64 256 1024 ; do
    i=0
    INSIZE=$[m*1024*1024]
    newCase
    while [ $i -lt $ITER ] ; do
      callRun
      runProgram
      callStop
      ((i++))
      sleep 1
    done
  done

}

clear
echo "This is the energon benchmark framework"

quit="no"
while [ $quit != "yes" ]
do
echo "------------------------------"
echo "currently remote IP is $REMOTE"
echo "selected program is $PROGR"
echo "input size is $INSIZE"
echo "will perform $ITER iterations"
printSelectedPerfCounters
echo ""
echo "1. cycle run"
echo "2. single run"
echo "3. set program"
echo "4. set input size"
echo "5. set remote host ip"
echo "6. set performance counters"
echo "7. set iter (number of iterations of every run)"
echo "8. exit"
echo -n "Your choice? : "
read choice

case $choice in
1) experiment ;;
2)
  i=0
  newCase
  while [ $i -lt $ITER ] ; do
    callRun
    runProgram
    callStop
    ((i++))
    sleep 1
  done
  ;;
3) selectProgram ;;
4) selectInputSize ;;
5) setRemoteIP ;;
6) setPerformanceCounters ;;
7) setIter ;;
8) quit="yes" ;;
*) echo "\"$choice\" is not valid"
esac
done


