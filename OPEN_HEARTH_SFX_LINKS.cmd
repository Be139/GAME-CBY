@echo off
if exist "D:\Tools\Python\python.exe" "D:\Tools\Python\python.exe" "%~dp0Tools\generate_hearth_sfx_link_navigator.py"
start "" "%~dp0HEARTH_SFX_Link_Navigator.html"
