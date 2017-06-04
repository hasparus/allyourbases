#!/usr/bin/env ruby

# TODO: info how to use this script.

def launch args
  cmd = 'dotnet run -p Sophie/Sophie.csproj '
  exec(cmd) if args.empty?

  mode, *scenarios = *args
  if mode == 't' || mode == 'test'
    scenarios = Dir['test_scenarios/*.scenario'] if scenarios.empty?
    scenarios.each do |scenario|
      puts "\033[35m Running scenario: #{scenario} \n ~~~ \033[37m"
      puts `#{cmd} < #{scenario}`
      puts "\033[35m ~~~ \033[37m\033[39m"
    end
  end
end

`dotnet restore Sophie` # if it doesnt work try running "dotnet restore" in Sophie/ directory in shell.
launch ARGV
