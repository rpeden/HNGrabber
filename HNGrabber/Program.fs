// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System.Net
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack
open Newtonsoft.Json;


type HNStory(title: string, address: string) =
    member x.Title = title
    member x.Address = address


let downloadFrontPage = 
    use client = new WebClient()
    client.DownloadString "https://news.ycombinator.com"


let parsePage (htmlString: string) =
    let document = HtmlDocument()
    document.LoadHtml htmlString
    document.DocumentNode


let extractStories (document: HtmlNode) =

    let buildStory (storyNode: HtmlNode) = 
        (* handle the case where the element isn't formed the way
           we expect it to be *)
        try
            let story = storyNode.QuerySelector(".title a")
            let address = story.Attributes.["href"].Value
            let title = story.InnerText
            Some( HNStory(title, address) )
        with
            | :? System.DivideByZeroException -> None

    document.QuerySelectorAll(".athing")
          |> Seq.cast<HtmlNode>
          |> Seq.choose buildStory
          |> Seq.cache

[<EntryPoint>]
let main argv =
    let stories =  downloadFrontPage
                |> parsePage
                |> extractStories
                |> JsonConvert.SerializeObject
    printfn "%s" stories
    let x = System.Console.ReadLine() 
    0 // return an integer exit code




