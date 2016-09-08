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

let correctLinks: Story -> Story = fun story ->
    if not (story.Address.StartsWith "http") then
        story.Address <- "https://www.reddit.com" + story.Address
    story
        
let getRedditStories(subReddit: string, minimumScore: int) = 
    let stories =  downloadRedditPage subReddit
                |> parsePage
                |> extractRedditStories
                |> Seq.map correctLinks
                |> Seq.where (fun story -> story.Metadata.Points >= minimumScore)
    stories

type RedditStoryGrabber(minimumScore: int, ?subReddit: string) =
    

    member this.GetStories() = (this :> IStoryGrabber).GetStories()

    interface IStoryGrabber with
        member this.GetStories() = 
            let subRedditToUse = defaultArg subReddit "programming"
            getRedditStories(subRedditToUse,minimumScore)