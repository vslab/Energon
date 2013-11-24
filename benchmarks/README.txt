

requisites:
curl (ubuntu package: curl)
perf (ubuntu package: linux-commontools)

SPEC CPU2006

install it, run it with the test input:
runspec --tune=base --config=Khushboo-macosx.cfg --size=test int
runspec --tune=base --config=Khushboo-macosx.cfg --size=test ?



to compile on linux just run make
to compile on windows run compile.bat from the visual studio command line


to compile iozone do:
sudo su -
wget http://www.iozone.org/src/current/iozone3_414.tar
tar xvf iozone3_414.tar
cd /root/iozone3_414/src/current
make linux

