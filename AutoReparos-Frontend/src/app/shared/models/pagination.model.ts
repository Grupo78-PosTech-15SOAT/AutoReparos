export interface PagedResult<T> {
  items: T[];
  total: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
