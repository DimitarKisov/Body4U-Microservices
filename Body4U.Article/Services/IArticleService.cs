using Body4U.Article.Models;
using Body4U.Common;

namespace Body4U.Article.Services
{
    public interface IArticleService
    {
        Result<int> Create(CreateArticleRequestModel request);
    }
}
