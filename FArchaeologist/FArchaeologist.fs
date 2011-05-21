module FArchaeologist

[<AutoOpen>]
module Operators =
  
  let inline (?) this key =
    ( ^a : (member get_Item : ^b -> ^c) (this,key))

  let inline (?<-) this key value =
    ( ^a : (member set_Item : ^b * ^c -> ^d) (this,key,value))

module AltNetMiner =
  open System
  open System.Collections.Generic
  open Microsoft.FSharp.Math
  open MongoDB

  /// Retrieve the discussions from the Mongo database.
  let getDiscussions (connectionString:string) =

    use mongo = new Mongo(connectionString)
    mongo.Connect()
  
    // Retrieve the discussions from the Mongo db
    mongo?AltNetMiner?AltNetSeattleDiscussions.Linq()
    |> Seq.toArray

  /// Get the mentions for each sender.
  let getSenderMentions discussions =

    let formatDiscussion (d:Document) =
      let sender = unbox<string>(d?sender).TrimStart('@').TrimEnd('!')
      match d?mentions with
      // Filter out documents with no mentions
      | :? List<string> -> (sender, unbox<List<string>>(d?mentions) |> Seq.toArray)
      | :? List<obj> -> (sender, unbox<List<obj>>(d?mentions) |> Seq.map string |> Seq.toArray)
      | _ -> (sender, List<string>() |> Seq.toArray)
  
    let createMention (name, mentions) =
      let cleanMentions =
        mentions
        |> Array.filter (not << String.IsNullOrEmpty)
        |> Array.filter (not << ((=) name))
      (name, cleanMentions)

    discussions
    |> Array.map (formatDiscussion >> createMention)

  /// Find the tweeps that have a mention in the data set.
  let getNodes (discussions: (string * #seq<string>)[]) maxGroupLevel =

    let createNode index (name, count) =
      dict [| ("name", name |> box)
              ("group", Math.Floor((decimal count)/10M) + 1M |> int |> box)
              ("id", index |> box) |]

    let onlyMaxGroupLevel (node:IDictionary<_,_>) = unbox<int>(node?group) <= maxGroupLevel

    discussions
    |> Seq.map snd
    |> Seq.collect id
    |> Seq.countBy id
    |> Seq.mapi createNode
    |> Seq.filter onlyMaxGroupLevel
    |> Seq.toArray

  /// Get the link relationships between each sender.
  let getLinks (discussions:(string * #seq<string>)[]) (nodes:IDictionary<string,obj>[]) =

    let isMentioned (sender, _) =
      nodes
      |> Array.exists (fun node -> unbox<string>(node?name) = sender)

    let findNameIndex name =
      nodes
      |> Seq.tryFind (fun d -> unbox<string>(d?name) = name)
      |> Option.map (fun d -> unbox<int>(d?id))

    let createLink (sender, mentions) =
      mentions
      |> Seq.choose (fun mention ->
          findNameIndex sender |> Option.bind (fun source ->
          findNameIndex mention |> Option.bind (fun target ->
          Some <| dict [| ("source", source)
                          ("target", target)
                          ("value", 1) |])))

    // Filter the results to only those with a mention.
    discussions
    |> Seq.filter isMentioned
    |> Seq.map createLink
    |> Seq.collect id
    |> Seq.distinct
    |> Seq.filter (not << Seq.isEmpty)
    |> Seq.toArray