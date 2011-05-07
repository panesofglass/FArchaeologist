module Program

open System.Configuration
open System.Linq
open FArchaeologist
open FArchaeologist.AltNetMiner
open MongoDB

let connectionString = ConfigurationManager.ConnectionStrings.["mongodb"].ConnectionString

let discussions = getDiscussions connectionString
let senderMentions = getSenderMentions discussions
let nodes = getNodes senderMentions
let links = getLinks senderMentions nodes

//discussions |> Seq.iter (printfn "%A")
nodes |> Seq.iter (printfn "%A")
links |> Seq.iter (printfn "%A")
System.Console.ReadLine() |> ignore