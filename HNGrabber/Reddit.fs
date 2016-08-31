module Reddit

open Common
open StoryTypes

open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack


let downloadRedditPage(subreddit: string) = 
    downloadPage ("https://reddit.com/r/" + subreddit)

let extractStoryMetadata (story: HtmlNode): StoryMetadata = 
    let points = extractNumber (story.QuerySelector(".likes").InnerText)

    let entry = story.QuerySelector(".entry")
    let submitter = entry.QuerySelector(".tagline").QuerySelector(".author").InnerText
    let commentAnchor = entry.QuerySelector(".comments")
    let commentLink = commentAnchor.GetAttributeValue("href", "")
    let commentCount = extractNumber (commentAnchor.InnerText)

    StoryMetadata(points, commentCount, submitter, commentLink) 

let extractStory (story: HtmlNode): Story = 
    let metadata = extractStoryMetadata story
    let titleAnchor = story.QuerySelector(".title").QuerySelector(".title")
    let titleText = titleAnchor.InnerText
    let titleLink = titleAnchor.GetAttributeValue("href", "")

    Story(titleText, titleLink, metadata)

let extractRedditStories (document: HtmlNode): seq<Story> = 
    let stories = document.QuerySelectorAll(".link")
                  |> Seq.map extractStory
    stories

let getRedditStories() = 
    let redditPage = parsePage (downloadRedditPage "programming")

    let stories =  downloadRedditPage "programming"
                |> parsePage
                |> extractRedditStories
                |> Seq.where(fun story -> story.Metadata.Points > 50)
                |> convertToJson
    stories