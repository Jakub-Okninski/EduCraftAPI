using NPOI.SS.Formula.Functions;

namespace EduCraftAPI.Models
{
    public class PaginatedResult
    {
        public int TotalCount { get; set; }
        public List<SearchDto> Items { get; set; }
    }
}
