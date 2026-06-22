import { httpClient } from '../../api/http'
import type { PagedResponse, Specialization } from './types'

export interface SpecializationsQuery {
  pageNumber?: number
  pageSize?: number
  name?: string
  signal?: AbortSignal
}

export async function getSpecializations(query: SpecializationsQuery = {}) {
  const response = await httpClient.get<PagedResponse<Specialization>>(
    '/api/specializations',
    {
      params: {
        pageNumber: query.pageNumber ?? 1,
        pageSize: query.pageSize ?? 10,
        name: query.name,
      },
      signal: query.signal,
    },
  )

  return response.data
}
