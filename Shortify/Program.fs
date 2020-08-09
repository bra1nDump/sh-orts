namespace Shortify

open System
open Fake.Core

[<AutoOpen>]
module Flex =
    let flip f a b = f b a

[<AutoOpen>]
module Model =
    type Command = 
        {
            Command: string
            Arguments: string array
        }
        override this.ToString() = 
            [ yield this.Command; yield! this.Arguments ]
            |> String.concat " "

    type CommandInvocation = {
        Command: Command
        InvocationTime: DateTime
        CompletionTime: DateTime
    }

    type Shortcut = 
        {
            Name: string
            Commands: Command array
        }
        override this.ToString() = 
            this.Name
            + " runs:\n"
            + (
                this.Commands
                |> Array.map (fun c -> c.ToString())
                |> String.concat "\n"
            )

    type State = 
        {
            InvocationHistory: CommandInvocation array
            ActiveShortcuts: Shortcut array
        }
        static member empty = { InvocationHistory = [||]; ActiveShortcuts = [||] }

module Shortify = 
    // TODO: ? Move to Flex module, this is generic
    let topUsedCommandSequenceOfLength invocationHistory length =
        let sequenceses = [|
            for i in [0..Array.length invocationHistory - length] do
            invocationHistory |> Array.skip i |> Array.take length
        |]

        sequenceses 
        |> Array.groupBy id
        |> Array.map (fun (sequence, duplicates) -> Array.length duplicates, sequence)
        |> Array.sortByDescending fst
        |> Array.filter (fun (count, _) -> count > 1)
        |> Array.truncate 1

    let repeatedCommandSequence invocationHistory =
        let commandHistory = Array.map (fun i -> i.Command) invocationHistory
        let maxSequenceLength = Array.length commandHistory / 2

        if maxSequenceLength < 1 then [||]
        else
            [|1..maxSequenceLength|] |> Array.rev
            |> Array.collect (topUsedCommandSequenceOfLength commandHistory)

module Repl =
    open Shortify

    let parse (commandString: string) = 
        match commandString.Split(" ") |> Array.toList with 
        | [] -> None
        | command::args ->
            Some {
                Command = command
                Arguments = args |> Array.ofList
            }

    let execute (command: Command) =
        try
            CreateProcess.fromRawCommand command.Command command.Arguments
            |> Proc.run
            |> ignore

            Ok ()
        with _ -> Error ()

    let suggestCommandsShortcut isProactive minimumSequenceLength state = 
        let shortcutCandidates = 
            repeatedCommandSequence state.InvocationHistory
            |> Array.filter (fun (_, commands) -> Array.length commands > minimumSequenceLength)
            |> Array.truncate 3

        if Array.isEmpty shortcutCandidates then 
            if not isProactive then
                printfn "You are a genious who never repeats themselves! Congratuations! None to be done here"
            state
        else
            printfn "Suggested:"
            [
                for i, (usageCount, commands) in shortcutCandidates |> Array.indexed do
                printfn "  (%d) : %O -- used %d times" 
                    i 
                    (commands |> Array.map (sprintf "%O") |> String.concat "; ") 
                    usageCount
            ]
            |> ignore

            printfn "  (any other key to cancel)"

            try 
                printfn ""
                printf "  Make your selection > "
                let selection = Console.ReadLine() |> int
                let _, command = Array.item selection shortcutCandidates

                printf "  Name your shortcut > "
                let name = Console.ReadLine()
                let shortcut = {
                    Name = name
                    Commands = command
                }
                { state with ActiveShortcuts = Array.append [|shortcut|] state.ActiveShortcuts }
            with _ -> state

    let suggestedScheduledRun = id
    let suggestArgumentShortcuts = id

    let suggest isProactive minimumSequenceLength (state : State) : State =
        suggestCommandsShortcut isProactive minimumSequenceLength state
        |> suggestArgumentShortcuts
        |> suggestedScheduledRun

    let recordInvocation invocation state = 
        { state with InvocationHistory = Array.append [|invocation|] state.InvocationHistory }

    let invokeCommand command state = 
        let invocationTime = DateTime.Now
        let outcome = execute command

        let invocation = {
            Command = command
            InvocationTime = invocationTime
            CompletionTime = DateTime.Now
        }

        state
        |> recordInvocation invocation

    let repl (state : State) : State =
        printf "sh-orts> "

        let shortcutMap = 
            state.ActiveShortcuts
            |> Array.map (fun shortcut -> shortcut.Name, shortcut)
            |> Map.ofArray

        match Console.ReadLine() with 
        | "exit" | "quit" -> 
            printfn "See you soon! Buyee :)"
            Environment.Exit(0)
            failwith "unreachable"
        | "clear history" -> State.empty
        | "suggest" ->
            suggest false 1 state
        | "array" ->
            let yourShortcuts = state.ActiveShortcuts
            if Array.isEmpty yourShortcuts then
                printfn "Currently you dont have any shortcuts. You can add them with 'suggest' command"
            else
                printfn "Your shortcuts: "
                yourShortcuts |> Array.iter (fun s -> s.ToString() |> printfn "%s")
        
            state
        | maybeShortcutName when shortcutMap.ContainsKey maybeShortcutName ->
            let shortcut = shortcutMap.Item maybeShortcutName
            
            shortcut.Commands
            |> Array.fold (flip invokeCommand) state 
        | unknownCommand ->
            match parse unknownCommand with
            | None -> state
            | Some command -> 
                invokeCommand command state
                |> suggest true 2

module IO =
    open Fake.IO
    open Newtonsoft.Json

    let serialize<'a> (value: 'a) =
        JsonConvert.SerializeObject(value, Formatting.Indented)

    let deserialize<'a> (str: string) =
        try JsonConvert.DeserializeObject<'a>(str) |> Some
        with _ -> None

    let stateFileName = "shortify.state"
    let readState<'a>() = 
        File.create stateFileName
        File.readAsString stateFileName
        |> deserialize<'a>

    let writeState<'a> (state: 'a) =
        serialize state
        |> File.writeString false stateFileName

module App =
    open IO
    open Repl

    [<EntryPoint>]
    let main argv =
        printfn "Welcome to Shortify :)"
        printfn "I will observe your work proactively suggest"
        printfn " - Shortcuts to commonly used commands and batches of commands"
        printfn " - Solutions to errors you encounter"
    
        let mutable state = readState<State>() |> Option.defaultValue State.empty
        while true do
            state <- repl state
            writeState state
    
        0
