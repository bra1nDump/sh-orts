namespace Shortify

open Model
open Shortify

open System
open NUnit.Framework
open FsUnit

module ShortcutsTests =
    let invocation command = {
        Command = command
        InvocationTime = DateTime.Now
        CompletionTime = DateTime.Now
        Outcome = Ok ()
    }

    // samples
    let lsCommand = { Command = "ls"; Arguments = []}
    let lsInvocation = invocation lsCommand

    let timeLsCommand = { Command = "time"; Arguments = ["ls"] }
    let timeLsInvocation = invocation timeLsCommand

    [<Test>]
    let ``command that is used more than once is identified``() =
        topUsedCommands [lsInvocation; lsInvocation]
        |> should equal [2, lsCommand]

    [<Test>]
    let ``command sequence of 2 that is repeated is identified``() =
        topUsedCommandBatches [lsInvocation; lsInvocation; timeLsInvocation; lsInvocation; lsInvocation]
        |> should equal [2, [lsCommand; lsCommand]]
