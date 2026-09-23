import { apiClient } from './apiClient';

export interface CategoryDto {
  categoryId: number;
  categoryName: string;
  description: string | null;
  pictureBase64: string | null;
  productCount: number;
}

export interface CreateCategoryRequest {
  categoryName: string;
  description?: string;
  picture?: string;
}

export interface UpdateCategoryRequest {
  categoryId: number;
  categoryName: string;
  description?: string;
}

export const categoryApi = {
  getCategories: async (): Promise<CategoryDto[]> => {
    const response = await apiClient.get<CategoryDto[]>('/Category');
    return response.data;
  },

  createCategory: async (request: CreateCategoryRequest): Promise<{ categoryId: number; message: string }> => {
    const response = await apiClient.post<{ categoryId: number; message: string }>('/Category', request);
    return response.data;
  },

  updateCategory: async (id: number, request: UpdateCategoryRequest): Promise<void> => {
    await apiClient.put(`/Category/${id}`, request);
  },

  deleteCategory: async (id: number): Promise<void> => {
    await apiClient.delete(`/Category/${id}`);
  }
};
