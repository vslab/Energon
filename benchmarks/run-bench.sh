#!/bin/bash

if [ $EUID != 0 ]; then
    sudo -E "$0" "$@"
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

STARTDIR=`pwd`
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
  perfout=`perf list hw sw 2>&1`
  ll=($perfout)
  count=1
  declare -a res
  divisor=":"
  skipnext="no"
  for w in ${ll[@]}; 
  do
    if [ $w != "[Software" ] && [ $w !=  "[Hardware" ] && [ $w != "event]" ] ; then
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
  cd $STARTDIR
  cd bin
  select FILENAME in *;
  do
     PROGR=./$FILENAME
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
  if [ ${#INSIZE} -gt 0 ] ; then
    URL=$PROTOCOL$REMOTE$NEWCASE$DIVISOR$INSIZE
  else
    INSIZTMP=0
    URL=$PROTOCOL$REMOTE$NEWCASE$DIVISOR$INSIZTMP
  fi
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

perfout=""

# run the program...
function parseOutput {
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

# run the program...
function runProgram {
  echo "running perf stat $PERFARGS $PROGR $INSIZE ..."
  perfout=`perf stat $PERFARGS $PROGR $INSIZE 2>&1`
  # parse the output of perf stat
  parseOutput
  echo "$PROGR $INSIZE terminated"
  #echo $perfout
  echo ${perfcounters[@]}
  echo ${res[@]}
}


SPECPROGRAM="401.bzip2"
SPECRUNFOLDER="run_base_ref_gcc47-64bit.0000"

function setSPECCPUVars {
  clear
  echo "run SPEC CPU 2006 once, so all the folder will be setup"
  echo current SPEC folder is $SPEC
  echo the runfolder is $SPECRUNFOLDER
  echo "enter the new runfolder (leave empty to use the current value)"
  read newspecfolder
  if [ ${#newspecfolder} -gt 0 ] ; then
    SPECRUNFOLDER=$newspecfolder
  fi 
}

function selectSPECCPU {
  echo "select the SPEC CPU program to run (current is $SPECPROGRAM)"
  echo $SPEC
  echo $SPEC/benchspec/CPU2006
  select CHOICE in `find $SPEC/benchspec/CPU2006 -maxdepth 1 -mindepth 1 -exec basename {} \; -type d`;
  do
    SPECPROGRAM=$CHOICE
    break
  done
  SPECINVOKE=$SPEC/bin/specinvoke
  SPECFOLDER=$SPEC/benchspec/CPU2006/$SPECPROGRAM/run/$SPECRUNFOLDER
  SPECCOMMAND="$SPECINVOKE -d $SPECFOLDER -e /dev/null -o /dev/null -f speccmds.cmd -C -q" 
  PROGR=$SPECCOMMAND
  INSIZE=""
}

# -----  iozone  ------
IOZONEEXEC="/root/iozone3_414/src/current/iozone"
IOZONESIZE=4
IOZONEPROG=0

# create new experiment
function newIozoneCase {
  URL=$PROTOCOL$REMOTE$NEWCASE$DIVISOR$IOZONEPROG$DIVISOR$IOZONESIZE
  echo calling $URL
  CURLRES=`curl $URL`
  echo $CURLRES
}

function setupIozone  {
  PROGR="$IOZONEEXEC -w  -s $IOZONESIZE -i $IOZONEPROG"
  INSIZE=""
}

function runiozone {
  for p in 0 1 2 3 4 5 6 7 8 9 10 11 12 ; do
    IOZONEPROG=$p
    #for s in 4 16 256 1024 ; do
      #IOZONESIZE=$s
      IOZONESIZE="64m"
      newIozoneCase
      setupIozone
      i=0
      while [ $i -lt $ITER ] ; do
        setupIozone
        callRun
        runProgram
        callStop
        ((i++))
        sleep 1
      done
    #done
  done
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
echo "4. set SPEC CPU program"
echo "5. set input size"
echo "6. set remote host ip"
echo "7. set performance counters"
echo "8. set iter (number of iterations of every run)"
echo "9. run iozone set"
echo "10. exit"
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
4) selectSPECCPU ;;
5) selectInputSize ;;
6) setRemoteIP ;;
7) setPerformanceCounters ;;
8) setIter ;;
9) runiozone ;;
10) quit="yes" ;;
*) echo "\"$choice\" is not valid"
esac
done


