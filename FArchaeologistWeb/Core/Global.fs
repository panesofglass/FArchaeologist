namespace FSharpMVC2.Core

open System
open System.Collections.Generic
open System.Configuration
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Routing
open FArchaeologist
open Frack
open Frack.Hosting.AspNet
open MongoDB
open Newtonsoft.Json

[<HandleError>]
type HomeController() =
  inherit Controller()
  member x.Index() = x.View() :> ActionResult

[<HandleError>]
type ConversationsController() =
  inherit Controller()
  member x.Index() = x.View() :> ActionResult

type Route = { 
  controller : string
  action : string
  id : UrlParameter }

type Global() =
  inherit System.Web.HttpApplication() 

  static member RegisterRoutes(routes:RouteCollection) =
    let connectionString = ConfigurationManager.ConnectionStrings.["mongodb"].ConnectionString

    let discussions = AltNetMiner.getDiscussions connectionString
    let senderMentions = AltNetMiner.getSenderMentions discussions
    let nodes = AltNetMiner.getNodes senderMentions
    let links = AltNetMiner.getLinks senderMentions nodes

    let tweets request = async {
      let message = discussions |> JsonConvert.SerializeObject
      return "200 OK", dict [("Content-Type", "text/html")], Str message }

    // Create the application
    let conversation_edges request = async {
      let message = [| ("nodes", box nodes)
                       ("links", box links) |]
                    |> dict
                    |> JsonConvert.SerializeObject
      return "200 OK", dict [("Content-Type", "text/html")], Str message }

    // Set up the routes
    routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
    routes.MapFrackRoute("tweets", tweets)
    routes.MapFrackRoute("conversation_edges", conversation_edges)
    routes.MapRoute("Conversations", "conversations",
                    { controller = "Conversations"; action = "Index"; id =UrlParameter.Optional }) |> ignore
    routes.MapRoute("Default", "{controller}/{action}/{id}",
                    { controller = "Home"; action = "Index"; id = UrlParameter.Optional }) |> ignore

  member x.Start() =
    Global.RegisterRoutes(RouteTable.Routes)
