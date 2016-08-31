module Common

open System.Net

open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack
open Newtonsoft.Json;
open Newtonsoft.Json.Serialization

open StoryTypes

let downloadPage (url: string): string = 
    use client = new WebClient()
    client.DownloadString url

(* parse the string containing the HN front page into 
 * a document we can then extract the stories out of *)
let parsePage (htmlString: string): HtmlNode =
    let document = HtmlDocument()
    document.LoadHtml htmlString
    document.DocumentNode

(* Extract digits from beginning of string and convert to an integer *)
let extractNumber (strWithNum: string) = 
    try
        strWithNum |> Seq.takeWhile System.Char.IsDigit
                    |> System.String.Concat
                    |> int
    with
        | :? System.FormatException -> 0

(* Convert the sequence of HN stories to a JSON array with camel cased property names. *)
let convertToJson (stories: seq<Story>): string =
    let settings = JsonSerializerSettings( ContractResolver = CamelCasePropertyNamesContractResolver() )
    JsonConvert.SerializeObject(stories, Formatting.Indented, settings)