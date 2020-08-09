# Sh-orts - shell that proactively helps lazy devs

```
Welcome to Shortify :)
I will observe your work proactively suggest:
  - Shortcuts to commonly used commands and batches of commands
  - Solutions to errors you encounter
sh-orts >
```

### How to run

* Need to install .NET Core 3.1 sdk
* Clone this repo
* `cd` into root directory
* [Linux/macOS] `./shorts.sh`
* [Windows] `dotnet run Shortify/Shortify.fsproj`

### Features & Roadmap

- [x] Basic interface to get active shortcuts, ask for shortcut to be suggested
- [x] Unrecognized commands are forwared to regular shell
- [ ] Suggest shortcuts
    - [x] Once a command or a sequence of commands is repeated
    - [ ] Once an argument is repeated over N times
- [ ] Suggest to run a set of commands at a given time
    - [ ] If commands take a long time to complete & are ran around the same time
- [ ] If a command fails
    - [ ] and the same N commands are ran after words create recovery command for similar errors
    - [ ] perform a google/stackoverflow/propriatery engine search for suggested solutions
- [ ] Shift to ML from current rule based approach. See what GTP-3 can offer.
- [ ] For a given company share all interaction history, shortcuts, and recovery options between employees. Increasing data corpus leads to better suggestions. This can also be used to detect problems in documentation (ex: command sequences are in docs, but started failing for all employees). 

### Philosofy - stay motivated by specific use cases

Almost every morning I run a sequence of commands that roughly correspond to
```
$ git pull
$ helper_script commonly-used-arg1 commonly-used-arg2 long-arg
$ # Open Xcode manually and hit run
```

Each of those steps take approximately 10 minutess! And I often make a mistakes in the long arguments. I have been running the same/similar sequence of commands for the past 3 months and have not optimized my workflow! This is frightening to me, but this happens every time!

Another part of this that is harder to solve for - there 

Look, all I needed is someone to point out to me - "Hey, why don't you do this? This should reduce friction in your workflow." 

### Tech features

[x] Crossplatform .NET Core :)
[x] Persists state across sessions into a file (aka better .bash_history)
[ ] Troubleshoot bugs when running `git commit -m` or `cd ..` and I suspect many others
[ ] Someone fix serialization/deserialization in F# type system already!

### Free form

Why not just write the damn bashrc file and just errors google yourself? I mean how much time does this actually save? Obviously this project is not mature enough to actually replace shell, at least for the tech features missing.
