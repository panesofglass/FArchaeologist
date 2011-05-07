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

type Route = { 
  controller : string
  action : string
  id : UrlParameter }

type Global() =
  inherit System.Web.HttpApplication() 

  static member RegisterRoutes(routes:RouteCollection) =
    let connectionString = ConfigurationManager.ConnectionStrings.["mongodb"].ConnectionString

    // Create the application
    let app request = async {
      let message = AltNetMiner.getDiscussions(connectionString) |> JsonConvert.SerializeObject
      return "200 OK", dict [("Content-Type", "text/html")], Str message }

    // Set up the routes
    routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
    routes.MapFrackRoute("conversation_edges", app)
    routes.MapRoute("Default", "{*page}",
                    { controller = "Home"; action = "Index"; id = UrlParameter.Optional })

  member x.Start() =
    Global.RegisterRoutes(RouteTable.Routes)
