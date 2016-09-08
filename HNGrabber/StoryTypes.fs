module StoryTypes

type StoryMetadata(points: int, commentCount: int, user: string, commentLink: string) =
    member x.Points = points
    member x.CommentCount = commentCount
    member x.CommentLink = commentLink
    member x.User = user

type Story(title: string, address: string, metadata: StoryMetadata) =
    member val Address = address with get, set
    member x.Title = title   
    member x.Metadata = metadata

type IStoryGrabber = 
    abstract member GetStories : unit -> seq<Story>
