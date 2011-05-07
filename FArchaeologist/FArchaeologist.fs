module FArchaeologist

[<AutoOpen>]
module Dynamic =
  
  let inline (?) this key =
    ( ^a : (member get_Item : ^b -> ^c) (this,key))

  let inline (?<-) this key value =
    ( ^a : (member set_Item : ^b * ^c -> ^d) (this,key,value))

module AltNetMiner =
  open System.Collections.Generic
  open System.Linq
  open MongoDB

  let getDiscussions (connectionString:string) =

    use mongo = new Mongo(connectionString)
    mongo.Connect()
  
    // Retrieve the discussions from the Mongo db
    let discussions = mongo?AltNetMiner?AltNetSeattleDiscussions.Linq().ToArray()

    // Find the tweeps that have a mention in the data set.
    let mentionedTweeps =
      discussions
      |> Array.choose (fun d ->
          match d?mentions with
          | :? List<string> -> Some(unbox<List<string>>(d?mentions))
          | _ -> None)
      |> Array.collect (Seq.toArray)

    // Filter the results to only those with a mention.
    discussions |> Array.filter (fun d -> mentionedTweeps.Contains(unbox<string>(d?sender)))

