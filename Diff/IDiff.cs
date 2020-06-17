using System.Threading.Tasks;

namespace Blazorme
{
    public interface IDiff
    {
        public Task<string> GetAsync(string firstInput, string secondInput, 
            string firstTitle = DiffInputTitle.First, string secondTitle = DiffInputTitle.Second);

        public Task<string> GetHtmlAsync(string firstInput, string secondInput,
            string firstTitle = DiffInputTitle.First, string secondTitle = DiffInputTitle.Second,
            DiffOutputFormat outputFormat = DiffOutputFormat.Inline,
            DiffStyle style = DiffStyle.Word);
    }
}
