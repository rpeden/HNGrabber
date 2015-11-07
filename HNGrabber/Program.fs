open System.Net
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack
open Newtonsoft.Json;
open Newtonsoft.Json.Serialization

type HNStoryMetadata(points: int, commentCount: int, user: string, commentLink: string) =
    member x.Points = points
    member x.CommentCount = commentCount
    member x.CommentLink = commentLink
    member x.User = user

type HNStory(title: string, address: string, metadata: HNStoryMetadata) =
    member x.Title = title
    member x.Address = address
    member x.Metadata = metadata

let downloadFrontPage = 
    use client = new WebClient()
    client.DownloadString "https://news.ycombinator.com"

let parsePage (htmlString: string) =
    let document = HtmlDocument()
    document.LoadHtml htmlString
    document.DocumentNode

let extractMetadata (metadata: HtmlNode) = 
    (* Extract digits from beginning of string and convert to an integer *)
    let extractNumber (strWithNum: string) = 
        try
            strWithNum |> Seq.takeWhile System.Char.IsDigit
                       |> System.String.Concat
                       |> int
        with
            | :? System.FormatException -> 0
    
    let points = extractNumber (metadata.QuerySelector(".score").InnerText)
    let commentAnchor = Seq.last (metadata.QuerySelectorAll("a[href^=item]"))
    let commentCount = extractNumber commentAnchor.InnerText
    let commentLink = commentAnchor.Attributes.["href"].Value
    let user = metadata.QuerySelector("a[href^=user]").InnerText

    HNStoryMetadata(points,commentCount,user,commentLink)

let extractStories (document: HtmlNode) =

    let buildStory (storyNode: HtmlNode) = 
        (* handle the case where the element isn't formed the way
           we expect it to be *)
        try
            let story = storyNode.QuerySelector(".title a")
            let metadata = extractMetadata storyNode.NextSibling;
            let address = story.Attributes.["href"].Value
            let title = story.InnerText
            Some( HNStory(title, address, metadata) )
        with
            | :? System.NullReferenceException -> None

    document.QuerySelectorAll(".athing")
          |> Seq.cast<HtmlNode>
          |> Seq.choose buildStory
          |> Seq.cache

let convertToJson (stories: seq<HNStory>) =
    let settings = JsonSerializerSettings( ContractResolver = CamelCasePropertyNamesContractResolver() )
    JsonConvert.SerializeObject(stories, Formatting.Indented, settings)

[<EntryPoint>]
let main argv =
    let stories =  downloadFrontPage
                |> parsePage
                |> extractStories
                |> convertToJson
    printfn "%s" stories
    let x = System.Console.ReadLine() 
    0 // return an integer exit code