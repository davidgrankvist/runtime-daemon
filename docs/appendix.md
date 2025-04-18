# Appendix

Things that are good to write down, but would add clutter to README.md.

## Named Pipe Protocol

The protocol is a single line of ASCII text where arguments are delimited by a `|` (pipe). For example, the client may send

```
C:\Users\someUser\someDirectory\hello.dll|firstArg|secondArg\n
```

which will tell the daemon to execute `hello.dll` with the argument list `firstArg secondArg`. Afterwards the server may reply `OK\n`. The CLI accepts a relative file path, but it is resolved before sending it to the daemon (as the daemon does not know from which directory the CLI will run).
