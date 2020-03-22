[<AutoOpen>]
module RaynMaker.Portfolio.Service.Core

open System.IO
open System.Reflection

let getHome () =
    let location = Assembly.GetExecutingAssembly().Location
    location |> Path.GetDirectoryName |> Path.GetFullPath

