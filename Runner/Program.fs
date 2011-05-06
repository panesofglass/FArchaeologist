module Program

open System.Linq
open FArchaeologist
open MongoDB

let config = {
  Host = "flame.mongohq.com"
  Port = 27018
  Username = "darkxanthos"
  Password = "abc123!" }

AltNetMiner.getDiscussions(config).Linq().AsEnumerable()
  |> Seq.countBy (fun d -> d?sender)
  |> Seq.iter (printfn "%A")