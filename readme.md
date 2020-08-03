# DDONamedGearPlanner

This project was created by [Pfhoenix](https://github.com/Pfhoenix/). It aims to streamline the DDO gear tetris game through facilitating item discovery and combinations. It is GUI windows forms application (written in C#).

The lastest release can always be downloaded [here](https://github.com/Pfhoenix/DDONamedGearPlanner/releases).

## Data Export

This project has done such a thorough job scraping [ddowiki.com](https://ddowiki.com/) that is is no longer makes sense for anyone else to re-scrape the wiki. Instead, it is easier to just use the data that this project so carefully retrieved.

The data is stored as a C# serialized binary file [ddodata.dat](DDONamedGearPlanner/ddodata.dat). This is very much not a portable format. As such, this fork has added a utility to convert the data to an sqlite3 dump.

The *export* command line utility was developed in a linux environment using mono. If you are in such an environment, you should be able to generate this dump by running `make` in the [export](export) directory.

If you are not in such an environment, fear not. The dump can also be produced using docker by running the following commands in the root directory of this project:

```bash
docker build . --tag ddo_export
docker run ddo_export > ddodata.sql
```