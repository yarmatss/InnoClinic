import { httpClient } from "@shared/api";
import type { PagedResponse } from "@shared/model";
import type { Specialization } from "../model/specialization";

export interface SpecializationsQuery {
  pageNumber?: number;
  pageSize?: number;
  name?: string;
  sortBy?: string;
  sortOrder?: string;
  signal?: AbortSignal;
}

export async function getSpecializations(query: SpecializationsQuery = {}) {
  const response = await httpClient.get<PagedResponse<Specialization>>(
    "/api/specializations",
    {
      params: {
        pageNumber: query.pageNumber ?? 1,
        pageSize: query.pageSize ?? 10,
        name: query.name,
        sortBy: query.sortBy,
        sortOrder: query.sortOrder,
      },
      signal: query.signal,
    },
  );

  return response.data;
}
