namespace FSharpMVC2.Core

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Routing
open FArchaeologist
open Frack
open Frack.Hosting.AspNet
open MongoDB
open Newtonsoft.Json

type Global() =
  inherit System.Web.HttpApplication() 

  static member RegisterRoutes(routes:RouteCollection) =
    // Echo the request body contents back to the sender. 
    // Use Fiddler to post a message and see it return.
    let app (request: IDictionary<string, obj>) = async {
      let config = {
        Host = "flame.mongohq.com"
        Port = 27018
        Username = "darkxanthos"
        Password = "abc123!" }
      
      let message = AltNetMiner.getDiscussions(config).Linq().ToArray() |> JsonConvert.SerializeObject

      return "200 OK", dict [("Content-Type", "text/html")], Str message }

    // Uses the head middleware.
    // Try using Fiddler and perform a HEAD request.
    routes.MapFrackRoute("conversation_edges", app)

  member x.Start() =
    Global.RegisterRoutes(RouteTable.Routes)
