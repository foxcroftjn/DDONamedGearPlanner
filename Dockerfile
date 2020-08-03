FROM mono:latest
WORKDIR /project
COPY DDONamedGearPlanner/DDOData.cs DDONamedGearPlanner/ddodata.dat ./
COPY export/export.cs ./
RUN csc *.cs
CMD ["mono","export.exe"]