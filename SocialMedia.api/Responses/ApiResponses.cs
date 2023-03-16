using SocialMedia.core.CutomEntities;

namespace SocialMedia.api.Responses
{
    public class ApiResponses<T>
    {
        public T Data { get; set; }
        public Metadata Metadata { get; set; }
        public ApiResponses(T data) 
        { 
            Data = data;
        }

    }
}
