ddodata.sql: export.exe ddodata.dat
	mono export.exe > ddodata.sql

export.exe: export.cs ../DDONamedGearPlanner/DDOData.cs
	csc $^

ddodata.dat: ../DDONamedGearPlanner/ddodata.dat
	ln -fs ../DDONamedGearPlanner/ddodata.dat

.PHONY: test clean

test: SHELL=/bin/bash
test: ddodata.sql
	paste -d \\n <(sqlite3 < ddodata.sql 2>&1 | sed "s/^/`printf '\e[31m'`/") <(sed -n "$$(sqlite3 < ddodata.sql 2>&1 | cut -f4 -d' ' | sed 's/:/p/' | paste -d ';' -)" ddodata.sql | sed "s/^/`printf '\e[0m'`/")

clean:
	rm -f export.exe ddodata.dat ddodata.sql