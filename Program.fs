﻿namespace Shortify

open System
open Fake.Core
open Fake.IO
open System.Text.Json

[<AutoOpen>]
module Model =
    type Command = 
        {
            Command: string
            Arguments: string list
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
            Commands: Command list
        }
        override this.ToString() = 
            this.Name
            + " runs:\n"
            + (
                this.Commands
                |> List.map (fun c -> c.ToString())
                |> String.concat "\n"
            )

    type State = {
        InvocationHistory: CommandInvocation list
        ActiveShortcuts: Shortcut list
    }

module Shortify = 
    // TODO: refactor to return batches of commands
    let topUsedCommands invocationHistory =
        let commandsWithUsageCount = 
            invocationHistory
            |> List.map (fun invocation -> invocation.Command)
            |> List.groupBy (fun command -> command.Command.ToString())
            |> List.map (fun (_, commands) -> List.length commands, commands.Head)
        let top3UsedCommands = 
            commandsWithUsageCount
            |> List.sortByDescending fst
            |> List.filter (fst >> ((<=) 1))
            |> List.truncate 3
        top3UsedCommands

module Repl =
    open Shortify

    let parse (commandString: string) = 
        match commandString.Split(" ") |> Array.toList with 
        | [] -> None
        | command::args ->
            Some {
                Command = command
                Arguments = args
            }

    let execute (command: Command) =
        CreateProcess.fromRawCommand command.Command command.Arguments
        |> Proc.run
        |> ignore

    let suggestCommandsShortcut state = 
        let shortcutCandidates = topUsedCommands state.InvocationHistory
        if shortcutCandidates.IsEmpty then 
            printfn "You are a genious who never repeats themselves! Congratuations! None to be done here"
            state
        else
            [
                for i, (usageCount, command) in shortcutCandidates |> List.indexed do
                printfn "Type the number the command you want to shortcut"
                printfn "%d : %A -- used %d times" i command usageCount
            ]
            |> ignore

            try 
                let selection = Console.ReadLine() |> int
                let _, command = List.item selection shortcutCandidates

                printf "Enter shortcut name: "
                let name = Console.ReadLine()
                let shortcut = {
                    Name = name
                    Commands = [command]
                }
                { state with ActiveShortcuts = shortcut::state.ActiveShortcuts }
            with _ -> state

    let suggestedScheduledRun = id
    let suggestArgumentShortcuts = id

    let suggest (state : State) : State =
        suggestCommandsShortcut state
        |> suggestArgumentShortcuts
        |> suggestedScheduledRun

    let recordInvocation invocation state = 
        { state with InvocationHistory = invocation::state.InvocationHistory }

    let repl (state : State) : State =
        printf "sh-orts> "

        match Console.ReadLine() with 
        | "exit" | "quit" -> 
            printfn "See you soon! Buyee :)"
            Environment.Exit(0)
            failwith "unreachable"
        | "suggest" -> 
            // TODO: split into suggestion stage and accepting
            suggest state
        | "list" ->
            let yourShortcuts = state.ActiveShortcuts
            if List.isEmpty yourShortcuts then
                printfn "Currently you dont have any shortcuts. You can add them with 'suggest' command"
            else
                printfn "Your shortcuts: "
                yourShortcuts |> List.iter (printfn "%A")
        
            state
        | unknownCommand ->
            match parse unknownCommand with
            | None -> state
            | Some command -> 
                let invocationTime = DateTime.Now
                execute command

                let invocation = {
                    Command = command
                    InvocationTime = invocationTime
                    CompletionTime = DateTime.Now
                }

                state
                |> recordInvocation invocation
            
                // TODO:
                //  - was it successfull? 
                //  - How long did it run?
                //  - Did any error occur? Want to google it or some custom search engine? :)

module IO =
    let serialize<'a> (value: 'a) =
        let options = JsonSerializerOptions()
        options.WriteIndented <- true
        JsonSerializer.Serialize(value, typeof<'a>, options)

    let deserialize<'a> (str: string) =
        let options = JsonSerializerOptions()
        try JsonSerializer.Deserialize<'a>(str, options) |> Some
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
        printfn "Use this as a regular shell, but we will keep track of want commands you run and when"
    
        let mutable state = 
            readState()
            |> Option.defaultValue { InvocationHistory = []; ActiveShortcuts = [] }
        while true do
            state <- repl state
            writeState state
    
        0
