namespace Shortify

open Model
open Shortify

open System
open NUnit.Framework
open FsUnit

module ShortcutsTests =
    // samples
    let lsCommand = { Command = "ls"; Arguments = []}
    let lsInvocation = {
        Command = lsCommand
        InvocationTime = DateTime.Now
        CompletionTime = DateTime.Now
    }

    [<Test>]
    let ``shortcuts is proposed if a command is used more than once``() =
        topUsedCommands [lsInvocation; lsInvocation]
        |> should not' (be Empty)
