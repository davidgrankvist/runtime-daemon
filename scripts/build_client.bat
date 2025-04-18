@echo off

mkdir bin > NUL 2>&1

cl runtime-daemon-client\main.c ^
    /Fe: bin\runtime-daemon.exe ^
    /Fo: bin\
