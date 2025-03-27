
@echo off

rem H is the destination game folder
rem GAMEDIR is the name of the mod folder (usually the mod name)
rem GAMEDATA is the name of the local GameData
rem VERSIONFILE is the name of the version file, usually the same as GAMEDATA,
rem    but not always

set H=%KSPDIR%
set H2=R:\KSP_1.12.5-dev



set GAMEDIR=KSP_PartVolume
set GAMEDATA="GameData"
set VERSIONFILE=%GAMEDIR%.version

set DP0=r:\dp0\kspdev

copy /Y "%1%2" "%GAMEDATA%\%GAMEDIR%\Plugins"
copy /Y "%1%3".pdb "%GAMEDATA%\%GAMEDIR%\Plugins"

copy /Y %VERSIONFILE% %GAMEDATA%\%GAMEDIR%

xcopy /y /s /I %GAMEDATA%\%GAMEDIR% "%H%\GameData\%GAMEDIR%"
rem xcopy /y /s /I %GAMEDATA%\%GAMEDIR% "%H2%\GameData\%GAMEDIR%"

rem pause
