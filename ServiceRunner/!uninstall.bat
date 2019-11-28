@echo off
@setlocal enableextensions
@cd /d "%~dp0"
%WinDir%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /u ServiceRunner.exe
pause