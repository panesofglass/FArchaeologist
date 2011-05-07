module Program

open System.Configuration
open System.Linq
open FArchaeologist
open MongoDB

let connectionString = ConfigurationManager.ConnectionStrings.["mongodb"].ConnectionString

AltNetMiner.getDiscussions(connectionString)
  |> Seq.countBy (fun d -> d?sender)
  |> Seq.iter (printfn "%A")