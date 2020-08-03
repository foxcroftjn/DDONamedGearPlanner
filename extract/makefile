ddodata.dump: extract.exe ddodata.dat
	mono extract.exe > ddodata.dump

extract.exe: extract.cs ../DDONamedGearPlanner/DDOData.cs
	csc $^

ddodata.dat: ../DDONamedGearPlanner/ddodata.dat
	ln -fs ../DDONamedGearPlanner/ddodata.dat

.PHONY: test clean

test: SHELL=/bin/bash
test: ddodata.dump
	paste -d \\n <(sqlite3 < ddodata.dump 2>&1 | sed "s/^/`printf '\e[31m'`/") <(sed -n "$$(sqlite3 < ddodata.dump 2>&1 | cut -f4 -d' ' | sed 's/:/p/' | paste -d ';' -)" ddodata.dump | sed "s/^/`printf '\e[0m'`/")

clean:
	rm -f extract.exe ddodata.dat ddodata.dump
