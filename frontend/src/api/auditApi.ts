import { apiClient } from './apiClient';
import { PaginatedList } from './productApi';

export interface AuditLogDto {
  id: number;
  tableName: string;
  action: string;
  userId: string | null;
  timestampUtc: string;
  oldValues: string | null;
  newValues: string | null;
  primaryKey: string | null;
}

export interface GetAuditLogsParams {
  pageIndex?: number;
  pageSize?: number;
  tableName?: string;
  action?: string;
}

export const auditApi = {
  getAuditLogs: async (params: GetAuditLogsParams = {}): Promise<PaginatedList<AuditLogDto>> => {
    const response = await apiClient.get<PaginatedList<AuditLogDto>>('/AuditLogs', { params });
    return response.data;
  }
};
