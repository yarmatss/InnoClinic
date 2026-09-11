export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  code: string;
  traceId: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
