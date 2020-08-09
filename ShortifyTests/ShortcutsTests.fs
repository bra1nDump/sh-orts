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
    }

    // samples
    let lsCommand = { Command = "ls"; Arguments = [||]}
    let lsInvocation = invocation lsCommand

    let timeLsCommand = { Command = "time"; Arguments = [|"ls"|] }
    let timeLsInvocation = invocation timeLsCommand

    [<Test>]
    let ``command that is used more than once is identified``() =
        repeatedCommandSequence [|lsInvocation; lsInvocation|]
        |> should equal [|2, [|lsCommand|]|]

    [<Test>]
    let ``command sequence of 2 that is repeated is identified``() =
        repeatedCommandSequence [|lsInvocation; lsInvocation; timeLsInvocation; lsInvocation; lsInvocation|]
        |> Array.truncate 1
        |> should equal [|2, [|lsCommand; lsCommand|]|]

    [<Test>]
    let ``2 repeated command sequences are identified``() =
        repeatedCommandSequence [|lsInvocation; lsInvocation; timeLsInvocation; lsInvocation; lsInvocation|]
        |> Array.length
        |> should equal 2