// Matches the BE response envelope (ResultViewModel<T>) — PascalCase, same as ECM's API_RES<T>.
export interface ApiResponse<T> {
  Data: T;
  IsSuccess: boolean;
  Message?: string;
  Total: number;
  PageNumber: number;
  PageSize: number;
}

// Matches BE QueryViewModel<TFilter, TSort> for list endpoints (Trips/Search, etc.).
// SortType: 0 = Asc, 1 = Desc. FieldName is the sort-enum's numeric value.
export interface OrderModel {
  fieldName?: number | string;
  sortType: number;
}

export interface QueryModel<TFilter> {
  filterModel?: TFilter;
  orderModel?: OrderModel;
  pageNumber: number;
  pageSize: number;
}
