module FArchaeologist

[<AutoOpen>]
module Dynamic =
  let inline (?) this key =
    ( ^a : (member get_Item : ^b -> ^c) (this,key))
  
  let inline (?<-) this key value =
    ( ^a : (member set_Item : ^b * ^c -> ^d) (this,key,value))

type MongoConfig = {
  Host : string
  Port : int
  Username : string
  Password : string }
  with member x.ConnectionString = sprintf "mongodb://%s:%s@%s:%d" x.Username x.Password x.Host x.Port

module AltNetMiner =
  open MongoDB

  let connect (config: MongoConfig) =
    let mongo = new Mongo(config.ConnectionString)
    mongo.Connect()
    mongo

  let getDiscussions config =
    let mongo = connect config
    mongo?AltNetMiner?AltNetSeattleDiscussions

