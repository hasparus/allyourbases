#!/usr/bin/env ruby

`dotnet restore` #if it doesnt work try running "dotnet restore" in shell.
exec "dotnet run #{ARGV[0] == 't' ? '< test_input.json': ''}"
