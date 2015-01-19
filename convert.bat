set var=PawnCfgData
..\..\..\dev\\bin\excel2json\excel2json --excel %var%.xlsx --json %var%.json --csharp %var%.cs --header 3
move %var%.cs ..\..\Scripts\MemDatabase\DataStruct\%var%.cs
pause