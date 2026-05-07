namespace AutoReparos.Application.Shared
{
    public record PagedRequest
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = Math.Max(value, 1);
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = Math.Clamp(value, 1, MaxPageSize);
        }

        public int Skip => (PageNumber - 1) * PageSize;
    }
}