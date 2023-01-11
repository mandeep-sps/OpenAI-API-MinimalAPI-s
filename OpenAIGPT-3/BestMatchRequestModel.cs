namespace OpenAIGPT_3
{
    public class BestMatchRequestModel
    {
        public string? Text { get; set; }

        public string? Username { get; set; }

    }

    public class CreateCompletionModel
    {
        public string? Text { get; set; }

    }

    public class FetchModelData
    {
        public string? ID { get; set;}

        public string? Owner { get; set;}

        public string? Text { get; set;}
    }
}
