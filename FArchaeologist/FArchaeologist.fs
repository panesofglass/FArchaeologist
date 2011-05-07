module FArchaeologist

[<AutoOpen>]
module Dynamic =
  
  let inline (?) this key =
    ( ^a : (member get_Item : ^b -> ^c) (this,key))

  let inline (?<-) this key value =
    ( ^a : (member set_Item : ^b * ^c -> ^d) (this,key,value))

module AltNetMiner =
  open System
  open System.Collections.Generic
  open System.Linq
  open Microsoft.FSharp.Math
  open MongoDB

  let getDiscussions (connectionString:string) =

    use mongo = new Mongo(connectionString)
    mongo.Connect()
  
    // Retrieve the discussions from the Mongo db
    mongo?AltNetMiner?AltNetSeattleDiscussions.Linq().ToArray()

  let getSenderMentions (discussions: Document[]) =
    discussions
    |> Array.choose (fun d ->
        match d?mentions with
        // Filter out documents with no mentions
        | :? List<string> -> Some(unbox<string>(d?sender), unbox<List<string>>(d?mentions))
        | :? List<obj> -> Some(unbox<string>(d?sender), unbox<List<obj>>(d?mentions).Select(Func<_,_>(string)).ToList())
        | _ -> Some(unbox<string>(d?sender), List<string>()))
    |> Array.map (fun (name, mentions) -> (name.TrimStart('@'), mentions))

  // Find the tweeps that have a mention in the data set.
  let getNodes (discussions: (string * #seq<string>)[]) =
    discussions
    |> Seq.map snd
    // Flatten the sequence
    |> Seq.collect id
    // Group by and count the names
    |> Seq.countBy id
    // Convert the counts to mention groups
    |> Seq.mapi (fun idx (name, count) ->
                    [| ("name", name |> box)
                       ("group", Math.Floor((decimal count)/10M) + 1M |> int |> box)
                       ("id", idx |> box) |]
                    |> dict)

  let getLinks (discussions:(string * #seq<string>)[]) (mentionGroups:IDictionary<string,obj> seq) =
    // Filter the results to only those with a mention.
    discussions
    |> Seq.filter (fun (sender, _) ->
        mentionGroups
        |> Seq.map (fun g -> unbox<string>(g?name))
        |> Seq.exists (fun name -> name = sender))
    |> Seq.map (fun (sender, mentions) ->
        let findNameIndex name =
          mentionGroups
          |> Seq.find (fun d -> unbox<string>(d?name) = name)
          |> fun d -> d?id
        mentions
        |> Seq.collect (fun mention ->
            [| ("source", findNameIndex sender)
               ("target", findNameIndex mention)
               ("value", box 1) |]))
    |> Seq.filter (not << Seq.isEmpty)
    |> Seq.map dict

