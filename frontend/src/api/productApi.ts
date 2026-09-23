import { apiClient } from './apiClient';

export interface ProductSummaryDto {
  productId: number;
  productName: string;
  categoryId: number | null;
  categoryName: string | null;
  unitPrice: number | null;
  unitsInStock: number | null;
  discontinued: boolean;
  quantityPerUnit: string | null;
}

export interface PaginatedList<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CategoryDetailDto {
  categoryId: number;
  categoryName: string;
  description: string | null;
  picture: string | null;
  pictureBase64: string | null;
}

export interface ProductDetailDto {
  productId: number;
  productName: string;
  quantityPerUnit: string | null;
  unitPrice: number | null;
  unitsInStock: number | null;
  unitsOnOrder: number | null;
  reorderLevel: number | null;
  discontinued: boolean;
  category: CategoryDetailDto | null;
  supplierId: number | null;
  supplierName: string | null;
}

export interface BulkCreateProductsResult {
  batchId?: string;
  totalProcessed: number;
  successfulImports?: number;
  failedImports?: number;
  elapsedMilliseconds?: number;
  status?: string;
  message?: string;
  enqueuedAtUtc?: string;
  errors?: string[];
}

export interface BulkCreateProductItemDto {
  productName: string;
  categoryId?: number;
  supplierId?: number;
  quantityPerUnit?: string;
  unitPrice?: number;
  unitsInStock?: number;
  discontinued?: boolean;
}

export interface GetProductsParams {
  pageIndex?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  discontinued?: boolean;
  sortBy?: string;
  sortOrder?: string;
}

export const productApi = {
  getProducts: async (params: GetProductsParams): Promise<PaginatedList<ProductSummaryDto>> => {
    const response = await apiClient.get<PaginatedList<ProductSummaryDto>>('/Products', { params });
    return response.data;
  },

  getProductById: async (id: number): Promise<ProductDetailDto> => {
    const response = await apiClient.get<ProductDetailDto>(`/Products/${id}`);
    return response.data;
  },

  bulkCreateProducts: async (command: {
    products?: BulkCreateProductItemDto[];
    generateRandomCount?: number;
    batchSize?: number;
  }): Promise<BulkCreateProductsResult> => {
    const response = await apiClient.post<BulkCreateProductsResult>('/Product', command);
    return response.data;
  },

  createCategory: async (category: {
    categoryName: string;
    description?: string;
    picture?: string;
  }): Promise<{ categoryId: number; message: string }> => {
    const response = await apiClient.post<{ categoryId: number; message: string }>('/Category', category);
    return response.data;
  }
};
