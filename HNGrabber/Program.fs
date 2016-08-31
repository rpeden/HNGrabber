module Program

open System.Net
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

open StoryTypes
open Common


let downloadHNFrontPage: string =
    downloadPage "https://news.ycombinator.com"

(* extract metadata (point, comment count, username of submitter, and 
 * link to comment page) for a single HN story *)
let extractMetadata (metadata: HtmlNode): StoryMetadata = 
    let points = extractNumber (metadata.QuerySelector(".score").InnerText)
    let commentAnchor = Seq.last (metadata.QuerySelectorAll("a[href^=item]"))
    let commentCount = extractNumber commentAnchor.InnerText
    let commentLink = commentAnchor.Attributes.["href"].Value
    let user = metadata.QuerySelector("a[href^=user]").InnerText

    StoryMetadata(points,commentCount,user,commentLink)

(* Extract all stories from the HN front page *)
let extractStories (document: HtmlNode): seq<Story> =

    (* Given a story node, build a Story instance *)
    let buildStory (storyNode: HtmlNode): Story option = 
        try
            let story = storyNode.QuerySelector(".title a")
            let metadata = extractMetadata storyNode.NextSibling;
            let address = story.Attributes.["href"].Value
            let title = story.InnerText
            Some( Story(title, address, metadata) )
        with
            (* Handle the exception that occurs if one of the elements we're expecting to find 
             * is missing. *)
            | :? System.NullReferenceException -> None

    document.QuerySelectorAll(".athing")
          |> Seq.cast<HtmlNode>
          |> Seq.choose buildStory
          |> Seq.cache

open Reddit

[<EntryPoint>]
let main argv =
    let stories =  downloadHNFrontPage
                |> parsePage
                |> extractStories
                |> Seq.where(fun story -> story.Metadata.Points > 50)
                |> convertToJson

    

    let redditStories = getRedditStories()

    //printfn "%s" stories
    printfn "%s" redditStories
    let x = System.Console.ReadLine() 
    0 // return an integer exit code