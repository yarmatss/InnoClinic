import { httpClient } from "../../api/http";
import type { Specialization } from "./types";
import type { PagedResponse } from "../../common/types/pagination";

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
